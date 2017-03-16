using System;

namespace VainBotDiscord.YouTube
{
    public class YouTubeRecord
    {
        public string PlaylistId { get; set; }

        public string VideoId { get; set; }

        public string VideoTitle { get; set; }

        public string VideoDescription { get; set; }

        public string AuthorName { get; set; }

        public string AuthorUrl { get; set; }

        public string AuthorIconUrl { get; set; }

        public string ImageUrl { get; set; }

        public DateTime PublishedAt { get; set; }
    }
}
