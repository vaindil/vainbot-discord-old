using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VainBotDiscord.YouTube
{
    public class YouTubePlaylist
    {
        public List<YouTubePlaylistItem> Items { get; set; }
    }

    public class YouTubePlaylistItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "contentDetails")]
        public YouTubePlaylistItemContentDetails ContentDetails { get; set; }
    }

    public class YouTubePlaylistItemContentDetails
    {
        [JsonProperty(PropertyName = "videoId")]
        public string VideoId { get; set; }

        [JsonProperty(PropertyName = "videoPublishedAt")]
        public DateTime VideoPublishedAt { get; set; }
    }
}
