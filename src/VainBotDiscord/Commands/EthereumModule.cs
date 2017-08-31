using Discord.Commands;
using Newtonsoft.Json;
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
            var priceString = await _httpClient
                .GetStringAsync("https://min-api.cryptocompare.com/data/pricemulti?fsyms=BTC,ETH&tsyms=USD");
            var prices = JsonConvert.DeserializeObject<CryptoApiResponse>(priceString);

            var sb = new StringBuilder();
            sb.Append("Current ETH price: ");
            sb.Append(prices.Eth.Usd);
            sb.Append("\n");

            sb.Append("Current BTC price: ");
            sb.Append(prices.Btc.Usd);

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
