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
            amount = Math.Round(amount, 2);

            if (amount < 0)
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
                await ReplyAsync("You have too few slothies. Sorry! :HeyGuys:231512066937192448");
                return;
            }

            var baseMsg = $"{Context.Message.Author.Username}! " +
                $"You have {await user.Points.GetCorrectPluralityAsync(_context)} and you bet {amount}.";

            var msg = await ReplyAsync(baseMsg + " Rolling to determine your fate...");

            var success = _rng.Next(2) == 1;
            var trick = _rng.Next(4) == 3;

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
    }
}
