using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class CrendorServerOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context,
                                                                        CommandInfo command,
                                                                        IServiceProvider services)
        {
            if (context.Guild.Id == 149051954348294145)
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError(""));
        }
    }
}
