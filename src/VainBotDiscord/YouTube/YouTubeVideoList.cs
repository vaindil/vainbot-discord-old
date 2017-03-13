using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VainBotDiscord.YouTube
{
    public class YouTubeVideoList
    {
        public List<YouTubeVideoListItem> Items { get; set; }
    }

    public class YouTubeVideoListItem
    {
        public string Id { get; set; }

        public YouTubeVideoSnippet Snippet { get; set; }
    }

    public class YouTubeVideoSnippet
    {
        [JsonProperty(PropertyName = "publishedAt")]
        public DateTime PublishedAt { get; set; }

        public string ChannelId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public YouTubeVideoThumbnailList Thumbnails { get; set; }

        [JsonProperty(PropertyName = "channelTitle")]
        public string ChannelTitle { get; set; }
    }

    public class YouTubeVideoThumbnailList
    {
        public YouTubeVideoThumbnail Default { get; set; }

        public YouTubeVideoThumbnail Medium { get; set; }

        public YouTubeVideoThumbnail High { get; set; }

        public YouTubeVideoThumbnail Standard { get; set; }

        public YouTubeVideoThumbnail Maxres { get; set; }

    }

    public class YouTubeVideoThumbnail
    {
        public string Url { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
