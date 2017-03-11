using Discord.Commands;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class CrendorServerOnlyAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context,
                                                                        CommandInfo command,
                                                                        IDependencyMap map)
        {
            if (context.Guild.Id == 149051954348294145)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("");
        }
    }
}
