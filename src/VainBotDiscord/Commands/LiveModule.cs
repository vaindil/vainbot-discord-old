using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VainBotDiscord.Twitch;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("live")]
    [Alias("twitch")]
    public class LiveModule : ModuleBase<VbCommandContext>
    {
        ThrottlerService _throttler;

        public LiveModule(ThrottlerService throttler)
        {
            _throttler = throttler;
        }

        [Command]
        public async Task GetLive([Remainder]string unused = null)
        {
            if (!Context.HasMainUser 
                || Context.MainUser.TwitchUserId == null
                || !_throttler.CommandAllowed(ThrottleTypes.Live, Context.Channel.Id))
                return;

            StreamLastOnline lastOnline;
            StreamRecord streamRecord;

            using (var db = new VbContext())
            {
                lastOnline = await db.StreamLastOnlines.FirstOrDefaultAsync(s => s.UserId == Context.MainUser.TwitchUserId);
                streamRecord = await db.StreamRecords.FirstOrDefaultAsync(s => s.UserId == Context.MainUser.TwitchUserId);
            }

            string msg;

            if (lastOnline == null && streamRecord == null)
                msg = $"{Context.MainUser.FriendlyUsername} is offline. " +
                    $"I'll have more info in the future, after the next time the stream goes live. " +
                    $"Sorry!";

            else if (lastOnline == null && streamRecord != null)
            {
                var duration = DateTime.UtcNow - streamRecord.StartTime;
                msg = $"{Context.MainUser.FriendlyUsername} is live! {Context.MainUser.StreamUrl}\n" +
                    $"Currently playing {streamRecord.CurrentGame}\n" +
                    $"Live for {duration.ToFriendlyString()}";
            }

            else
            {
                var duration = DateTime.UtcNow - lastOnline.LastOnlineAt;
                msg = $"{Context.MainUser.FriendlyUsername} is offline.\n" +
                    $"Last online {duration.ToFriendlyString()} ago, was playing {lastOnline.LastGame}.";
            }

            await ReplyAsync(msg);

            _throttler.Throttle(ThrottleTypes.Live, Context.Channel.Id);
        }
    }
}
