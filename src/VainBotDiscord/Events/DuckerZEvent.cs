using Discord.WebSocket;
using System.Threading.Tasks;

namespace VainBotDiscord.Events
{
    public static class DuckerZEvent
    {
        public static async Task DuckerZAsync(SocketMessage inMsg)
        {
            var msg = inMsg as SocketUserMessage;

            if (msg.Author.Id != 132714099241910273 || msg.Content != "got 'em")
                return;

            var guildId = ((SocketTextChannel)msg.Channel).Guild.Id;
            string emote;

            if (guildId == 265256381437706240) // Destiny
                emote = "<:DuckerZ:259886357596340224>";
            else if (guildId == 149051954348294145) // Crendor
                emote = "<:DuckerZ:259886357596340224>";
            else
                return;

            await msg.Channel.SendMessageAsync(emote);
        }
    }
}
