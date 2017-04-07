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
                if (_rng.Next(21) != 1)
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
        }
    }
}
