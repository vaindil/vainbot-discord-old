using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
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

            var apiToken = await DbInitAsync();
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

            //client.MessageReceived += AddReactionToUser;
            client.MessageReceived += LolCounter;
            client.UserLeft += UserLeaves;
            
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

        public async Task AddReactionToUser(SocketMessage inMsg)
        {
            var msg = inMsg as SocketUserMessage;

            // 132714099241910273
            
            if (msg.Author.Id == 110878826136907776)
            {
                await msg.AddReactionAsync("🇮");
                await msg.AddReactionAsync("🇸");
                await msg.AddReactionAsync("🇺");
                await msg.AddReactionAsync("🇨");
                await msg.AddReactionAsync("🇰");
                await msg.AddReactionAsync("LUL:232582021493424128");
            }
        }

        public async Task UserLeaves(SocketGuildUser user)
        {
            using (var db = new VbContext())
            {
                var target = await db.UserPoints
                    .FirstOrDefaultAsync(up => up.ServerId == user.Guild.Id && up.UserId == user.Id);
                if (target != null)
                    db.UserPoints.Remove(target);

                var targetAdmin = await db.Admins
                    .FirstOrDefaultAsync(a => a.ServerId == user.Guild.Id && a.UserId == user.Id);
                if (targetAdmin != null)
                    db.Admins.Remove(targetAdmin);

                await db.SaveChangesAsync();
            }
        }

        public async Task LolCounter(SocketMessage inMsg)
        {
            var msg = inMsg as SocketUserMessage;

            // 110878826136907776
            if (msg.Author.Id != 110878826136907776)
                return;

            var content = msg.Content.ToLower();
            if (content != "lol" && !content.Contains(" lol ") && !content.EndsWith(" lol") && !content.StartsWith("lol "))
                return;

            int count;

            using (var db = new VbContext())
            {
                var countKv = await db.KeyValues.FirstOrDefaultAsync(k => k.Key == DbKey.LolCounter.ToString());
                if (countKv == null)
                {
                    countKv = new KeyValue
                    {
                        Key = DbKey.LolCounter.ToString(),
                        Value = "0"
                    };

                    db.KeyValues.Add(countKv);
                    await db.SaveChangesAsync();
                }

                count = int.Parse(countKv.Value);
                count += 1;
                countKv.Value = count.ToString();

                await db.SaveChangesAsync();
            }

            await msg.AddReactionAsync("LUL:232582021493424128");

            if (count % 5 == 0)
                await msg.Channel.SendMessageAsync(msg.Author.Username + " lol counter: " + count);
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

        async Task<string> DbInitAsync()
        {
            using (var db = new VbContext())
            {
                var apiToken = await db.KeyValues.GetValueAsync(DbKey.DiscordApiKey);

                var bettingAllowedExists = await db.KeyValues
                    .FirstOrDefaultAsync(kv => kv.Key == DbKey.BettingAllowed.ToString());
                if (bettingAllowedExists == null)
                {
                    var bet = new KeyValue
                    {
                        Key = DbKey.BettingAllowed.ToString(),
                        Value = false.ToString()
                    };

                    db.KeyValues.Add(bet);
                    await db.SaveChangesAsync();
                }

                var lastTokenUpdateExists = await db.KeyValues
                    .FirstOrDefaultAsync(kv => kv.Key == DbKey.LastNaTokenUpdate.ToString());
                if (lastTokenUpdateExists == null)
                {
                    var dtNa = new KeyValue
                    {
                        Key = DbKey.LastNaTokenUpdate.ToString(),
                        Value = DateTime.UtcNow.AddMinutes(-20).ToString()
                    };

                    var dtEu = new KeyValue
                    {
                        Key = DbKey.LastEuTokenUpdate.ToString(),
                        Value = DateTime.UtcNow.AddMinutes(-20).ToString()
                    };

                    var eu = new KeyValue
                    {
                        Key = DbKey.LastEuTokenPrice.ToString(),
                        Value = "N/A"
                    };

                    var na = new KeyValue
                    {
                        Key = DbKey.LastNaTokenPrice.ToString(),
                        Value = "N/A"
                    };

                    db.KeyValues.Add(dtNa);
                    db.KeyValues.Add(dtEu);
                    db.KeyValues.Add(eu);
                    db.KeyValues.Add(na);

                    await db.SaveChangesAsync();
                }

                return apiToken;
            }
        }
    }
}
