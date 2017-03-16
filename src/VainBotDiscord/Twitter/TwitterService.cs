using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VainBotDiscord.Twitter
{
    public class TwitterService
    {
        readonly string _consumerKey;
        readonly string _consumerSecret;
        readonly string _accessToken;
        readonly string _accessTokenSecret;

        readonly string _method = "GET";
        readonly string _path = "https://api.twitter.com/1.1/statuses/user_timeline.json";

        readonly DiscordSocketClient _client;
        static HttpClient _httpClient = new HttpClient();
        List<Timer> _timerList;

        public TwitterService(DiscordSocketClient client)
        {
            _consumerKey = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY");
            _consumerSecret = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET");
            _accessToken = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");
            _accessTokenSecret = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN_SECRET");

            _client = client;
        }

        public async Task InitTwitterServiceAsync()
        {
            _timerList = new List<Timer>();

            _httpClient.Timeout = new TimeSpan(0, 0, 8);

            var twittersToCheck = new List<TwitterToCheck>();
            using (var db = new VbContext())
            {
                twittersToCheck = await db.TwittersToCheck.ToListAsync();
            }

            if (twittersToCheck == null || twittersToCheck.Count == 0)
                return;

            foreach (var t in twittersToCheck)
            {
                var timer = new Timer(CheckTwitterAsync, t, new TimeSpan(0, 0, 5), new TimeSpan(0, 0, t.Frequency));
                _timerList.Add(timer);
            }
        }

        async void CheckTwitterAsync(object twitterToCheckIn)
        {
            var twitterToCheck = (TwitterToCheck)twitterToCheckIn;
            var channel = (_client.GetChannel((ulong)twitterToCheck.DiscordChannelId)) as SocketTextChannel;

            TweetRecord existing;

            using (var db = new VbContext())
            {
                existing = await db.TweetRecords.FirstOrDefaultAsync(r => r.UserId == twitterToCheck.UserId);
            }

            List<Tweet> tweets;

            if (existing == null)
                tweets = await GetTweetsAsync(twitterToCheck.UserId, null);
            else
                tweets = await GetTweetsAsync(twitterToCheck.UserId, existing.TweetId);

            if (tweets.Count == 0)
                return;

            Tweet latestTweet;

            if (existing == null)
                latestTweet = tweets[0];
            else
                latestTweet = tweets[tweets.Count - 1];

            for (var i = 0; i < tweets.Count; i++)
            {
                var tweet = tweets[i];

                if (tweet.Text.StartsWith("@"))
                {
                    tweets.RemoveAt(i);
                    i--;
                    continue;
                }

                var embed = CreateEmbed(tweet);
                await channel.SendMessageAsync("", embed: embed);

                if (existing == null)
                    break;
            }

            if (tweets.Count == 0 || (existing != null && existing.TweetId == latestTweet.Id))
                return;

            using (var db = new VbContext())
            {
                if (existing == null)
                {
                    existing = new TweetRecord
                    {
                        UserId = latestTweet.User.Id,
                        TweetId = latestTweet.Id,
                        Text = latestTweet.Text,
                        AuthorName = latestTweet.User.Name,
                        AuthorUsername = latestTweet.User.Username,
                        ProfileImageUrl = latestTweet.User.ProfileImageUrl,
                        CreatedAt = latestTweet.CreatedAt.UtcDateTime
                    };

                    db.TweetRecords.Add(existing);
                }
                else
                {
                    existing.TweetId = latestTweet.Id;
                    existing.Text = latestTweet.Text;
                    existing.AuthorName = latestTweet.User.Name;
                    existing.AuthorUsername = latestTweet.User.Username;
                    existing.ProfileImageUrl = latestTweet.User.ProfileImageUrl;
                    existing.CreatedAt = latestTweet.CreatedAt.UtcDateTime;

                    db.TweetRecords.Update(existing);
                }

                await db.SaveChangesAsync();
            }
        }

        async Task<List<Tweet>> GetTweetsAsync(long userId, long? sinceId)
        {
            var path = _path + "?user_id=" + userId + "&count=100";
            if (sinceId.HasValue)
                path += "&since_id=" + sinceId.Value.ToString();

            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", GenerateAuthHeader(userId, sinceId));

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var tweets = JsonConvert.DeserializeObject<List<Tweet>>(responseString);

            if (sinceId.HasValue)
                tweets.Reverse();

            return tweets;
        }

        Embed CreateEmbed(Tweet tweet)
        {
            var embed = new EmbedBuilder();

            var author = new EmbedAuthorBuilder
            {
                Name = tweet.User.Username + " (" + tweet.User.Name + ")",
                Url = "https://twitter.com/" + tweet.User.Username,
                IconUrl = tweet.User.ProfileImageUrl
            };

            var footer = new EmbedFooterBuilder
            {
                Text = "Posted on " +
                    tweet.CreatedAt.ToString("MMM d, yyyy") +
                    " at " +
                    tweet.CreatedAt.ToString("H:mm")
            };

            embed.Title = "Go to tweet";
            embed.Description = tweet.Text;
            embed.Url = "https://twitter.com/" + tweet.User.Username + "/status/" + tweet.Id;
            embed.Color = new Color(29, 161, 242);
            embed.Author = author;
            embed.Footer = footer;

            return embed.Build();
        }

        string GenerateAuthHeader(long userId, long? sinceId)
        {
            var nonce =
                Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)));

            var timespan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var timestamp = Convert.ToInt64(timespan.TotalSeconds).ToString(CultureInfo.InvariantCulture);

            var signature = GenerateSignature(userId, sinceId, nonce, timestamp);

            var sb = new StringBuilder();

            sb.Append("oauth_nonce=\"");
            sb.Append(Uri.EscapeDataString(nonce));
            sb.Append("\",");

            sb.Append("oauth_signature_method=\"");
            sb.Append(Uri.EscapeDataString("HMAC-SHA1"));
            sb.Append("\",");

            sb.Append("oauth_timestamp=\"");
            sb.Append(Uri.EscapeDataString(timestamp));
            sb.Append("\",");

            sb.Append("oauth_consumer_key=\"");
            sb.Append(Uri.EscapeDataString(_consumerKey));
            sb.Append("\",");

            sb.Append("oauth_token=\"");
            sb.Append(Uri.EscapeDataString(_accessToken));
            sb.Append("\",");

            sb.Append("oauth_signature=\"");
            sb.Append(Uri.EscapeDataString(signature));
            sb.Append("\",");

            sb.Append("oauth_version=\"");
            sb.Append(Uri.EscapeDataString("1.0"));
            sb.Append("\"");

            return sb.ToString();
        }

        string GenerateSignature(long userId, long? sinceId, string nonce, string timestamp)
        {
            var keyValues = new SortedDictionary<string, string>
            {
                { "user_id", userId.ToString() },
                { "count", "100" },
                { "oauth_consumer_key", _consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", _accessToken },
                { "oauth_version", "1.0" }
            };

            if (sinceId.HasValue)
                keyValues.Add("since_id", sinceId.Value.ToString());

            var sb = new StringBuilder();

            sb.Append(_method);
            sb.Append("&");
            sb.Append(Uri.EscapeDataString(_path));
            sb.Append("&");

            foreach (var kv in keyValues)
            {
                sb.Append(Uri.EscapeDataString(string.Format("{0}={1}&", kv.Key, kv.Value)));
            }

            var sigBase = sb.ToString().Substring(0, sb.Length - 3);
            var sigKey = Uri.EscapeDataString(_consumerSecret) + "&" + Uri.EscapeDataString(_accessTokenSecret);
            var hmacsha1 = new HMACSHA1(new ASCIIEncoding().GetBytes(sigKey));

            return Convert.ToBase64String(hmacsha1.ComputeHash(new ASCIIEncoding().GetBytes(sigBase)));
        }
    }
}
