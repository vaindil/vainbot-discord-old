using Discord.Commands;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    [Group("twitchid")]
    public class TwitchIdModule : ModuleBase<VbCommandContext>
    {
        readonly HttpClient _client;

        public TwitchIdModule(HttpClient client)
        {
            _client = client;
        }

        [Command]
        public async Task GetId(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                await ReplyAsync("That username is empty.");
                return;
            }

            if (username.Length > 20)
            {
                await ReplyAsync("No Twitch username is that long.");
                return;
            }

            string clientId;

            using (var db = new VbContext())
                clientId = await db.KeyValues.GetValueAsync(DbKey.TwitchClientId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.twitch.tv/kraken/users?login=" + username);

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));
            request.Headers.Add("Client-ID", clientId);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await ReplyAsync("Error calling the Twitch API. Sucks to be you.");
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var twitchUser = JsonConvert.DeserializeObject<TwitchUserResponse>(responseContent, Extensions.GetJsonSettings());
            if (twitchUser == null || twitchUser.Users == null || twitchUser.Users.Count != 1)
            {
                await ReplyAsync("The Twitch response isn't what I was expecting. Go bug vaindil about it.");
                return;
            }

            await ReplyAsync($"{twitchUser.Users[0].DisplayName}: ID is {twitchUser.Users[0].Id}");
        }
    }
}
