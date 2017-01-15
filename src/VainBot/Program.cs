using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
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

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, apiToken);

            await client.ConnectAsync();

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
