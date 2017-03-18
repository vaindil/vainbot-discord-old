using System;

namespace VainBotDiscord.Utils
{
    public static class MiscUtils
    {
        public static string ToFriendlyString(this TimeSpan ts)
        {
            var durationString = "";

            if (ts.Days > 1)
                durationString += ts.Days + " days, ";
            else if (ts.Days == 1)
                durationString += ts.Days + " day, ";

            if (ts.Hours > 1 || (ts.Hours == 0 && !string.IsNullOrEmpty(durationString)))
                durationString += ts.Hours + " hrs, ";
            else if (ts.Hours == 1)
                durationString += ts.Hours + " hr, ";

            if (ts.Minutes > 1 || (ts.Minutes == 0 && !string.IsNullOrEmpty(durationString)))
                durationString += ts.Minutes + " mins";
            else if (ts.Minutes == 1)
                durationString += ts.Minutes + " min";

            if (string.IsNullOrEmpty(durationString))
                durationString = "0 mins";

            return durationString;
        }
    }
}
