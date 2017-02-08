using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    [Group("token")]
    [Alias("wowtoken")]
    public class WowTokenModule : ModuleBase
    {
        readonly HttpClient _client;

        public WowTokenModule(HttpClient client)
        {
            _client = client;
        }

        [Command]
        public async Task Current()
        {
            bool needsUpdate = false;

            using (var db = new VbContext())
            {
                var prev = DateTime.UtcNow.AddMinutes(-10);

                var lastNaUpdateString = await db.KeyValues.GetValueAsync(DbKey.LastNaTokenUpdate);
                var lastNaUpdate = DateTime.Parse(lastNaUpdateString);

                var lastEuUpdateString = await db.KeyValues.GetValueAsync(DbKey.LastEuTokenUpdate);
                var lastEuUpdate = DateTime.Parse(lastEuUpdateString);

                if (lastNaUpdate < prev || lastEuUpdate < prev)
                    needsUpdate = true;
            }

            string naPrice;
            string euPrice;
            DateTime naDate;
            DateTime euDate;

            if (needsUpdate)
            {
                var result = await _client.GetAsync("https://wowtoken.info/snapshot.json");
                if (!result.IsSuccessStatusCode)
                {
                    await ReplyAsync("Error querying the API. Not my fault, sorry!");
                    return;
                }

                var resultString = await result.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<WowTokenResponse>(resultString);

                naPrice = tokenResponse.Na.Formatted.Buy;
                euPrice = tokenResponse.Eu.Formatted.Buy;
                naDate = tokenResponse.Na.Raw.UpdatedIso8601;
                euDate = tokenResponse.Eu.Raw.UpdatedIso8601;

                using (var db = new VbContext())
                {
                    var naDt = await db.KeyValues.FirstAsync(kv => kv.Key == DbKey.LastNaTokenUpdate.ToString());
                    var euDt = await db.KeyValues.FirstAsync(kv => kv.Key == DbKey.LastEuTokenUpdate.ToString());
                    var naKv = await db.KeyValues.FirstAsync(kv => kv.Key == DbKey.LastNaTokenPrice.ToString());
                    var euKv = await db.KeyValues.FirstAsync(kv => kv.Key == DbKey.LastEuTokenPrice.ToString());

                    naDt.Value = tokenResponse.Na.Raw.UpdatedIso8601.ToString();
                    euDt.Value = tokenResponse.Eu.Raw.UpdatedIso8601.ToString();
                    naKv.Value = naPrice;
                    euKv.Value = euPrice;

                    await db.SaveChangesAsync();
                }
            }
            else
            {
                using (var db = new VbContext())
                {
                    naPrice = await db.KeyValues.GetValueAsync(DbKey.LastNaTokenPrice);
                    euPrice = await db.KeyValues.GetValueAsync(DbKey.LastEuTokenPrice);

                    var naDateString = await db.KeyValues.GetValueAsync(DbKey.LastNaTokenUpdate);
                    var euDateString = await db.KeyValues.GetValueAsync(DbKey.LastEuTokenUpdate);
                    naDate = DateTime.Parse(naDateString);
                    euDate = DateTime.Parse(euDateString);
                }
            }

            await ReplyAsync(
                "**NA**: " + naPrice + "\n" +
                "_(as of " + naDate.ToUniversalTime().ToString("MM-dd H:mm") + ")_\n" +
                "\n" +
                "**EU**: " + euPrice + "\n" +
                " _(as of " + euDate.ToUniversalTime().ToString("MM-dd H:mm") + ")_\n" +
                "\n" +
                "More info: <https://wowtoken.info>");
        }
    }
}
