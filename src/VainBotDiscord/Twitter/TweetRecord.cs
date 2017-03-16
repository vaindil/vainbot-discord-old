using System;

namespace VainBotDiscord.Twitter
{
    public class TweetRecord
    {
        public long UserId { get; set; }

        public long TweetId { get; set; }

        public string Text { get; set; }

        public string TweetUrl { get; set; }

        public string AuthorName { get; set; }

        public string AuthorUsername { get; set; }

        public string ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
