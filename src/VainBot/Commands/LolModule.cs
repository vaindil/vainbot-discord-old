using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("lol")]
    [CrendorServerOnly]
    public class LolModule : ModuleBase
    {
        readonly VbContext _context;

        public LolModule(VbContext context)
        {
            _context = context;
        }

        [Command]
        public async Task LolCount()
        {
            var count = int.Parse(await _context.KeyValues.GetValueAsync(DbKey.LolCounter));
            var user = await Context.Client.GetUserAsync(110878826136907776);

            await ReplyAsync(user.Username + " lol counter: " + count);
        }
    }
}
