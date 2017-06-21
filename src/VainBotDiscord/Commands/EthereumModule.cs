using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    [Group("ethereum")]
    [Alias("eth", "bitcoin", "btc", "cake", "crypto")]
    public class EthereumModule : ModuleBase
    {
        static readonly HttpClient _httpClient = new HttpClient();

        [Command]
        public async Task Eth([Remainder]string unused = null)
        {
            const decimal cakeStartEth = 352.44M;
            const decimal cakeStartBtc = 2740.24M;

            var priceString = await _httpClient
                .GetStringAsync("https://min-api.cryptocompare.com/data/pricemulti?fsyms=BTC,ETH&tsyms=USD");
            var prices = JsonConvert.DeserializeObject<CryptoApiResponse>(priceString);

            var sb = new StringBuilder();
            sb.Append("Current ETH price: ");
            sb.Append(prices.Eth.Usd);
            sb.Append("\n");

            sb.Append("Current BTC price: ");
            sb.Append(prices.Btc.Usd);

            if (Context.Guild.Id == 268547141721522178)
            {
                sb.Append("\n\n__Cake's starting prices__ _(2017-06-21 0100)_\n");
                sb.Append($"ETH: {cakeStartEth} | {prices.Eth.Usd - cakeStartEth}\n");
                sb.Append($"BTC: {cakeStartBtc} | {prices.Btc.Usd - cakeStartBtc}");
            }

            await ReplyAsync(sb.ToString());
        }

        class CryptoApiResponse
        {
            public CryptoPrice Btc { get; set; }

            public CryptoPrice Eth { get; set; }

            public class CryptoPrice
            {
                public decimal Usd { get; set; }
            }
        }
    }
}
