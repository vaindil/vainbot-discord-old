using Discord;
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
            var guildId = (msg.Channel as SocketTextChannel).Guild.Id;
            
            if (msg.Author.Id == 149716909556891648 && guildId == 265256381437706240)
            {
                if (_rng.Next(51) != 1)
                    return;

                await msg.AddReactionAsync(new Emoji("🇮"));
                await msg.AddReactionAsync(new Emoji("🇲"));
                await msg.AddReactionAsync(new Emoji("🇬"));
                await msg.AddReactionAsync(new Emoji("🇦"));
                await msg.AddReactionAsync(new Emoji("🇾"));
                await msg.AddReactionAsync(Emote.Parse("<:gachiGASM:271856536282857472>"));
            }
        }
    }
}
