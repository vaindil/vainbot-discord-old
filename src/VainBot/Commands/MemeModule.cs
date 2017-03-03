using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using VainBotDiscord;

namespace VainBot.Commands
{
    [Group("meme")]
    public class MemeModule : ModuleBase
    {
        readonly HttpClient _client;
        readonly Random _rng;

        public MemeModule(HttpClient client, Random rng)
        {
            _client = client;
            _rng = rng;
        }

        [Command]
        public async Task RandomMeme()
        {
            var pageIndex = _rng.Next(0, 3);

            var result = await _client
                .GetStringAsync("http://version1.api.memegenerator.net/Instances_Select_ByPopular" + 
                                $"?languageCode=en&pageIndex={pageIndex}&urlName=&days=1");

            var memeResult = JsonConvert.DeserializeObject<MemeResult>(result);
            var index = _rng.Next(1, memeResult.Result.Count + 1);

            var img = await _client.GetAsync(memeResult.Result[index].InstanceImageUrl);

            using (var stream = new MemoryStream())
            {
                await img.Content.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(stream, "datmeme.png");
            }
        }
    }
}
