using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VainBotDiscord
{
    public class ServerMainUser
    {
        public long DiscordServerId { get; set; }

        public string FriendlyUsername { get; set; }

        public long? TwitchUserId { get; set; }

        public string StreamUrl { get; set; }

        public string YouTubeChannelId { get; set; }

        public string YouTubePlaylistId { get; set; }

        public long? TwitterUserId { get; set; }
    }

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

    public class StreamLastOnline
    {
        public long UserId { get; set; }

        public string FriendlyUsername { get; set; }

        public DateTime LastOnlineAt { get; set; }

        public string LastGame { get; set; }
    }

    public class CatFacts
    {
        public List<string> Facts { get; set; }
    }

    public class MemeResult
    {
        public List<Meme> Result { get; set; }
    }

    public class Meme
    {
        [JsonProperty(PropertyName = "instanceImageUrl")]
        public string InstanceImageUrl { get; set; }
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

    public class WowTokenResponse
    {
        [JsonProperty(PropertyName = "NA")]
        public WowTokenWrapper Na { get; set; }

        [JsonProperty(PropertyName = "EU")]
        public WowTokenWrapper Eu { get; set; }
    }

    public class WowTokenWrapper
    {
        public long Timestamp { get; set; }

        public RawWowToken Raw { get; set; }

        public FormattedWowToken Formatted { get; set; }
    }

    public class RawWowToken
    {
        public long Buy { get; set; }

        [JsonProperty(PropertyName = "24min")]
        public long Min24 { get; set; }

        [JsonProperty(PropertyName = "24max")]
        public long Max24 { get; set; }

        [JsonProperty(PropertyName = "timeToSell")]
        public long TimeToSell { get; set; }

        public int Result { get; set; }

        public long Updated { get; set; }

        [JsonProperty(PropertyName = "updatedISO8601")]
        public DateTime UpdatedIso8601 { get; set; }
    }

    public class FormattedWowToken
    {
        public string Buy { get; set; }

        [JsonProperty(PropertyName = "24min")]
        public string Min24 { get; set; }

        [JsonProperty(PropertyName = "24max")]
        public string Max24 { get; set; }

        [JsonProperty(PropertyName = "24pct")]
        public decimal Pct24 { get; set; }

        [JsonProperty(PropertyName = "timeToSell")]
        public string TimeToSell { get; set; }

        public string Result { get; set; }

        public string Updated { get; set; }

        [JsonProperty(PropertyName = "updatedhtml")]
        public string UpdatedHtml { get; set; }

        [JsonProperty(PropertyName = "sparkurl")]
        public string SparkUrl { get; set; }

        public string Region { get; set; }
    }

    public enum DbKey
    {
        DiscordApiKey,
        TwitchClientId,
        CrendorServerId,
        PointsNameSingular,
        PointsNamePlural,
        LolCounter,
        BettingAllowed,
        LastNaTokenUpdate,
        LastEuTokenUpdate,
        LastNaTokenPrice,
        LastEuTokenPrice
    }
}
