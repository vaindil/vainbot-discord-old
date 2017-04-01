using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace VainBotDiscord.Events
{
    public static class UserReactionEvent
    {
        static Random _rng = new Random();

        public static async Task AddReactionToUserAsync(SocketMessage inMsg)
        {
            var msg = inMsg as SocketUserMessage;
            
            if (msg.Author.Id == 149716909556891648)
            {
                if (_rng.Next(11) != 1)
                    return;

                var channel = msg.Channel as SocketGuildChannel;

                // destiny's server if true
                var frankerZString = channel.Guild.Id == 265256381437706240
                    ? "FrankerZ:271856531904004126"
                    : "FrankerZ:269238998482944000";

                await msg.AddReactionAsync("🇩");
                await msg.AddReactionAsync("🇴");
                await msg.AddReactionAsync("🇬");
                await msg.AddReactionAsync(frankerZString);
                await msg.AddReactionAsync("🇧");
                await msg.AddReactionAsync("🇦");
                await msg.AddReactionAsync("🇾");
            }
            else if (msg.Author.Id == 221480025025675265)
            {
                var r = _rng.Next(4);
                if (r == 1)
                {
                    await msg.AddReactionAsync("DaFeels:271856531572523010");
                    await msg.AddReactionAsync("🔫");
                }
                else if (r == 2)
                {
                    await msg.AddReactionAsync("PEPE:271856531845152771");
                }
            }
        }
    }
}
