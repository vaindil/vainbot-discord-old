using Discord.WebSocket;
using System.Threading.Tasks;

namespace VainBotDiscord.Events
{
    public static class UserReactionEvent
    {
        public static async Task AddReactionToUserAsync(SocketMessage inMsg)
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
    }
}
