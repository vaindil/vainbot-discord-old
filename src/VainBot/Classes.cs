using System;

namespace VainBot
{
    public class UserPoints
    {
        public ulong ServerId { get; set; }

        public ulong UserId { get; set; }

        public decimal Points { get; set; }

        public bool Allow { get; set; }
    }

    public class Admin
    {
        public ulong ServerId { get; set; }

        public ulong UserId { get; set; }
    }

    public class KeyValue
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    public enum DbKey
    {
        DiscordApiKey,
        CrendorServerId,
        PointsNameSingular,
        PointsNamePlural
    }
}
