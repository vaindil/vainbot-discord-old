using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("lol")]
    [CrendorServerOnly]
    public class LolModule : ModuleBase<VbCommandContext>
    {
        [Command]
        public async Task LolCount()
        {
            int count;

            using (var db = new VbContext())
            {
                count = int.Parse(await db.KeyValues.GetValueAsync(DbKey.LolCounter));
            }

            var user = await Context.Client.GetUserAsync(110878826136907776);
            await ReplyAsync($"{user.Username} lol counter: {count}");
        }
    }
}
