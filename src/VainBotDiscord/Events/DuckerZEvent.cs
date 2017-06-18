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

            await msg.Channel.SendMessageAsync("<:DuckerZ:259886357596340224>");
        }
    }
}
