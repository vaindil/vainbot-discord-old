namespace VainBotDiscord.YouTube
{
    public class YouTubeToCheck
    {
        public string PlaylistId { get; set; }

        public string ChannelName { get; set; }

        public int Frequency { get; set; }

        public long DiscordServerId { get; set; }

        public long DiscordChannelId { get; set; }

        public string DiscordMessage { get; set; }
    }
}
