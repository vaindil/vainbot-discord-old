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
    public class YouTubeModule : ModuleBase<VbCommandContext>
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
            if (!Context.HasMainUser
                || Context.MainUser.YouTubeChannelId == null
                || !_throttler.CommandAllowed(ThrottleTypes.YouTube, Context.Channel.Id))
                return;

            YouTubeRecord record;

            using (var db = new VbContext())
            {
                record = await db.YouTubeRecords.FirstOrDefaultAsync(y => y.PlaylistId == Context.MainUser.YouTubePlaylistId);
            }

            if (record == null)
            {
                await ReplyAsync("BOT IS ALL CUCKED UP! Pester vaindil about it.");
                return;
            }

            record.PublishedAt = DateTime.SpecifyKind(record.PublishedAt, DateTimeKind.Utc);

            var embed = CreateEmbed(record);

            await ReplyAsync("", embed: embed);
            _throttler.Throttle(ThrottleTypes.YouTube, Context.Channel.Id);
        }

        Embed CreateEmbed(YouTubeRecord record)
        {
            var embed = new EmbedBuilder();
            var author = new EmbedAuthorBuilder
            {
                Name = record.AuthorName,
                Url = new Uri(record.AuthorUrl),
                IconUrl = new Uri(record.AuthorIconUrl)
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
            embed.ImageUrl = new Uri(record.ImageUrl);
            embed.Title = record.VideoTitle;
            embed.Url = new Uri("https://www.youtube.com/watch?v=" + record.VideoId);

            embed.AddField(descriptionField);

            return embed.Build();
        }
    }
}
