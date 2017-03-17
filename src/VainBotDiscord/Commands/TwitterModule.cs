﻿using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VainBotDiscord.Twitter;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("twitter")]
    [Alias("tweet", "tweets")]
    [DestinyServerOnly]
    public class TwitterModule : ModuleBase
    {
        readonly ThrottlerService _throttler;
        readonly TimeZoneInfo _tz;

        public TwitterModule(ThrottlerService throttler, TimeZoneInfo tz)
        {
            _throttler = throttler;
            _tz = tz;
        }

        [Command]
        public async Task GetTwitter([Remainder]string unused = null)
        {
            if (!_throttler.CommandAllowed(ThrottleTypes.Twitter, Context.Channel.Id))
                return;

            TweetRecord record;

            using (var db = new VbContext())
            {
                record = await db.TweetRecords.FirstOrDefaultAsync(y => y.UserId == 4726147296);
            }

            if (record == null)
            {
                await Context.Channel.SendMessageAsync("BOT IS ALL CUCKED UP! Pester vaindil about it.");
                return;
            }

            record.CreatedAt = DateTime.SpecifyKind(record.CreatedAt, DateTimeKind.Utc);

            var embed = CreateEmbed(record);

            await Context.Channel.SendMessageAsync("", embed: embed);
            _throttler.Throttle(ThrottleTypes.Twitter, Context.Channel.Id);
        }

        Embed CreateEmbed(TweetRecord tweet)
        {
            var embed = new EmbedBuilder();

            var author = new EmbedAuthorBuilder
            {
                Name = tweet.AuthorUsername + " (" + tweet.AuthorName + ")",
                Url = "https://twitter.com/" + tweet.AuthorUsername,
                IconUrl = tweet.ProfileImageUrl
            };

            var createdAt = TimeZoneInfo.ConvertTime(tweet.CreatedAt, _tz);

            var footer = new EmbedFooterBuilder
            {
                Text = "Posted on " +
                    createdAt.ToString("MMM d, yyyy") +
                    " at " +
                    createdAt.ToString("H:mm") +
                    " Central"
            };

            embed.Title = "Go to tweet";
            embed.Description = tweet.Text;
            embed.Url = "https://twitter.com/" + tweet.AuthorUsername + "/status/" + tweet.TweetId;
            embed.Color = new Color(29, 161, 242);
            embed.Author = author;
            embed.Footer = footer;

            return embed.Build();
        }
    }
}
