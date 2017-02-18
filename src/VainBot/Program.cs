using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using VainBotDiscord.Events;
using VainBotDiscord.Twitch;

// vaindil: 132714099241910273

namespace VainBotDiscord
{
    public class Program
    {
        DiscordSocketClient client;
        CommandService commands;
        DependencyMap map;
        static HttpClient httpClient;

        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();
        
        public async Task Run()
        {
            var isDev = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VAINBOT_ISDEV"));

            if (!Directory.Exists("TTSTemp"))
                Directory.CreateDirectory("TTSTemp");

            var apiToken = await DbInit.InitDbAsync();
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentNullException(nameof(apiToken), "Discord API token not found");

            var twitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");
            if (twitchClientId == null)
                throw new ArgumentNullException(nameof(twitchClientId), "Twitch Client ID env var not found");

            var clientConfig = new DiscordSocketConfig
            {
                AudioMode = AudioMode.Outgoing,
                LogLevel = isDev ? LogSeverity.Info : LogSeverity.Warning
            };

            client = new DiscordSocketClient(clientConfig);

            commands = new CommandService();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VainBot/1.0");

            map = new DependencyMap();
            map.Add(client);
            map.Add(commands);
            map.Add(httpClient);
            map.Add(new VbContext());
            map.Add(new Random());

            //client.MessageReceived += UserReactionEvent.AddReactionToUserAsync;
            client.MessageReceived += LolCounterEvent.LolCounterAsync;
            client.UserLeft += UserLeavesEvent.UserLeavesAsync;
            
            client.Log += (message) =>
            {
                Console.WriteLine($"{message.ToString()}");
                return Task.CompletedTask;
            };

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, apiToken);
            await client.ConnectAsync();
            await client.SetGameAsync("Euro Truck Simulator 2018");

            var twitchSvc = new TwitchService(client);
            await twitchSvc.InitTwitchService();

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
            if (message == null)
                return;

            var argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
                return;

            var context = new CommandContext(client, message);

            var result = await commands.ExecuteAsync(context, argPos, map);
        }
    }
}
