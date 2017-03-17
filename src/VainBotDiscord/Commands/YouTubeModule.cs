using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VainBotDiscord.Utils;
using VainBotDiscord.YouTube;

namespace VainBotDiscord.Commands
{
    [Group("youtube")]
    [Alias("yt")]
    [DestinyServerOnly]
    public class YouTubeModule : ModuleBase
    {
        readonly ThrottlerService _throttler;
        readonly TimeZoneInfo _tz;

        public YouTubeModule(ThrottlerService throttler, TimeZoneInfo tz)
        {
            _throttler = throttler;
            _tz = tz;
        }

        [Command]
        public async Task GetYouTube([Remainder]string unused = null)
        {
            if (!_throttler.CommandAllowed(ThrottleTypes.YouTube, Context.Channel.Id))
                return;

            YouTubeRecord record;

            using (var db = new VbContext())
            {
                record = await db.YouTubeRecords.FirstOrDefaultAsync(y => y.PlaylistId == "UU554eY5jNUfDq3yDOJYirOQ");
            }

            if (record == null)
            {
                await Context.Channel.SendMessageAsync("BOT IS ALL CUCKED UP! Pester vaindil about it.");
                return;
            }

            record.PublishedAt = DateTime.SpecifyKind(record.PublishedAt, DateTimeKind.Utc);

            var embed = CreateEmbedAsync(record);

            await Context.Channel.SendMessageAsync("", embed: embed);
            _throttler.Throttle(ThrottleTypes.YouTube, Context.Channel.Id);
        }

        Embed CreateEmbedAsync(YouTubeRecord record)
        {
            var embed = new EmbedBuilder();
            var author = new EmbedAuthorBuilder
            {
                Name = record.AuthorName,
                Url = record.AuthorUrl,
                IconUrl = record.AuthorIconUrl
            };

            var publishedAt = TimeZoneInfo.ConvertTime(record.PublishedAt, _tz);

            var footer = new EmbedFooterBuilder
            {
                Text = "Posted on " +
                    publishedAt.ToString("MMM d, yyyy") +
                    " at " +
                    publishedAt.ToString("H:mm") +
                    " Central"
            };

            var descriptionField = new EmbedFieldBuilder
            {
                Name = "Description",
                Value = record.VideoDescription,
                IsInline = false
            };

            embed.Author = author;
            embed.Footer = footer;
            embed.Color = new Color(205, 32, 31);
            embed.ImageUrl = record.ImageUrl;
            embed.Title = record.VideoTitle;
            embed.Url = "https://www.youtube.com/watch?v=" + record.VideoId;

            embed.AddField(descriptionField);

            return embed.Build();
        }
    }
}
