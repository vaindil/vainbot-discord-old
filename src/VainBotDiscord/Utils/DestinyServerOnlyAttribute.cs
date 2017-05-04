using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class DestinyServerOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context,
                                                                        CommandInfo command,
                                                                        IServiceProvider services)
        {
            if (context.Guild.Id == 265256381437706240)
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError(""));
        }
    }
}
