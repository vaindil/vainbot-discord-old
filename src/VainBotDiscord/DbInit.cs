using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace VainBotDiscord
{
    public static class DbInit
    {
        public static async Task<string> InitDbAsync()
        {
            using (var db = new VbContext())
            {
                var apiToken = await db.KeyValues.GetValueAsync(DbKey.DiscordApiKey);

                var bettingAllowedExists = await db.KeyValues
                    .FirstOrDefaultAsync(kv => kv.Key == DbKey.BettingAllowed.ToString());
                if (bettingAllowedExists == null)
                {
                    var bet = new KeyValue
                    {
                        Key = DbKey.BettingAllowed.ToString(),
                        Value = false.ToString()
                    };

                    db.KeyValues.Add(bet);
                    await db.SaveChangesAsync();
                }

                var lolCounterExists = await db.KeyValues
                    .FirstOrDefaultAsync(kv => kv.Key == DbKey.LolCounter.ToString());
                if (lolCounterExists == null)
                {
                    db.KeyValues.Add(new KeyValue
                    {
                        Key = DbKey.LolCounter.ToString(),
                        Value = 0.ToString()
                    });

                    await db.SaveChangesAsync();
                }

                var noUCounterExists = await db.KeyValues
                    .FirstOrDefaultAsync(kv => kv.Key == DbKey.NoUCounter.ToString());
                if (noUCounterExists == null)
                {
                    db.KeyValues.Add(new KeyValue
                    {
                        Key = DbKey.NoUCounter.ToString(),
                        Value = 0.ToString()
                    });

                    await db.SaveChangesAsync();
                }

                var lastTokenUpdateExists = await db.KeyValues
                    .FirstOrDefaultAsync(kv => kv.Key == DbKey.LastNaTokenUpdate.ToString());
                if (lastTokenUpdateExists == null)
                {
                    var dtNa = new KeyValue
                    {
                        Key = DbKey.LastNaTokenUpdate.ToString(),
                        Value = DateTime.UtcNow.AddMinutes(-20).ToString()
                    };

                    var dtEu = new KeyValue
                    {
                        Key = DbKey.LastEuTokenUpdate.ToString(),
                        Value = DateTime.UtcNow.AddMinutes(-20).ToString()
                    };

                    var eu = new KeyValue
                    {
                        Key = DbKey.LastEuTokenPrice.ToString(),
                        Value = "N/A"
                    };

                    var na = new KeyValue
                    {
                        Key = DbKey.LastNaTokenPrice.ToString(),
                        Value = "N/A"
                    };

                    db.KeyValues.Add(dtNa);
                    db.KeyValues.Add(dtEu);
                    db.KeyValues.Add(eu);
                    db.KeyValues.Add(na);

                    await db.SaveChangesAsync();
                }

                return apiToken;
            }
        }
    }
}
