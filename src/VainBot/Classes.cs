using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

    public class CatFacts
    {
        public List<string> Facts { get; set; }
    }

    public class TwitchUserResponse
    {
        [JsonProperty(PropertyName = "_total")]
        public int Total { get; set; }

        public List<TwitchUser> Users { get; set; }
    }

    public class TwitchUser
    {
        [JsonProperty(PropertyName = "_id")]
        public long Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string Logo { get; set; }

        public string DisplayName { get; set; }

        public string Bio { get; set; }
    }

    public enum DbKey
    {
        DiscordApiKey,
        TwitchClientId,
        CrendorServerId,
        PointsNameSingular,
        PointsNamePlural,
        LolCounter,
        BettingAllowed
    }
}
