using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("rollmulti")]
    [Alias("multiroll")]
    [CrendorServerOnly]
    public class RollMultiModule : ModuleBase<VbCommandContext>
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
                var dL = d.ToLower();
                var deezNuts = dL.Split('d');

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

            if (reply.Length > 1950)
                reply = "You asked for so many dice that Discord had to step in and say no. Great job! 👍";

            await ReplyAsync(reply);
        }
    }
}
