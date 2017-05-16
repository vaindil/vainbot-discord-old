using Newtonsoft.Json;
using System;
using System.Globalization;

namespace VainBotDiscord.Twitter
{
    public class Tweet
    {
        public long Id { get; set; }

        public string Text { get; set; }

        [JsonConverter(typeof(TwitterDateTimeOffsetConverter))]
        [JsonProperty(PropertyName = "created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        public TweetUser User { get; set; }
    }

    public class TweetUser
    {
        public long Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(PropertyName = "screen_name")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "profile_image_url_https")]
        public string ProfileImageUrl { get; set; }
    }

    public class TwitterDateTimeOffsetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dstring = (string)reader.Value;

            existingValue = DateTimeOffset.ParseExact(dstring, "ddd MMM dd HH:mm:ss +0000 yyyy",
                                                      DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            return existingValue;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DateTimeOffset) == objectType;
        }
    }
}
