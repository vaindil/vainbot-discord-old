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
    [DestinyServerOnly]
    public class LiveModule : ModuleBase
    {
        ThrottlerService _throttler;

        public LiveModule(ThrottlerService throttler)
        {
            _throttler = throttler;
        }

        [Command]
        public async Task GetLive([Remainder]string unused = null)
        {
            if (!_throttler.CommandAllowed(ThrottleTypes.Live, Context.Channel.Id))
                return;

            StreamLastOnline lastOnline;
            StreamRecord streamRecord;

            using (var db = new VbContext())
            {
                lastOnline = await db.StreamLastOnlines.FirstOrDefaultAsync(s => s.UserId == 18074328);
                streamRecord = await db.StreamRecords.FirstOrDefaultAsync(s => s.UserId == 18074328);
            }

            string msg;

            if (lastOnline == null && streamRecord == null)
                msg = "I won't have this info until the next time the stream goes live. Sorry!";

            else if (lastOnline == null && streamRecord != null)
            {
                var duration = DateTime.UtcNow - streamRecord.StartTime;
                msg = "Destiny is live! <:FerretLOL:271856531857735680> <https://www.destiny.gg/bigscreen>\n" +
                    $"Currently playing {streamRecord.CurrentGame}\n" +
                    $"Live for {duration.ToFriendlyString()}";
            }

            else
            {
                var duration = DateTime.UtcNow - lastOnline.LastOnlineAt;
                msg = $"Destiny is offline. <:DaFeels:271856531572523010>\n" +
                    $"Last online {duration.ToFriendlyString()} ago, was playing {lastOnline.LastGame}.";
            }

            await Context.Channel.SendMessageAsync(msg);

            _throttler.Throttle(ThrottleTypes.Live, Context.Channel.Id);
        }
    }
}
