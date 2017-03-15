using System;

namespace VainBotDiscord.Twitch
{
    public class StreamGame
    {
        // not used, just need a PK
        public int Id { get; set; }

        public long StreamId { get; set; }

        public string Game { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? StopTime { get; set; }
    }
}
