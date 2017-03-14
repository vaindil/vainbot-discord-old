using System.Collections.Generic;

namespace VainBotDiscord.YouTube
{
    public class YouTubeChannelList
    {
        public List<YouTubeChannelListItem> Items { get; set; }
    }

    public class YouTubeChannelListItem
    {
        public string Id { get; set; }

        public YouTubeChannelSnippet Snippet { get; set; }
    }

    public class YouTubeChannelSnippet
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public YouTubeChannelSnippetThumbnails Thumbnails { get; set; }
    }

    public class YouTubeChannelSnippetThumbnails
    {
        public YouTubeChannelSnippetThumbnail Default { get; set; }

        public YouTubeChannelSnippetThumbnail Medium { get; set; }

        public YouTubeChannelSnippetThumbnail High { get; set; }
    }

    public class YouTubeChannelSnippetThumbnail
    {
        public string Url { get; set; }
    }
}
