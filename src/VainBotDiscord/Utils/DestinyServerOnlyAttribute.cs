using Discord.Commands;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class DestinyServerOnlyAttribute : PreconditionAttribute
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                                                                        CommandInfo command,
                                                                        IDependencyMap map)
        {
            if (context.Guild.Id == 265256381437706240)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("");
        }
    }
}
