using Discord.Commands;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class IsOwnerAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context,
                                                                        CommandInfo command,
                                                                        IDependencyMap map)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();

            if (appInfo.Owner.Id == context.Message.Author.Id)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Only bot admins can do that, you nerd.");
        }
    }
}
