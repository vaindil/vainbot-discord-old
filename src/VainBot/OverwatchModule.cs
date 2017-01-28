using Discord.Commands;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VainBot.OverwatchModels;

namespace VainBot
{
    //[Group("overwatch")]
    //[Alias("ow")]
    public class OverwatchModule : ModuleBase
    {
        readonly HttpClient _client;

        static Regex _validBTag = new Regex(@"^[\p{L}\p{Mn}][\p{L}\p{Mn}0-9]{2,11}#[0-9]{4,5}$", RegexOptions.IgnoreCase);
        static string _baseUrl = @"https://owapi.net/api/v3/u/";

        public OverwatchModule(HttpClient client)
        {
            _client = client;
        }

        //[Command]
        public async Task UserStats(string bTag)
        {
            if (!_validBTag.IsMatch(bTag))
            {
                await ReplyAsync("That's not a valid BattleTag, you nerd.");
                return;
            }

            var modBTag = bTag.Replace('#', '-');

            var response = await _client.GetAsync(_baseUrl + modBTag + "/stats");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                await ReplyAsync("That BattleTag doesn't exist or has never played Overwatch. Note that BattleTags " +
                                 "are case-sensitive (not my rule!).");
                return;
            }
            else if (!response.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error contacting the API. Go bug vaindil#2314. Sorry!");
                return;
            }

            //var msg = await ReplyAsync("Checking " + bTag + "'s info, just a moment...");

            var statsRaw = await response.Content.ReadAsStringAsync();
            var stats = JsonConvert.DeserializeObject<BlobModel>(statsRaw, Extensions.GetJsonSettings());

            var rank = stats?.Us?.Stats?.Competitive?.OverallStats?.Comprank;

            //await msg.ModifyAsync(c => c.Content = bTag + "'s competitive rank is " + rank + ".");

            if (rank.HasValue)
                await ReplyAsync(bTag + "'s competitive rank is " + rank + ". " + stats?.Us?.Stats?.Competitive?.OverallStats?.Avatar);
            else
                await ReplyAsync(bTag + " has not played competitive this season.");
        }
    }
}
