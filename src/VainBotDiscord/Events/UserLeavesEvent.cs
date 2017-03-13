using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VainBotDiscord.Events
{
    public static class UserLeavesEvent
    {
        public static async Task UserLeavesAsync(SocketGuildUser user)
        {
            using (var db = new VbContext())
            {
                var target = await db.UserPoints
                    .FirstOrDefaultAsync(up => up.ServerId == user.Guild.Id && up.UserId == user.Id);
                if (target != null)
                    db.UserPoints.Remove(target);

                var targetAdmin = await db.Admins
                    .FirstOrDefaultAsync(a => a.ServerId == user.Guild.Id && a.UserId == user.Id);
                if (targetAdmin != null)
                    db.Admins.Remove(targetAdmin);

                await db.SaveChangesAsync();
            }
        }
    }
}
