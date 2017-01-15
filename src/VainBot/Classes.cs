using System;

namespace VainBot
{
    public class UserPoints
    {
        public ulong ServerId { get; set; }

        public ulong UserId { get; set; }

        public double Points { get; set; }

        public bool Allow { get; set; }
    }

    public class Admin
    {
        public ulong ServerId { get; set; }

        public ulong UserId { get; set; }
    }

    public class KeyValue
    {
        public DbKey Key { get; set; }

        public string KeyString
        {
            get
            {
                return Key.ToString();
            }
            set
            {
                Key = (DbKey)Enum.Parse(typeof(DbKey), value);
            }
        }

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
