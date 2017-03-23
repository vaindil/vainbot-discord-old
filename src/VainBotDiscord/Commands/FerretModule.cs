using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("ferret")]
    [Alias("ferretlol", "<:FerretLOL:271856531857735680>")]
    [DestinyServerOnly]
    public class FerretModule : ModuleBase<VbCommandContext>
    {
        readonly HttpClient _client;
        readonly ThrottlerService _throttler;

        public FerretModule(HttpClient client, ThrottlerService throttler)
        {
            _client = client;
            _throttler = throttler;
        }

        [Command]
        public async Task Ferret()
        {
            if (!_throttler.CommandAllowed(ThrottleTypes.Ferret, Context.Channel.Id))
                return;

            var response = await _client.GetAsync("https://polecat.me/api/ferret");
            var responseString = await response.Content.ReadAsStringAsync();
            var ferret = JsonConvert.DeserializeObject<FerretResponse>(responseString);

            var img = await _client.GetAsync(ferret.Url);

            using (var stream = new MemoryStream())
            {
                await img.Content.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(stream, "ferret.png");
            }

            _throttler.Throttle(ThrottleTypes.Ferret, Context.Channel.Id);
        }

        class FerretResponse
        {
            public string Url { get; set; }

            public long Last { get; set; }
        }
    }
}
