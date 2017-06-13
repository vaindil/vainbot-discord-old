using Discord.Commands;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
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
