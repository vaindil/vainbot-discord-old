using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VainBot
{
    [Group("slothies")]
    public class PointsModule : ModuleBase
    {
        readonly VbContext _context;

        public PointsModule(VbContext context)
        {
            _context = context;
        }

        [Command]
        public async Task Rankings()
        {
            var top = await _context.UserPoints
                .Where(up => up.ServerId == Context.Guild.Id && up.Points > 0)
                .OrderByDescending(up => up.Points)
                .Take(5)
                .ToListAsync();

            var bottom = await _context.UserPoints
                .Where(up => up.ServerId == Context.Guild.Id && up.Points < 0)
                .OrderBy(up => up.Points)
                .Take(5)
                .ToListAsync();

            var reply = "**__Top on the Server__**\n";

            foreach (var t in top)
            {
                var user = await Context.Guild.GetUserAsync(t.UserId);
                var rank = top.IndexOf(t) + 1;
                var result = await t.Points.GetCorrectPluralityAsync(_context);

                reply += rank + ". " + user.Username + ": " + result + "\n";
            }

            reply += "\n**__Bottom on the Server__**\n";

            foreach (var b in bottom)
            {
                var user = await Context.Guild.GetUserAsync(b.UserId);
                var rank = bottom.IndexOf(b) + 1;
                var result = await b.Points.GetCorrectPluralityAsync(_context);

                reply += rank + ". " + user.Username + ": " + result + "\n";
            }

            reply = reply.TrimEnd('\\', 'n');

            await ReplyAsync(reply);
        }
    }
}
