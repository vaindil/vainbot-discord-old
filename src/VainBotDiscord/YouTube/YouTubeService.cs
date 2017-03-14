using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VainBotDiscord.YouTube
{
    public class YouTubeService
    {
        readonly DiscordSocketClient _client;
        static HttpClient _youTubeClient = new HttpClient();
        static string _youTubeApiKey;
        List<Timer> _timerList;

        public YouTubeService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task InitYouTubeService()
        {
            _timerList = new List<Timer>();

            // verified to exist in Program.Run()
            _youTubeApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");

            _youTubeClient.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
            _youTubeClient.Timeout = new TimeSpan(0, 0, 8);

            var youTubesToCheck = new List<YouTubeToCheck>();

            using (var db = new VbContext())
            {
                youTubesToCheck = await db.YouTubesToCheck.ToListAsync();
            }

            if (youTubesToCheck == null || youTubesToCheck.Count == 0)
                return;

            foreach (var yt in youTubesToCheck)
            {
                var t = new Timer(CheckYouTubeAsync, yt, new TimeSpan(0, 0, 20), new TimeSpan(0, 0, yt.Frequency));
                _timerList.Add(t);
            }
        }

        async void CheckYouTubeAsync(object youTubeToCheckIn)
        {
            var youTubeToCheck = (YouTubeToCheck)youTubeToCheckIn;
            var playlist = await GetYouTubePlaylistAsync(youTubeToCheck.PlaylistId);

            var latestIndex = 0;
            var latestDateTime = DateTime.MinValue;

            for (var i = 0; i < playlist.Items.Count; i++)
            {
                if (playlist.Items[i].ContentDetails.VideoPublishedAt > latestDateTime)
                {
                    latestIndex = i;
                    latestDateTime = playlist.Items[i].ContentDetails.VideoPublishedAt;
                }
            }

            var latestVideoId = playlist.Items[latestIndex].ContentDetails.VideoId;

            YouTubeRecord existingRecord;

            using (var db = new VbContext())
            {
                existingRecord = await db.YouTubeRecords.FirstOrDefaultAsync(r => r.PlaylistId == youTubeToCheck.PlaylistId);
            }

            if (existingRecord != null && latestVideoId == existingRecord.VideoId)
                return;

            var channel = (_client.GetChannel((ulong)youTubeToCheck.DiscordChannelId)) as SocketTextChannel;

            var videoList = await GetYouTubeVideoListAsync(latestVideoId);
            if (videoList.Items.Count != 1)
            {
                await channel.SendMessageAsync("Something broke when posting a new YouTube video. Bug vaindil about it. (error: v)");
                return;
            }

            var success = await SendMessageAsync(youTubeToCheck, videoList.Items[0]);
            if (!success)
                return;

            using (var db = new VbContext())
            {
                if (existingRecord == null)
                {
                    existingRecord = new YouTubeRecord
                    {
                        PlaylistId = youTubeToCheck.PlaylistId,
                        VideoId = videoList.Items[0].Id,
                        PublishedAt = videoList.Items[0].Snippet.PublishedAt
                    };

                    db.YouTubeRecords.Add(existingRecord);
                }
                else
                {
                    existingRecord.VideoId = videoList.Items[0].Id;
                    existingRecord.PublishedAt = videoList.Items[0].Snippet.PublishedAt;

                    db.YouTubeRecords.Update(existingRecord);
                }

                await db.SaveChangesAsync();
            }
        }

        async Task<bool> SendMessageAsync(YouTubeToCheck youTubeToCheck, YouTubeVideoListItem video)
        {
            var channel = (_client.GetChannel((ulong)youTubeToCheck.DiscordChannelId)) as SocketTextChannel;

            var embed = await CreateEmbedAsync(video);
            if (embed == null)
            {
                await channel.SendMessageAsync("Something broke when posting a new YouTube video. Bug vaindil about it. (error: e)");
                return false;
            }

            await channel.SendMessageAsync($"{video.Snippet.ChannelTitle} posted a new YouTube video.", embed: embed);
            return true;
        }

        async Task<YouTubePlaylist> GetYouTubePlaylistAsync(string playlistId)
        {
            var response = await _youTubeClient
                .GetAsync($"playlistItems?part=contentDetails&playlistId={playlistId}&maxResults=3&key={_youTubeApiKey}");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<YouTubePlaylist>(responseString, Extensions.GetJsonSettings());
        }

        async Task<YouTubeVideoList> GetYouTubeVideoListAsync(string videoId)
        {
            var response = await _youTubeClient
                .GetAsync($"videos?part=snippet&id={videoId}&maxResults=1&key={_youTubeApiKey}");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<YouTubeVideoList>(responseString, Extensions.GetJsonSettings());
        }

        async Task<Embed> CreateEmbedAsync(YouTubeVideoListItem video)
        {
            // GET CHANNEL OBJECT FROM API
            var response = await _youTubeClient
                .GetAsync($"channels?part=snippet&id={video.Snippet.ChannelId}&key={_youTubeApiKey}");
            var responseString = await response.Content.ReadAsStringAsync();
            var channelList = JsonConvert.DeserializeObject<YouTubeChannelList>(responseString);
            if (channelList.Items.Count != 1)
                return null;

            var channel = channelList.Items[0];

            var embed = new EmbedBuilder();
            var author = new EmbedAuthorBuilder
            {
                Name = channel.Snippet.Title,
                Url = "https://www.youtube.com/channel/" + channel.Id,
                IconUrl = channel.Snippet.Thumbnails.Default.Url
            };

            var shortDescription = video.Snippet.Description;

            var lineBreakIndex = video.Snippet.Description.IndexOf("\n");
            if (lineBreakIndex != -1)
                shortDescription = shortDescription.Substring(0, lineBreakIndex);

            if (shortDescription.Length > 500)
                shortDescription = shortDescription.Substring(0, 500) + "...";

            var descriptionField = new EmbedFieldBuilder
            {
                Name = "Description",
                Value = shortDescription,
                IsInline = false
            };

            embed.Author = author;
            embed.Color = new Color(205, 32, 31);
            embed.ImageUrl = video.Snippet.Thumbnails.Standard.Url;
            embed.Title = "New YouTube video from " + channel.Snippet.Title;
            embed.Url = author.Url;

            embed.AddField(descriptionField);

            return embed.Build();
        }
    }
}
