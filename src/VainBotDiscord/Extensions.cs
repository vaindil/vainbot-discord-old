using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VainBotDiscord
{
    public static class Extensions
    {
        static readonly Regex validDie = new Regex(@"^-?\d*d-?\d+$", RegexOptions.IgnoreCase);

        public static async Task<string> GetValueAsync(this DbSet<KeyValue> dbSetKv, DbKey key)
        {
            var keyValue = await dbSetKv.FirstAsync(kv => kv.Key == key.ToString());
            return keyValue.Value;
        }

        public static string ValidateDiceRoll(string d, bool checkRegex, int maxDice = 30)
        {
            if (checkRegex && !validDie.IsMatch(d))
                return d + " isn't a die, you nerd.";

            int numDice;
            d = d.ToLower();
            var deezNuts = d.Split('d');

            if (string.IsNullOrEmpty(deezNuts[0]))
                numDice = 1;
            else if (deezNuts[0].Length > 8)
                return "You tryin' to overflow me? I'm better than that.";
            else
                numDice = int.Parse(deezNuts[0]);

            if (deezNuts[1].Length > 8)
                return "You tryin' to overflow me? I'm better than that.";

            var numSides = int.Parse(deezNuts[1]);

            if (numDice < 1 || numDice > maxDice)
                return "I'd like to see you try to roll " + numDice + " dice at once.";

            if (numSides < 2 || numSides > 10000)
                return "You actually think " + GetAOrAn(numSides) + " " + numSides + "-sided die exists?";

            return string.Empty;
        }

        public static string GetAOrAn(int num)
        {
            while (num >= 10)
                num /= 10;

            if (num == 8)
                return "an";

            return "a";
        }

        public static async Task<string> GetCorrectPluralityAsync(this decimal num, VbContext context)
        {
            var msg = num.GetNumberString();

            msg += " ";

            if (num == 1)
                msg += await context.KeyValues.GetValueAsync(DbKey.PointsNameSingular);
            else
                msg += await context.KeyValues.GetValueAsync(DbKey.PointsNamePlural);

            return msg;
        }

        public static string GetNumberString(this decimal num)
        {
            string msg;

            if ((int)num == num)
                msg = ((int)num).ToString();
            else if (num == 3.14M)
                msg = "π";
            else
                msg = num.ToString();

            return msg;
        }

        public static string GetImagePath(string filename)
        {
            return $"Images{Path.DirectorySeparatorChar}{filename}";
        }

        public static JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(true, false)
                },
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }
    }
}
