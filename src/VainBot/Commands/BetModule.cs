using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VainBot.Commands
{
    [Group("bet")]
    public class BetModule : ModuleBase
    {
        readonly VbContext _context;
        readonly Random _rng;

        public BetModule(VbContext context, Random rng)
        {
            _context = context;
            _rng = rng;
        }

        [Command]
        public async Task Bet(decimal amount)
        {
            var betKey = await _context.KeyValues.GetValueAsync(DbKey.BettingAllowed);
            var allowed = bool.Parse(betKey);
            if (!allowed)
            {
                await ReplyAsync("Betting is currently disabled. Sorry!");
                return;
            }

            amount = Math.Round(amount, 2);

            if (amount <= 0)
            {
                await ReplyAsync("Hmm, vaindil totally didn't think of that one, you sure got 'em!");
                return;
            }

            if (amount > 10)
            {
                await ReplyAsync("You can't bet that much, you nerd.");
                return;
            }

            var user = await _context.UserPoints.FirstOrDefaultAsync(
                u => u.UserId == Context.Message.Author.Id && u.ServerId == Context.Guild.Id);

            if (user == null)
            {
                user = new UserPoints
                {
                    UserId = Context.Message.Author.Id,
                    ServerId = Context.Guild.Id,
                    Points = 0,
                    Allow = false
                };

                _context.UserPoints.Add(user);
                await _context.SaveChangesAsync();
            }

            if (user.Points < -20)
            {
                await ReplyAsync("You have too few slothies. Sorry! :HeyGuys:");
                return;
            }

            var baseMsg = $"{Context.Message.Author.Username}! " +
                $"You have {await user.Points.GetCorrectPluralityAsync(_context)} and you bet {amount}.";

            var msg = await ReplyAsync(baseMsg + " Rolling to determine your fate...");

            var success = _rng.Next(2) == 1;
            var trick = _rng.Next(4) == 3;

            if (Context.Message.Author.Id == 132714099241910273)
            {
                success = true;
                trick = false;
            }

            if (success)
                user.Points += amount;
            else
                user.Points -= amount;

            await _context.SaveChangesAsync();

            await Task.Delay(2500);

            if (trick)
            {
                if (success)
                    await msg.ModifyAsync(async m => m.Content = baseMsg +
                        $" You failed! Lost {await amount.GetCorrectPluralityAsync(_context)} and now have " +
                        $"{(user.Points - (2 * amount)).GetNumberString()}.");
                else
                    await msg.ModifyAsync(async m => m.Content = baseMsg +
                        $" You won! Gained {await amount.GetCorrectPluralityAsync(_context)} and now have " +
                        $"{(user.Points + (2 * amount)).GetNumberString()}.");

                baseMsg += " Just kidding!";

                await Task.Delay(4000);
            }

            if (success)
                await msg.ModifyAsync(async m => m.Content = baseMsg +
                    $" You won! Gained {await amount.GetCorrectPluralityAsync(_context)} " +
                    $"and now have {user.Points.GetNumberString()}.");
            else
                await msg.ModifyAsync(async m => m.Content = baseMsg +
                    $" You failed! Lost {await amount.GetCorrectPluralityAsync(_context)} " +
                    $"and now have {user.Points.GetNumberString()}.");

            if (trick)
                await msg.AddReactionAsync("LUL:232582021493424128");
        }

        [Command("toggle")]
        public async Task Toggle()
        {
            var isAdmin = await _context.Admins
                .AnyAsync(a => a.ServerId == Context.Guild.Id && a.UserId == Context.Message.Author.Id);
            if (!isAdmin)
            {
                await ReplyAsync("You can't do that, you nerd.");
                return;
            }

            var betKey = await _context.KeyValues.FirstAsync(kv => kv.Key == DbKey.BettingAllowed.ToString());
            var allowed = bool.Parse(betKey.Value);
            if (!allowed)
            {
                betKey.Value = true.ToString();
                await ReplyAsync("Betting is now enabled.");
            }
            else
            {
                betKey.Value = false.ToString();
                await ReplyAsync("Betting is now disabled.");
            }

            await _context.SaveChangesAsync();
        }
    }
}
