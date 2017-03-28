﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Twitch
{
    public class TwitchService
    {
        readonly DiscordSocketClient _client;
        readonly TimeZoneInfo _tz;

        static HttpClient _twitchClient;
        List<TwitchCheckTimer> _checkTimerList;
        List<TwitchUpdateTimer> _updateTimerList;

        public TwitchService(DiscordSocketClient client, TimeZoneInfo tz)
        {
            _client = client;
            _tz = tz;
        }

        public async Task InitTwitchServiceAsync()
        {
            _twitchClient = new HttpClient();
            _checkTimerList = new List<TwitchCheckTimer>();
            _updateTimerList = new List<TwitchUpdateTimer>();

            // verified to exist in Program.Run()
            var twitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");

            // https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-channel
            _twitchClient.BaseAddress = new Uri("https://api.twitch.tv/kraken/streams/");
            _twitchClient.DefaultRequestHeaders.Accept.Clear();
            _twitchClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));
            _twitchClient.DefaultRequestHeaders.Add("Client-ID", twitchClientId);
            _twitchClient.Timeout = new TimeSpan(0, 0, 8);

            var streamsToCheck = new List<StreamToCheck>();
            var streamRecords = new List<StreamRecord>();

            using (var db = new VbContext())
            {
                streamsToCheck = await db.StreamsToCheck.ToListAsync();
                streamRecords = await db.StreamRecords.ToListAsync();
            }

            if (streamsToCheck == null || streamsToCheck.Count == 0)
                return;

            foreach (var stream in streamsToCheck)
            {
                var t = new Timer(CheckTwitchAsync, stream, new TimeSpan(0, 0, 5), new TimeSpan(0, 0, stream.Frequency));
                var checkTimer = new TwitchCheckTimer(stream.UserId, t);
                _checkTimerList.Add(checkTimer);
            }

            foreach (var record in streamRecords)
            {
                var thisStreamToCheck = streamsToCheck.FirstOrDefault(s => s.UserId == record.UserId);

                if (thisStreamToCheck.EmbedColor != 0)
                {
                    var ut = new Timer(UpdateEmbedAsync, thisStreamToCheck, new TimeSpan(0, 0, 30), new TimeSpan(0, 1, 0));
                    var updateTimer = new TwitchUpdateTimer(record.StreamId, ut);
                    _updateTimerList.Add(updateTimer);
                }
            }
        }

        public async Task ReloadAsync()
        {
            foreach (var u in _updateTimerList)
            {
                u.Timer.Dispose();
            }

            _updateTimerList.Clear();

            foreach (var c in _checkTimerList)
            {
                c.Timer.Dispose();
            }

            _checkTimerList.Clear();

            await InitTwitchServiceAsync();
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
                    db.StreamRecords.Add(new StreamRecord
                    {
                        UserId = stream.Channel.Id,
                        StreamId = stream.Id,
                        DiscordMessageId = msgId,
                        CurrentGame = stream.Game,
                        StartTime = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();

                    db.StreamGames.Add(new StreamGame
                    {
                        Game = stream.Game,
                        StreamId = stream.Id,
                        StartTime = DateTime.UtcNow
                    });

                    var lastOnlines = db.StreamLastOnlines.Where(s => s.UserId == streamToCheck.UserId);
                    db.StreamLastOnlines.RemoveRange(lastOnlines);

                    await db.SaveChangesAsync();
                }

                if (streamToCheck.EmbedColor != 0)
                {
                    var ut = new Timer(UpdateEmbedAsync, streamToCheck, new TimeSpan(0, 0, 30), new TimeSpan(0, 1, 0));
                    var updateTimer = new TwitchUpdateTimer(stream.Id, ut);
                    _updateTimerList.Add(updateTimer);
                }

                return;
            }

            // not live and was previously live
            if (stream == null && existingRecord != null)
            {
                var channel = _client.GetChannel((ulong)streamToCheck.DiscordChannelId) as SocketTextChannel;
                var msgId = existingRecord.DiscordMessageId;

                if (streamToCheck.DeleteDiscordMessage && existingRecord.DiscordMessageId != 0)
                {
                    var msg = (await channel.GetMessageAsync((ulong)existingRecord.DiscordMessageId)) as RestUserMessage;

                    await msg.DeleteAsync();
                }
                else if (!streamToCheck.DeleteDiscordMessage && streamToCheck.EmbedColor != 0 && existingRecord.DiscordMessageId != 0)
                {
                    await FinalMessageUpdateAsync(streamToCheck, existingRecord);
                }

                if (streamToCheck.PinMessage && existingRecord.DiscordMessageId != 0)
                {
                    var msg = (await channel.GetMessageAsync((ulong)existingRecord.DiscordMessageId)) as RestUserMessage;
                    await msg.UnpinAsync();
                }

                using (var db = new VbContext())
                {
                    db.StreamLastOnlines.Add(new StreamLastOnline
                    {
                        UserId = streamToCheck.UserId,
                        FriendlyUsername = streamToCheck.FriendlyUsername,
                        LastOnlineAt = DateTime.UtcNow,
                        LastGame = existingRecord.CurrentGame
                    });

                    var games = db.StreamGames.Where(g => g.StreamId == existingRecord.StreamId);
                    db.StreamGames.RemoveRange(games);

                    await db.SaveChangesAsync();

                    db.StreamRecords.Remove(existingRecord);
                    await db.SaveChangesAsync();
                }

                var updateTimer = _updateTimerList.FirstOrDefault(t => t.StreamId == existingRecord.StreamId);
                if (updateTimer == null)
                    return;

                updateTimer.Timer.Dispose();
                _updateTimerList.Remove(updateTimer);
            }
        }

        async Task<long> SendMessageAsync(StreamToCheck streamToCheck, TwitchStream stream)
        {
            Embed embed = null;

            if (streamToCheck.EmbedColor != 0)
                embed = CreateEmbed(stream, (uint)streamToCheck.EmbedColor);

            var channel = (_client.GetChannel((ulong)streamToCheck.DiscordChannelId)) as SocketTextChannel;
            var resp = await channel.SendMessageAsync(streamToCheck.DiscordMessage, embed: embed);

            if (streamToCheck.PinMessage)
                await resp.PinAsync();

            return (long)resp.Id;
        }

        async void UpdateEmbedAsync(object streamToCheckIn)
        {
            var streamToCheck = (StreamToCheck)streamToCheckIn;
            StreamRecord record;

            using (var db = new VbContext())
            {
                record = await db.StreamRecords.FirstOrDefaultAsync(sr => sr.UserId == streamToCheck.UserId);
                if (record == null)
                    return;
            }

            var stream = await GetTwitchStreamAsync(streamToCheck.UserId);

            if (string.IsNullOrEmpty(stream.Game))
                stream.Game = "(no game)";
            
            if (record.CurrentGame != stream.Game)
            {
                using (var db = new VbContext())
                {
                    var streamGame = await db.StreamGames
                        .FirstOrDefaultAsync(g => g.StreamId == stream.Id && g.StopTime == null);

                    streamGame.StopTime = DateTime.UtcNow;
                    record.CurrentGame = stream.Game;

                    db.StreamGames.Add(new StreamGame
                    {
                        StreamId = stream.Id,
                        Game = stream.Game,
                        StartTime = DateTime.UtcNow
                    });

                    db.StreamRecords.Update(record);

                    await db.SaveChangesAsync();
                }
            }

            var embed = CreateEmbed(stream, (uint)streamToCheck.EmbedColor);

            var channel = _client.GetChannel((ulong)streamToCheck.DiscordChannelId) as SocketTextChannel;
            var msg = await channel.GetMessageAsync((ulong)record.DiscordMessageId) as RestUserMessage;

            await msg.ModifyAsync(f => f.Embed = embed, null);
        }

        async Task FinalMessageUpdateAsync(StreamToCheck streamToCheck, StreamRecord record)
        {
            List<StreamGame> games;

            using (var db = new VbContext())
            {
                var latest = await db.StreamGames
                    .FirstOrDefaultAsync(g => g.StreamId == record.StreamId && g.StopTime == null);

                if (latest != null)
                {
                    latest.StopTime = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
                
                games = await db.StreamGames.Where(g => g.StreamId == record.StreamId).ToListAsync();
            }

            record.StartTime = DateTime.SpecifyKind(record.StartTime, DateTimeKind.Utc);

            var streamDuration = DateTime.UtcNow - record.StartTime;
            var startTime = TimeZoneInfo.ConvertTime(record.StartTime, _tz);
            var stopTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);

            var msg = new StringBuilder(streamToCheck.FriendlyUsername + " was live.\n\n");
            msg.Append("**Started at:** " + startTime.ToString("HH:mm") + " Central\n");
            msg.Append("**Ended at:** " + stopTime.ToString("HH:mm") + " Central\n");
            msg.Append($"_(total of {streamDuration.ToFriendlyString()})_\n\n");

            msg.Append("__Games Played__");

            foreach (var g in games)
            {
                var duration = g.StopTime.Value - g.StartTime;
                msg.Append($"\n**{g.Game}:** {duration.ToFriendlyString()}");
            }

            var channel = (_client.GetChannel((ulong)streamToCheck.DiscordChannelId)) as SocketTextChannel;
            var existingMsg = await channel.GetMessageAsync((ulong)record.DiscordMessageId) as RestUserMessage;

            await existingMsg.ModifyAsync(m =>
            {
                m.Content = msg.ToString();
                m.Embed = null;
            });
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
            var now = DateTime.UtcNow;
            var cacheBuster =
                now.Year.ToString() +
                now.Month.ToString() +
                now.Day.ToString() +
                now.Hour.ToString() +
                ((now.Minute / 10) % 10).ToString();

            var embed = new EmbedBuilder();
            var color = new Color(inColor);
            var author = new EmbedAuthorBuilder();
            var imgUrl = stream.Preview.Template.Replace("{width}", "640").Replace("{height}", "360") + "?" + cacheBuster;

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
            
            embed.Color = color;
            embed.ImageUrl = imgUrl;
            embed.Title = stream.Channel.Status;
            embed.Url = stream.Channel.Url;

            if (stream.Channel.Id == 18074328)
            {
                author.Url = "https://www.destiny.gg/bigscreen";
                embed.Url = "https://www.destiny.gg/bigscreen";
            }

            embed.Author = author;

            embed.AddField(streamPlayingField);
            embed.AddField(streamViewersField);

            return embed.Build();
        }
    }
}
