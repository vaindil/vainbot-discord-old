using Discord.Commands;
using System.Threading.Tasks;
using VainBotDiscord.Twitch;
using VainBotDiscord.Twitter;
using VainBotDiscord.Utils;
using VainBotDiscord.YouTube;

namespace VainBotDiscord.Commands
{
    [Group("reload")]
    [IsOwner]
    public class ReloadModule : ModuleBase<VbCommandContext>
    {
        readonly TwitchService _twitchSvc;
        readonly YouTubeService _youTubeSvc;
        readonly TwitterService _twitterSvc;

        public ReloadModule(
            TwitchService twitchSvc,
            YouTubeService youTubeSvc,
            TwitterService twitterSvc)
        {
            _twitchSvc = twitchSvc;
            _youTubeSvc = youTubeSvc;
            _twitterSvc = twitterSvc;
        }

        [Command("twitch")]
        public async Task ReloadTwitchAsync()
        {
            await _twitchSvc.ReloadAsync();
            await ReplyAsync("Twitch reloaded");
        }

        [Command("youtube")]
        public async Task ReloadYouTubeAsync()
        {
            await _youTubeSvc.ReloadAsync();
            await ReplyAsync("YouTube reloaded");
        }

        [Command("twitter")]
        public async Task ReloadTwitterAsync()
        {
            await _twitterSvc.ReloadAsync();
            await ReplyAsync("Twitter reloaded");
        }
    }
}
