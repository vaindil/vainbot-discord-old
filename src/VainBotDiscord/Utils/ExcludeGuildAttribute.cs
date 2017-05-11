using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class ExcludeGuildAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context,
                                                                  ParameterInfo parameter,
                                                                  object value,
                                                                  IServiceProvider services)
        {
            var guildList = value as ulong[];
            if (guildList.Contains(context.Guild.Id))
                return Task.FromResult(PreconditionResult.FromError("Command is prohibited in this guild."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
