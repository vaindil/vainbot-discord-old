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
        public DbKey Key
        {
            get
            {
                return (DbKey)Enum.Parse(typeof(DbKey), KeyString);
            }
            set
            {
                KeyString = value.ToString();
            }
        }

        public string KeyString { get; set; }
        

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
