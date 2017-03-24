using Discord.Commands;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [DestinyServerOnly]
    public class AslanModule : ModuleBase<VbCommandContext>
    {
        [Command("aslan")]
        public async Task BotInfo([Remainder]string unused = null)
        {
            await ReplyAsync("<:ASLAN:271856531505545236> I post in <#291758537808412690> when " +
                "Destiny goes live on Twitch, posts a new YouTube video, or tweets. I also bully Baudgee. " +
                "<:FrankerZ:271856531904004126> Pester vaindil with any issues (vaindil#2314 on Discord). " +
                "Source is on GitHub: <https://github.com/vaindil/vainbot-discord>");
        }

        [Command("commands")]
        [Alias("command")]
        public async Task CommandList([Remainder]string unused = null)
        {
            await ReplyAsync(
                "!live: Gets info about the current or most recent stream\n" +
                "!youtube: Get the most recent YouTube video posted\n" +
                "!twitter: Get the most recent tweet posted\n" +
                "! <:FerretLOL:271856531857735680> : Post a random ferret picture\n" +
                "! <:ASLAN:271856531505545236> : Post a random Aslan picture");
        }
    }
}
