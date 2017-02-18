using Newtonsoft.Json;
using System;

namespace VainBotDiscord.Twitch
{
    public class TwitchStreamResponse
    {
        public TwitchStream Stream { get; set; }
    }

    public class TwitchStream
    {
        [JsonProperty(PropertyName = "_id")]
        public long Id { get; set; }

        public string Game { get; set; }

        public int Viewers { get; set; }

        public int VideoHeight { get; set; }

        public decimal AverageFps { get; set; }

        public decimal Delay { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public bool IsPlaylist { get; set; }

        public TwitchStreamPreview Preview { get; set; }

        public TwitchChannel Channel { get; set; }
    }

    public class TwitchStreamPreview
    {
        public string Small { get; set; }

        public string Medium { get; set; }

        public string Large { get; set; }

        public string Template { get; set; }
    }

    public class TwitchChannel
    {
        [JsonProperty(PropertyName = "_id")]
        public long Id { get; set; }

        public bool Mature { get; set; }

        public string Status { get; set; }

        public string BroadcasterLanguage { get; set; }

        public string DisplayName { get; set; }

        public string Game { get; set; }

        public string Language { get; set; }

        public string Name { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public bool Partner { get; set; }

        public string Logo { get; set; }

        public string VideoBanner { get; set; }

        public string ProfileBanner { get; set; }

        public string ProfileBannerBackgroundColor { get; set; }

        public string Url { get; set; }

        public long Views { get; set; }

        public long Followers { get; set; }
    }
}
