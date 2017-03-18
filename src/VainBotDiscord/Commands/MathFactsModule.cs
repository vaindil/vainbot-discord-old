using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    [Group("mathfact")]
    [Alias("mathfacts")]
    [CrendorServerOnly]
    public class MathFactsModule : ModuleBase<VbCommandContext>
    {
        readonly Random _rng;

        public MathFactsModule(Random rng)
        {
            _rng = rng;
        }

        [Command]
        public async Task MathFact()
        {
            var i = _rng.Next(0, MathFacts.Count);
            await ReplyAsync(MathFacts[i]);
        }

        static List<string> MathFacts = new List<string>
        {
            "If you put 23 people in a room, there is a 50% chance that two will share the same birthday. Read more: <https://en.wikipedia.org/wiki/Birthday_problem>",
            "Given a solid ball, it is possible to deconstruct the ball into a finite number of pieces that can then be put back together a different way to make two identical copies of the original ball. Read more: <https://en.wikipedia.org/wiki/Banach%E2%80%93Tarski_paradox>",
            "It is impossible to comb all of the hairs on a tennis ball in the same direction without creating a cowlick. This is called the hairy ball theorem. Read more: <https://en.wikipedia.org/wiki/Hairy_ball_theorem>",
            "There are different types of infinity, and some are larger than others. Video explanation: <https://goo.gl/ccxPY3>",
            "The writers of Futurama (including a Ph.D. mathematician) created and proved a mathematical theorem to solve a problem in the episode 'The Prisoner of Benda'. Read more: <https://en.wikipedia.org/wiki/The_Prisoner_of_Benda#The_theorem>",
            "1^2 = 1 | 11^2 = 121 | 111^2 = 12321 | 1111^2 = 1234321 | 11111^2 = 123454321 | ...",
            "1 million seconds is approximately 11.5 days. 1 billion seconds is just under 32 years.",
            "If you glue two Möbius strips together, topologically you'll create a Klein bottle. Read more: <https://en.wikipedia.org/wiki/Klein_bottle>",
            "0.9999... repeating is exactly equal to 1. Read more: <https://en.wikipedia.org/wiki/0.999...>",
            "You're presented with three doors. Behind one is a fantastic prize, but behind the other two is nothing. You choose your door, and you are shown that a different door had nothing behind it. You are allowed to change your door to the other of the two remaining doors. Statistically, you are more likely to win the prize if you switch to the other door. Read more: <https://en.wikipedia.org/wiki/Monty_Hall_problem>",
            "A shape exists that has infinite surface area but a finite volume. In other words, it can be filled with paint but you cannot paint it. Read more: <https://en.wikipedia.org/wiki/Gabriel%27s_Horn>",
            "A standard deck of 52 playing cards can be stacked in this many ways. Statistically, each time you shuffle a deck of cards, it's likely that the deck has never before been placed in that order. 80658175170943878571660636856403766975289505440883277824000000000000",
            "73 is the 21st prime number. 21 is the product of 7 and 3. The reverse of 73, 37, is the 12th prime number--12 is the reverse of 21. In binary, 73 is a palindrome: 1001001.",
            "Graham's number is a number so large that the universe does not contain enough space to represent it. If you were to attempt to visualize the entire number, your brain would literally collapse into a black hole. Read more: <https://en.wikipedia.org/wiki/Graham%27s_number>",
            "If you spell out each number in English (one, two, three, etc.), the first time you use the letter 'a' is at 'one thousand'.",
            "Pick any 3-digit number. Multiply it by 7, 13, and 11. The answer every time will be 597597.",
            "Given any party of 6 people, it must be true that either 3 of them all know each other or 3 of them have never met one another."
        };
    }
}
