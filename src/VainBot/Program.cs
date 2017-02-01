using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

// vaindil: 132714099241910273

namespace VainBot
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
            string apiToken;

            using (var db = new VbContext())
            {
                apiToken = await db.KeyValues.GetValueAsync(DbKey.DiscordApiKey);
            }

            client = new DiscordSocketClient();
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

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, apiToken);
            await client.ConnectAsync();

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
    }
}
