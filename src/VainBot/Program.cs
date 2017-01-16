using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;

namespace VainBot
{
    public class Program
    {
        DiscordSocketClient client;
        CommandService commands;
        DependencyMap map;

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

            map = new DependencyMap();
            map.Add(client);
            map.Add(commands);
            map.Add(new VbContext());

            client.UserLeft += UserLeaves;

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, apiToken);

            await client.ConnectAsync();

            await Task.Delay(-1);
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
