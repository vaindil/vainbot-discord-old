using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VainBotDiscord.Events;
using VainBotDiscord.Twitch;
using VainBotDiscord.Twitter;
using VainBotDiscord.Utils;
using VainBotDiscord.YouTube;

// vaindil: 132714099241910273

namespace VainBotDiscord
{
    public class Program
    {
        DiscordSocketClient client;
        CommandService commands;
        IServiceProvider services;
        static HttpClient httpClient;
        static List<ServerMainUser> mainUsers;
        static bool isDev;

        static void Main() => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            isDev = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VAINBOT_ISDEV"));

            if (!Directory.Exists("TTSTemp"))
                Directory.CreateDirectory("TTSTemp");

            var apiToken = await DbInit.InitDbAsync();
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentNullException(nameof(apiToken), "Discord API token not found");

            VerifyEnvironmentVariables();

            using (var db = new VbContext())
            {
                mainUsers = await db.ServerMainUsers.ToListAsync();
            }

            var clientConfig = new DiscordSocketConfig
            {
                LogLevel = isDev ? LogSeverity.Info : LogSeverity.Warning
            };

            client = new DiscordSocketClient(clientConfig);

            commands = new CommandService();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VainBot/1.0");

            TimeZoneInfo tz;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            else
                tz = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<VbContext>();
            serviceCollection.AddSingleton(client);
            serviceCollection.AddSingleton(httpClient);
            serviceCollection.AddSingleton(new ThrottlerService());
            serviceCollection.AddSingleton(new Random());
            serviceCollection.AddSingleton(tz);
            services = serviceCollection.BuildServiceProvider();

            //client.MessageReceived += UserReactionEvent.AddReactionToUserAsync;
            client.MessageReceived += LolCounterEvent.LolCounterAsync;
            client.MessageReceived += NoUCounterEvent.NoUCounterAsync;
            client.MessageReceived += DuckerZEvent.DuckerZAsync;
            client.UserLeft += UserLeavesEvent.UserLeavesAsync;
            client.Ready += async () => await client.SetGameAsync("something");

            var twitchCancelToken = new CancellationTokenSource();
            var youTubeCancelToken = new CancellationTokenSource();
            var twitterCancelToken = new CancellationTokenSource();

            client.Connected += async () =>
            {
                if (isDev)
                    return;

                twitchCancelToken = new CancellationTokenSource();
                youTubeCancelToken = new CancellationTokenSource();
                twitterCancelToken = new CancellationTokenSource();

                var twitchSvc = new TwitchService(client, tz, twitchCancelToken.Token);
                serviceCollection.AddSingleton(twitchSvc);
                services = serviceCollection.BuildServiceProvider();
                try
                {
                    await twitchSvc.InitTwitchServiceAsync();
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                        Console.WriteLine("Twitch service canceled");
                    else
                        Console.WriteLine("Twitch service unhandled exception");
                }
                finally
                {
                    twitchCancelToken.Dispose();
                }

                var youTubeSvc = new YouTubeService(client, tz, youTubeCancelToken.Token);
                serviceCollection.AddSingleton(youTubeSvc);
                services = serviceCollection.BuildServiceProvider();
                try
                {
                    await youTubeSvc.InitYouTubeServiceAsync();
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                        Console.WriteLine("YouTube service canceled");
                    else
                        Console.WriteLine("YouTube service unhandled exception");
                }
                finally
                {
                    youTubeCancelToken.Dispose();
                }

                var twitterSvc = new TwitterService(client, tz, twitterCancelToken.Token);
                serviceCollection.AddSingleton(twitterSvc);
                services = serviceCollection.BuildServiceProvider();
                try
                {
                    await twitterSvc.InitTwitterServiceAsync();
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                        Console.WriteLine("Twitter service canceled");
                    else
                        Console.WriteLine("Twitter service unhandled exception");
                }
                finally
                {
                    twitterCancelToken.Dispose();
                }
            };

            client.Disconnected += (ex) =>
            {
                twitchCancelToken.Cancel();
                youTubeCancelToken.Cancel();
                twitterCancelToken.Cancel();

                return Task.CompletedTask;
            };

            client.Log += (message) =>
            {
                Console.WriteLine($"{message.ToString()}");
                return Task.CompletedTask;
            };

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, apiToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null || message.Author.Id == client.CurrentUser.Id)
                return;

            if (isDev)
            {
                var appInfo = await client.GetApplicationInfoAsync();
                if (message.Author.Id != appInfo.Owner.Id)
                    return;
            }

            var argPos = 0;

            // HasStringPrefix needs to be first so that argPos is set correctly
            if (!(message.HasStringPrefix("! ", ref argPos)
                || message.HasCharPrefix('!', ref argPos)
                || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
                return;

            var guildId = (message.Channel as SocketGuildChannel).Guild.Id;
            var context = new VbCommandContext(client, message);

            if (mainUsers.Exists(m => (ulong)m.DiscordServerId == guildId))
                context.MainUser = mainUsers.Find(m => (ulong)m.DiscordServerId == guildId);

            var result = await commands.ExecuteAsync(context, argPos, services);
        }

        void VerifyEnvironmentVariables()
        {
            if (Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID") == null)
                throw new ArgumentNullException("Twitch Client ID env var not found", innerException: null);

            if (Environment.GetEnvironmentVariable("YOUTUBE_API_KEY") == null)
                throw new ArgumentNullException("YouTube API key env var not found", innerException: null);

            if (Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY") == null)
                throw new ArgumentNullException("Twitter consumer key env var not found", innerException: null);

            if (Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET") == null)
                throw new ArgumentNullException("Twitter consumer secret env var not found", innerException: null);

            if (Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN") == null)
                throw new ArgumentNullException("Twitter access token env var not found", innerException: null);

            if (Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN_SECRET") == null)
                throw new ArgumentNullException("Twitter access token secret env var not found", innerException: null);

            if (Environment.GetEnvironmentVariable("IMGUR_CLIENT_ID") == null)
                throw new ArgumentNullException("Imgur client ID env var not found", innerException: null);
        }
    }
}
