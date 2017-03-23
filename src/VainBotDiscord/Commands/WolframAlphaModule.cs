using Discord.Commands;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("wolfram")]
    [Alias("wolframalpha", "lookup")]
    //[CrendorServerOnly]
    public class WolframAlphaModule : ModuleBase<VbCommandContext>
    {
        readonly HttpClient _client;
        readonly ThrottlerService _throttler;

        public WolframAlphaModule(HttpClient client, ThrottlerService throttler)
        {
            _client = client;
            _throttler = throttler;
        }

        [Command]
        public async Task QueryImperial([Remainder]string query = null)
        {
            await SubmitQuery(query, UnitType.Imperial);
        }

        [Command("i")]
        [Alias("imperial")]
        public async Task QueryImperial2([Remainder]string query = null)
        {
            await SubmitQuery(query, UnitType.Imperial);
        }

        [Command("m")]
        [Alias("metric")]
        public async Task QueryMetric([Remainder]string query = null)
        {
            await SubmitQuery(query, UnitType.Metric);
        }

        async Task SubmitQuery(string query, UnitType unit)
        {
            if (!_throttler.CommandAllowed(ThrottleTypes.WolframAlpha, Context.Channel.Id))
                return;

            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("You have to actually, y'know, query something.");
                return;
            }

            if (query.Length > 70)
            {
                await ReplyAsync("That's too long, try again.");
                return;
            }

            var appId = Environment.GetEnvironmentVariable("WOLFRAMALPHA_APPID");
            var url = $"https://api.wolframalpha.com/v1/result?appid={appId}&units={unit.ToString().ToLower()}&i=";
            url += WebUtility.UrlEncode(query);
            var msg = "";

            var response = await _client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotImplemented)
                msg = "WolframAlpha couldn't understand that query. Try sucking less.";
            else if (!response.IsSuccessStatusCode)
                msg = "Error querying the WolframAlpha API. Probably not my fault. Pester vaindil about it.";
            else
                msg = await response.Content.ReadAsStringAsync();

            await ReplyAsync(msg);

            _throttler.Throttle(ThrottleTypes.WolframAlpha, Context.Channel.Id);
        }

        enum UnitType
        {
            Imperial,
            Metric
        }
    }
}
