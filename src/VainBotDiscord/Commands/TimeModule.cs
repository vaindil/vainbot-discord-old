using Discord.Commands;
using NodaTime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("time")]
    [IsOwner]
    public class TimeModule : ModuleBase<VbCommandContext>
    {
        readonly ThrottlerService _throttler;

        public TimeModule(ThrottlerService throttler)
        {
            _throttler = throttler;
        }

        [Command("help")]
        public async Task GetInfo([Remainder]string unused = null)
        {
            await ReplyAsync("`!time timezone`: Gets the current time in the specified timezone.\n" +
                "`!time timezone time`: Gets the specified time in the specified timezone.");
        }

        [Command("timezone")]
        [Alias("timezones", "list")]
        public async Task GetTimezones([Remainder]string unused = null)
        {
            var tzsCollection = DateTimeZoneProviders.Tzdb.Ids;
            var tzs = new List<string>(tzsCollection );
            tzs.Sort();

            var sb = new StringBuilder();

            foreach (var tz in tzs)
            {
                sb.Append(tz + "\n");
            }

            var msg = sb.ToString();
            var dmChannel = await Context.User.CreateDMChannelAsync();

            while (msg.Length > 1000)
            {
                var thisMsg = msg.Substring(0, 1000);
                await dmChannel.SendMessageAsync(thisMsg);

                msg = msg.Substring(1000);
            }
            
            await dmChannel.SendMessageAsync(msg);
        }
    }
}
