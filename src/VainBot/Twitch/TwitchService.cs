using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace VainBotDiscord.Twitch
{
    public class TwitchService
    {
        readonly DiscordSocketClient _client;
        static HttpClient _twitchClient = new HttpClient();
        List<Timer> _timerList;

        public TwitchService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task InitTwitchService()
        {
            _timerList = new List<Timer>();

            // verified to exist in Program.Run()
            var twitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");

            // https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-channel
            _twitchClient.BaseAddress = new Uri("https://api.twitch.tv/kraken/streams/");
            _twitchClient.DefaultRequestHeaders.Accept.Clear();
            _twitchClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));
            _twitchClient.DefaultRequestHeaders.Add("Client-ID", twitchClientId);
            _twitchClient.Timeout = new TimeSpan(0, 0, 8);

            var streamsToCheck = new List<StreamToCheck>();

            using (var db = new VbContext())
            {
                streamsToCheck = await db.StreamsToCheck.ToListAsync();
            }

            if (streamsToCheck == null || streamsToCheck.Count == 0)
                return;

            foreach (var stream in streamsToCheck)
            {
                var t = new Timer(CheckTwitchAsync, stream, new TimeSpan(0, 0, 20), new TimeSpan(0, 0, stream.Frequency));
                _timerList.Add(t);

                if (stream.EmbedColor != 0)
                {
                    var ut = new Timer(UpdateEmbedAsync, stream, new TimeSpan(0, 0, 30), new TimeSpan(0, 20, 0));
                    _timerList.Add(ut);
                }
            }
        }

        async void CheckTwitchAsync(object streamToCheckIn)
        {
            var streamToCheck = (StreamToCheck)streamToCheckIn;

            var stream = await GetTwitchStreamAsync(streamToCheck.UserId);

            StreamRecord existingRecord;

            using (var db = new VbContext())
            {
                existingRecord = await db.StreamRecords.FirstOrDefaultAsync(sr => sr.UserId == streamToCheck.UserId);
            }

            // live and was not previously live
            if (stream != null && existingRecord == null)
            {
                var msgId = await SendMessageAsync(streamToCheck, stream);
                using (var db = new VbContext())
                {
                    var record = new StreamRecord
                    {
                        UserId = stream.Channel.Id,
                        StreamId = stream.Id,
                        DiscordMessageId = msgId
                    };

                    db.StreamRecords.Add(record);
                    await db.SaveChangesAsync();
                }

                return;
            }

            // not live and was previously live
            if (stream == null && existingRecord != null)
            {
                var msgId = existingRecord.DiscordMessageId;
                if (streamToCheck.DeleteDiscordMessage && existingRecord.DiscordMessageId != 0)
                {
                    var channel = _client.GetChannel((ulong)streamToCheck.DiscordChannelId) as SocketTextChannel;
                    var msg = (await channel.GetMessageAsync((ulong)existingRecord.DiscordMessageId)) as RestUserMessage;

                    await msg.DeleteAsync();
                }

                using (var db = new VbContext())
                {
                    db.StreamRecords.Remove(existingRecord);
                    await db.SaveChangesAsync();
                }
            }
        }

        async Task<long> SendMessageAsync(StreamToCheck streamToCheck, TwitchStream stream)
        {
            Embed embed = null;

            if (streamToCheck.EmbedColor != 0)
                embed = CreateEmbed(stream, (uint)streamToCheck.EmbedColor);

            var channel = (_client.GetChannel((ulong)streamToCheck.DiscordChannelId)) as SocketTextChannel;
            var resp = await channel.SendMessageAsync(streamToCheck.DiscordMessage, embed: embed);

            return (long)resp.Id;
        }

        async void UpdateEmbedAsync(object streamToCheckIn)
        {
            var streamToCheck = (StreamToCheck)streamToCheckIn;
            var stream = await GetTwitchStreamAsync(streamToCheck.UserId);
            long msgId = 0;

            using (var db = new VbContext())
            {
                var record = await db.StreamRecords.FirstOrDefaultAsync(sr => sr.UserId == streamToCheck.UserId);
                if (record == null)
                    return;

                msgId = record.DiscordMessageId;
            }

            var embed = CreateEmbed(stream, (uint)streamToCheck.EmbedColor);

            var channel = _client.GetChannel((ulong)streamToCheck.DiscordChannelId) as SocketTextChannel;
            var msg = await channel.GetMessageAsync((ulong)msgId) as RestUserMessage;

            await msg.ModifyAsync(f => f.Embed = embed, null);
        }

        async Task<TwitchStream> GetTwitchStreamAsync(long userId)
        {
            var response = await _twitchClient.GetAsync(userId.ToString());
            var responseString = await response.Content.ReadAsStringAsync();
            var streamResponse = JsonConvert.DeserializeObject<TwitchStreamResponse>(responseString, Extensions.GetJsonSettings());

            return streamResponse.Stream;
        }

        Embed CreateEmbed(TwitchStream stream, uint inColor)
        {
            var embed = new EmbedBuilder();
            var color = new Color(inColor);
            var author = new EmbedAuthorBuilder();
            var imgUrl = stream.Preview.Template.Replace("{width}", "640").Replace("{height}", "360") + "?" +
                DateTime.UtcNow.Ticks.ToString();

            author.Name = stream.Channel.DisplayName ?? stream.Channel.Name;
            author.Url = stream.Channel.Url;
            author.IconUrl = stream.Channel.Logo;

            var streamPlayingField = new EmbedFieldBuilder
            {
                Name = "Playing",
                Value = stream.Game,
                IsInline = true
            };

            var streamViewersField = new EmbedFieldBuilder
            {
                Name = "Viewers",
                Value = stream.Viewers.ToString(),
                IsInline = true
            };

            embed.Author = author;
            embed.Color = color;
            embed.ImageUrl = imgUrl;
            embed.Title = stream.Channel.Status;
            embed.Url = stream.Channel.Url;

            embed.AddField(streamPlayingField);
            embed.AddField(streamViewersField);

            return embed.Build();
        }
    }
}
