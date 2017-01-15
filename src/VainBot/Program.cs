using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace VainBot
{
    public class Program
    {
        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        DiscordSocketClient client;

        public async Task Run()
        {
            string apiToken;

            using (var db = new VbContext())
            {
                apiToken = db.KeyValues.First(kv => kv.Key == DbKey.DiscordApiKey).Value;
            }

            client = new DiscordSocketClient();

            client.MessageReceived += async (message) =>
            {
                if (message.Content == "!ping")
                    await message.Channel.SendMessageAsync("pong");
            };

            await client.LoginAsync(TokenType.Bot, apiToken);

            await client.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}
