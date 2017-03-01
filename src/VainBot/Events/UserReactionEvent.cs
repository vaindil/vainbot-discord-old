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

            // 132714099241910273

            if (msg.Author.Id == 149716909556891648)
            {
                if (_rng.Next(16) != 1)
                    return;

                await msg.AddReactionAsync("🇩");
                await msg.AddReactionAsync("🇴");
                await msg.AddReactionAsync("🇬");
                await msg.AddReactionAsync("FrankerZ:269238998482944000");
                await msg.AddReactionAsync("🇧");
                await msg.AddReactionAsync("🇦");
                await msg.AddReactionAsync("🇾");
            }
        }
    }
}
