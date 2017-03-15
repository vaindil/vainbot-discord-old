using System;

namespace VainBotDiscord.Twitch
{
    public class StreamRecord
    {
        public long UserId { get; set; }

        public long StreamId { get; set; }

        public long DiscordMessageId { get; set; }

        public DateTime StartTime { get; set; }

        public string CurrentGame { get; set; }
    }
}
