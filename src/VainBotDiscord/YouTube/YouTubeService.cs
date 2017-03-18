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
        readonly TimeZoneInfo _tz;

        static HttpClient _youTubeClient = new HttpClient();
        static string _youTubeApiKey;
        List<Timer> _timerList;

        public YouTubeService(DiscordSocketClient client, TimeZoneInfo tz)
        {
            _client = client;
            _tz = tz;
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
                var t = new Timer(CheckYouTubeAsync, yt, new TimeSpan(0, 0, 5), new TimeSpan(0, 0, yt.Frequency));
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

            var video = videoList.Items[0];

            var embed = await SendMessageAsync(youTubeToCheck, video);
            if (embed == null)
                return;

            using (var db = new VbContext())
            {
                if (existingRecord == null)
                {
                    existingRecord = new YouTubeRecord
                    {
                        PlaylistId = youTubeToCheck.PlaylistId,
                        VideoId = video.Id,
                        VideoTitle = video.Snippet.Title,
                        VideoDescription = embed.Description,
                        ImageUrl = embed.Image.Value.Url,
                        AuthorName = embed.Author.Value.Name,
                        AuthorUrl = embed.Author.Value.Url,
                        AuthorIconUrl = embed.Author.Value.IconUrl,
                        PublishedAt = video.Snippet.PublishedAt.UtcDateTime
                    };

                    db.YouTubeRecords.Add(existingRecord);
                }
                else
                {
                    existingRecord.VideoId = video.Id;
                    existingRecord.VideoTitle = video.Snippet.Title;
                    existingRecord.VideoDescription = embed.Description;
                    existingRecord.ImageUrl = embed.Image.Value.Url;
                    existingRecord.AuthorName = embed.Author.Value.Name;
                    existingRecord.AuthorUrl = embed.Author.Value.Url;
                    existingRecord.AuthorIconUrl = embed.Author.Value.IconUrl;
                    existingRecord.PublishedAt = video.Snippet.PublishedAt.UtcDateTime;

                    db.YouTubeRecords.Update(existingRecord);
                }

                await db.SaveChangesAsync();
            }
        }

        async Task<Embed> SendMessageAsync(YouTubeToCheck youTubeToCheck, YouTubeVideoListItem video)
        {
            var channel = (_client.GetChannel((ulong)youTubeToCheck.DiscordChannelId)) as SocketTextChannel;

            var embed = await CreateEmbedAsync(video);
            if (embed == null)
            {
                await channel.SendMessageAsync("Something broke when posting a new YouTube video. Bug vaindil about it. (error: e)");
                return null;
            }

            await channel.SendMessageAsync($"{video.Snippet.ChannelTitle} posted a new YouTube video.", embed: embed);
            return embed;
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

            var publishedAt = TimeZoneInfo.ConvertTime(video.Snippet.PublishedAt, _tz);

            var footer = new EmbedFooterBuilder
            {
                Text = "Posted on " +
                publishedAt.ToString("MMM d, yyyy") +
                " at " +
                publishedAt.ToString("H:mm") +
                " Central"
            };

            var shortDescription = video.Snippet.Description;

            var lineBreakIndex = video.Snippet.Description.IndexOf("\n");
            if (lineBreakIndex != -1)
                shortDescription = shortDescription.Substring(0, lineBreakIndex);

            if (shortDescription.Length > 500)
                shortDescription = shortDescription.Substring(0, 500) + "...";

            embed.Author = author;
            embed.Footer = footer;
            embed.Color = new Color(205, 32, 31);
            embed.ImageUrl = video.Snippet.Thumbnails.Standard.Url;
            embed.Title = video.Snippet.Title;
            embed.Description = shortDescription;
            embed.Url = "https://www.youtube.com/watch?v=" + video.Id;

            return embed.Build();
        }
    }
}
