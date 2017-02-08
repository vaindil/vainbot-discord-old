using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    [Group("catfact")]
    [Alias("catfacts")]
    public class CatFactsModule : ModuleBase
    {
        readonly HttpClient _client;

        public CatFactsModule(HttpClient client)
        {
            _client = client;
        }

        [Command]
        public async Task CatFact()
        {
            var url = new Uri("https://catfacts-api.appspot.com/api/facts?number=1");
            var result = await _client.GetAsync(url);

            var factString = await result.Content.ReadAsStringAsync();
            var facts = JsonConvert.DeserializeObject<CatFacts>(factString);

            if (facts == null || facts.Facts == null || facts.Facts.Count == 0)
            {
                await ReplyAsync("Error getting your cat fact. Not my fault. Sorry!");
                return;
            }

            await ReplyAsync(facts.Facts[0]);
        }
    }
}
