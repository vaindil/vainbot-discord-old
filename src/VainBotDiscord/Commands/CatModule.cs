using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("cat")]
    [Alias("<:ASLAN:271856531505545236>", "<:DJAslan:271856531505545216>")]
    [DestinyServerOnly]
    public class CatModule : ModuleBase<VbCommandContext>
    {
        readonly HttpClient _client;
        readonly Random _rng;

        readonly ThrottlerService _throttler;

        public CatModule(HttpClient client, Random rng, ThrottlerService throttler)
        {
            _client = client;
            _rng = rng;

            _throttler = throttler;
        }

        [Command(RunMode = RunMode.Async)]
        public async Task Cat([Remainder]string unused = null)
        {
            if (!_throttler.CommandAllowed(ThrottleTypes.Cat, Context.Channel.Id))
                return;

            var imgurClientId = Environment.GetEnvironmentVariable("IMGUR_CLIENT_ID");

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.imgur.com/3/album/ohOjC/images");
            request.Headers.Authorization = new AuthenticationHeaderValue("Client-ID", imgurClientId);

            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var album = JsonConvert.DeserializeObject<ImgurAlbum>(responseString);
            var imageList = album.Data;

            if (imageList == null || imageList.Count == 0)
            {
                await ReplyAsync("Error getting an Aslan picture. Probably not my fault. Sorry!");
                return;
            }

            var image = imageList[_rng.Next(imageList.Count)];
            var imgUri = new Uri(image.Link.Replace("http://", "https://"));

            var img = await _client.GetAsync(imgUri);

            using (var stream = new MemoryStream())
            {
                await img.Content.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(stream, Path.GetFileName(imgUri.AbsolutePath));
            }

            _throttler.Throttle(ThrottleTypes.Cat, Context.Channel.Id);
        }

        class ImgurAlbum
        {
            public List<ImgurImage> Data { get; set; }
        }

        class ImgurImage
        {
            public string Id { get; set; }

            public string Link { get; set; }
        }
    }
}
