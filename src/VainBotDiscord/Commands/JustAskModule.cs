using Discord.Commands;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [CompSciChannelOnly]
    public class JustAskModule : ModuleBase
    {
        [Command("justask")]
        [Alias("ask")]
        public async Task JustAsk([Remainder]string unused = null)
        {
            await ReplyAsync("Just ask your question and someone will help if they can.");
        }
    }
}
