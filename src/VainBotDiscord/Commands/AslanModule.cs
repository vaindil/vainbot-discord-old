using Discord.Commands;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [DestinyServerOnly]
    public class AslanModule : ModuleBase
    {
        [Command("aslan")]
        public async Task BotInfo([Remainder]string unused = null)
        {
            await Context.Channel.SendMessageAsync("<:ASLAN:271856531505545236> I post in <#291758537808412690> when " +
                "Destiny goes live on Twitch, posts a new YouTube video, or tweets. I also bully Baudgee. " +
                "<:FrankerZ:271856531904004126> Pester vaindil with any issues (vaindil#2314 on Discord). " +
                "Source is on GitHub: <https://github.com/vaindil/vainbot-discord>");
        }

        [Command("commands")]
        [Alias("command")]
        public async Task CommandList([Remainder]string unused = null)
        {
            await Context.Channel.SendMessageAsync("!youtube/!yt: Get the most recent YouTube video posted");
        }
    }
}
