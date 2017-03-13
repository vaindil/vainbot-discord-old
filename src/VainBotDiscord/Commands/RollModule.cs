using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("roll")]
    [CrendorServerOnly]
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
            await ReplyAsync(Context.User.Username + " rolled 1d20 and got " + num + ".");
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
                validString = Extensions.ValidateDiceRoll(dice[0], false, 1000);
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

            if ((msgOnly && msg.Length > 100) || (!msgOnly && !diceOnly && dice[1].Length > 100))
            {
                await ReplyAsync("If you're depending on randomness to help you with that much, then you need way " +
                    "more help than I can give you.");
                return;
            }

            var numDice = 1;
            var numSides = 20;

            if (customDice)
            {
                var dL = dice[0].ToLower();
                var deezNuts = dL.Split('d');

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
                await ReplyAsync(Context.User.Username + " rolled 1d20 " + msg + " and got " + total + ".");
                return;
            }
            else if (diceOnly)
            {
                await ReplyAsync(Context.User.Username + " rolled " + dice[0] + " and got " + total + ".");
                return;
            }

            await ReplyAsync(Context.User.Username + " rolled " + dice[0] + " " + dice[1] + " and got " + total + ".");
        }
    }
}
