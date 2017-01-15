using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace VainBot
{
    public class Program
    {
        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        DiscordSocketClient client;

        public async Task Run()
        {
            client = new DiscordSocketClient();

            var token = "MTYyMzU2Mzk4ODU0ODk3NjY0.C1x6Ug.vjYR0YEXsi40NtVfi6QnDCl7DYE";

            client.MessageReceived += async (message) =>
            {
                if (message.Content == "!ping")
                    await message.Channel.SendMessageAsync("pong");
            };

            await client.LoginAsync(TokenType.Bot, token);

            await client.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}
