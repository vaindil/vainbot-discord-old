using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VainBot
{
    [Group("1slothies")]
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

        [Command]
        public async Task GetUser(IUser user)
        {
            decimal points;

            var userPoint = await _context.UserPoints
                .FirstOrDefaultAsync(up => up.ServerId == Context.Guild.Id && up.UserId == user.Id);

            if (userPoint == null)
                points = 0;
            else
                points = userPoint.Points;

            await ReplyAsync(user.Username + " has " + (await points.GetCorrectPluralityAsync(_context)) + ".");
        }

        // TODO check admin for overrides

        [Command]
        public async Task AddPoints(IUser user, decimal points)
        {
            var curUser = await _context.UserPoints
                .FirstOrDefaultAsync(up => up.ServerId == Context.Guild.Id && up.UserId == Context.User.Id);

            if (curUser == null || !curUser.Allow)
            {
                await ReplyAsync(">implying you can edit points");
                return;
            }

            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("You can't edit your own points, that would be cheating.");
                return;
            }

            if (user.Id == 132714099241910273 && Context.User.Id != 132714099241910273)
            {
                await ReplyAsync("Editing vaindil's points is not allowed. Teehee.");
                return;
            }

            var userPoint = await _context.UserPoints
                .FirstOrDefaultAsync(up => up.ServerId == Context.Guild.Id && up.UserId == user.Id);

            if (userPoint == null)
            {
                userPoint = new UserPoints
                {
                    ServerId = Context.Guild.Id,
                    UserId = user.Id,
                    Allow = false,
                    Points = 0
                };

                _context.UserPoints.Add(userPoint);
                await _context.SaveChangesAsync();
            }

            if (points > 99999 || points < -99999)
            {
                await ReplyAsync("Let's not go overboard. Not altered, currently " + userPoint.Points + ".");
                return;
            }

            userPoint.Points += Math.Round(points, 2);

            if (userPoint.Points > 10000000)
            {
                await ReplyAsync("That's a bit much. Untouched, reset to max of 10000000.");

                userPoint.Points = 10000000;
                await _context.SaveChangesAsync();
                return;
            }

            if (userPoint.Points < -10000000)
            {
                await ReplyAsync("That's a bit much. Untouched, reset to min of -10000000.");

                userPoint.Points = -10000000;
                await _context.SaveChangesAsync();
                return;
            }

            await _context.SaveChangesAsync();

            await ReplyAsync(user.Username + " now has " + (await userPoint.Points.GetCorrectPluralityAsync(_context)) + ".");
        }
    }
}
