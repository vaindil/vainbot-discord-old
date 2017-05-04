using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace VainBotDiscord.Utils
{
    public class IsAdminAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context,
                                                                        CommandInfo command,
                                                                        IServiceProvider services)
        {
            using (var db = new VbContext())
            {
                var isAdmin = await db.Admins
                    .AnyAsync(a => a.ServerId == context.Guild.Id && a.UserId == context.Message.Author.Id);

                if (isAdmin)
                    return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError("Only bot admins can do that, you nerd.");
        }
    }
}
