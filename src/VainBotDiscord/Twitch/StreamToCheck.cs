namespace VainBotDiscord.Twitch
{
    public class StreamToCheck
    {
        public long UserId { get; set; }

        public string FriendlyUsername { get; set; }

        public int Frequency { get; set; }

        public long DiscordServerId { get; set; }

        public long DiscordChannelId { get; set; }

        public string DiscordMessage { get; set; }

        public bool DeleteDiscordMessage { get; set; }

        /// <summary>
        /// Set to 0 to disable embedding
        /// </summary>
        public int EmbedColor { get; set; }
    }
}
