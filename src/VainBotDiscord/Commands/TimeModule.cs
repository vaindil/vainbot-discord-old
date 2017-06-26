using Discord.Commands;
using NodaTime;
using System.Globalization;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    [Group("time")]
    public class TimeModule : ModuleBase<VbCommandContext>
    {
        [Command("help")]
        [Priority(10)]
        public async Task GetInfo([Remainder]string unused = null)
        {
            await ReplyAsync("`!time [timezone]`: Gets the current time in the specified timezone. " +
                "Example: `!time America/Detroit`");
        }

        [Command("timezone")]
        [Alias("timezones", "list")]
        [Priority(8)]
        public async Task GetTimezones([Remainder]string unused = null)
        {
            await ReplyAsync(
                "The list of supported timezones is available here: <https://docs.nightbot.tv/commands/variables/time#timezones>");
        }

        [Command]
        [Priority(1)]
        public async Task CurrentInTimezone([Remainder]string timezone = null)
        {
            var crendor = false;

            if (timezone == null
                || string.Equals(timezone, "crendor", System.StringComparison.CurrentCultureIgnoreCase)
                || string.Equals(timezone, "crendor", System.StringComparison.CurrentCultureIgnoreCase))
            {
                timezone = "America/Chicago";
                crendor = true;
            }

            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            if (tz == null)
            {
                await ReplyAsync("Invalid timezone provided. Use `!time help` for help. Supported timezones are listed here: " +
                    "<https://docs.nightbot.tv/commands/variables/time#timezones>");
                return;
            }

            var curDt = SystemClock.Instance.GetCurrentInstant().InZone(tz);
            var shortTz = crendor ? " Central Crendor Time" : curDt.ToString(" x", CultureInfo.InvariantCulture);

            await ReplyAsync($"Current time is {curDt.ToString("H:mm", CultureInfo.InvariantCulture)}{shortTz}.");
        }
    }

    [Group("timezones")]
    [Alias("timezone")]
    public class TimezoneModule : ModuleBase<VbCommandContext>
    {
        [Command]
        public async Task TimezoneLink([Remainder]string unused = null)
        {
            await ReplyAsync(
                "The list of supported timezones is available here: <https://docs.nightbot.tv/commands/variables/time#timezones>");
        }
    }
}
