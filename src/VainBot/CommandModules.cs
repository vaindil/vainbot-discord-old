using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VainBot
{
    [Group("roll")]
    public class RollModule : ModuleBase
    {
        readonly Random _rng;
        static Regex validDie = new Regex(@"^-?\d*d-?\d+$", RegexOptions.IgnoreCase);

        public RollModule(Random rng)
        {
            _rng = rng;
        }

        [Command]
        public async Task Rolld20()
        {
            var num = _rng.Next(1, 21);
            await ReplyAsync(Context.User.Username + " rolled 1d20 and got **" + num + "**.");
        }

        [Command]
        public async Task RollSingle([Remainder]string msg)
        {
            var customDice = false;
            var diceOnly = false;
            var msgOnly = false;
            var total = 0;

            var dice = msg.Split(new char[] { ' ' }, 2);

            string validString;

            if (validDie.IsMatch(dice[0]))
            {
                validString = Extensions.ValidateDiceRoll(dice[0], false);
                customDice = true;

                if (dice.Length == 1)
                    diceOnly = true;
            }
            else
            {
                validString = string.Empty;
                msgOnly = true;
            }

            if (validString != string.Empty)
            {
                await ReplyAsync(validString);
                return;
            }

            var numDice = 1;
            var numSides = 20;

            if (customDice)
            {
                var deezNuts = dice[0].Split('d');

                if (string.IsNullOrEmpty(deezNuts[0]))
                    numDice = 1;
                else
                    numDice = int.Parse(deezNuts[0]);

                numSides = int.Parse(deezNuts[1]);
            }
            
            for (var i = 0; i < numDice; i++)
            {
                var datBoi = _rng.Next(1, numSides + 1);
                total += datBoi;
            }

            if (msgOnly)
            {
                await ReplyAsync(Context.User.Username + " rolled 1d20 **" + msg + "** and got **" + total + "**.");
                return;
            }
            else if (diceOnly)
            {
                await ReplyAsync(Context.User.Username + " rolled " + dice[0] + " and got **" + total + "**.");
                return;
            }

            await ReplyAsync(Context.User.Username + " rolled " + dice[0] + " **" + dice[1] + "** and got **" + total + "**.");
        }
    }

    [Group("rollmulti")]
    public class RollMultiModule : ModuleBase
    {
        readonly Random _rng;

        public RollMultiModule(Random rng)
        {
            _rng = rng;
        }

        [Command]
        public async Task Rolld20()
        {
            var num = _rng.Next(1, 21);
            await ReplyAsync(Context.User.Username + " rolled 1d20 and got **" + num + "**.");
        }

        [Command]
        public async Task Roll([Remainder]string inDice)
        {
            inDice = inDice.Trim();

            var dice = inDice.Split(' ');
            if (dice.Length == 0)
            {
                await ReplyAsync("You didn't provide any dice, you nerd.");
                return;
            }

            var total = 0;
            var reply = "__Results__\n";

            foreach (var d in dice)
            {
                var validString = Extensions.ValidateDiceRoll(d, true);

                if (validString != string.Empty)
                {
                    await ReplyAsync(validString);
                    return;
                }

                int numDice;
                var deezNuts = d.Split('d');

                if (string.IsNullOrEmpty(deezNuts[0]))
                    numDice = 1;
                else
                    numDice = int.Parse(deezNuts[0]);
                
                var numSides = int.Parse(deezNuts[1]);

                reply += d + ": ";

                for (var i = 0; i < numDice; i++)
                {
                    var datBoi = _rng.Next(1, numSides + 1);
                    reply += datBoi + ", ";
                    total += datBoi;
                }

                reply = reply.Substring(0, reply.Length - 2);
                reply += "\n";
            }

            reply += "\n**Total**: " + total;
            await ReplyAsync(reply);
        }
    }

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

        [Command]
        public async Task AddPoints(IUser user, decimal points)
        {
            var curIsAdmin = await _context.Admins
                .AnyAsync(a => a.ServerId == Context.Guild.Id && a.UserId == Context.User.Id);

            var curUser = await _context.UserPoints
                .FirstOrDefaultAsync(up => up.ServerId == Context.Guild.Id && up.UserId == Context.User.Id);

            if (curUser == null || !curUser.Allow)
            {
                await ReplyAsync(">implying you can edit points");
                return;
            }

            if (user.Id == Context.User.Id && !curIsAdmin)
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
                await ReplyAsync("Let's not go overboard. Not altered, currently " + userPoint.Points.GetNumberString() + ".");
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

        [Command("allow")]
        public async Task ToggleAllow(IUser user)
        {
            var curIsAdmin = await _context.Admins
                .AnyAsync(a => a.ServerId == Context.Guild.Id && a.UserId == Context.User.Id);
            var targetIsAdmin = await _context.Admins
                .AnyAsync(a => a.ServerId == Context.Guild.Id && a.UserId == user.Id);

            if (!curIsAdmin)
            {
                await ReplyAsync("You can't do that, silly.");
                return;
            }

            if (targetIsAdmin)
            {
                await ReplyAsync(user.Username + " is an admin, you nerd.");
                return;
            }

            var target = await _context.UserPoints
                .FirstOrDefaultAsync(up => up.ServerId == Context.Guild.Id && up.UserId == user.Id);

            if (target == null || !target.Allow)
            {
                if (target == null)
                {
                    target = new UserPoints
                    {
                        ServerId = Context.Guild.Id,
                        UserId = user.Id,
                        Points = 0,
                        Allow = true
                    };

                    _context.UserPoints.Add(target);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    target.Allow = true;
                }

                await ReplyAsync(user.Username + " is now allowed to edit slothies.");
                return;
            }
            
            target.Allow = false;
            await _context.SaveChangesAsync();

            await ReplyAsync(user.Username + " is now disallowed from editing slothies.");
        }
    }
}
