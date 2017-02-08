using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    [Group("slothfact")]
    [Alias("slothfacts")]
    public class SlothFactsModule : ModuleBase
    {
        readonly Random _rng;

        public SlothFactsModule(Random rng)
        {
            _rng = rng;
        }

        [Command]
        public async Task SlothFact()
        {
            var i = _rng.Next(0, SlothFacts.Count);
            await ReplyAsync(SlothFacts[i]);
        }

        static List<string> SlothFacts = new List<string>
        {
            "Sloths can sometimes maintain their grasp on limbs after death.",
            "Both two-toed and three-toed sloths grow to 1.5 to 2 feet long.",
            "The digestion process can take as long as a month to complete for an adult sloth!",
            "Sloths can rotate their heads around 270 degrees!",
            "Sloths have very slow metabolisms for creatures their size. This is why they can survive with leaves as their main source of food.",
            "Sloths are only about 25% muscle. They can't shiver if they get too cold!",
            "As much as 2/3 of a well-fed sloth's weight can be contained within its stomach chambers.",
            "Sloths mate and give birth while hanging in trees!",
            "Three-toed sloths have short stubby tails while two-toed sloths don't.",
            "To help conserve energy, a sloth's internal body temperature is only 30-34° C. It can drop even lower while they sleep.",
            "A sloth's laziness is actually a great method of survival. Its slow movement and camouflage helps it evade detection from natural predators.",
            "Sloths are sturdy! They are usually unharmed from falls.",
            "A sloth's maximum weight is about 40 pounds. Nearly two-thirds of this weight can be contained in the sloth's stomach compartments if it is well-fed.",
            "Three-toed sloths use their short tail to dig a hole and bury their poops! 💩",
            "The sloth can tolerate the largest change in body temperature of any mammal, from 74 to 92° F!",
            "Sloths only have one baby at a time.",
            "The outer hairs on a sloth actually grow in the opposite direction compared to other mammals.",
            "Sloths' internal organs are fixed to their ribcage. This prevents their lungs from being compressed while hanging upside down!",
            "There are two different families of sloths: _Megalonychidae_ (two-toed) and _Bradypodidae_ (three-toed).",
            "Sloths are the world's slowest-digesting mammal, only defecating once a week!",
            "Up until about 10,000 years ago, several species of ground sloths existed, such as _Megatherium_. This species grew to about the size of an elephant!",
            "Sloths are actually excellent swimmers! While in water they can slow their heart rate down to one-third its average pace. They can also move about three times faster in water with their version of the doggy paddle!",
            "The sloth is the world's slowest mammal!",
            "Sloths don't sweat and don't emit body odor. This helps avoid predation.",
            "Three-toed sloths have a maximum land speed of about 2 meters per minute!",
            "Sloths are excellent survivors. Of the five species of sloth, only one is currently endangered: the Maned Three-Toed Sloth.",
            "Healthy sloths generally live from 10 to 16 years in the wild. In captivity they can live to be over 30!",
            "A sloth can hold its breath for up to 40 minutes while in water!",
            "Despite the name, two-toed sloths actually have three toes. They only have two fingers though and they generally move a bit quicker than three-toed sloths.",
            "Sloths sometimes fatally mistake powerlines for trees. :crendorThump:230134127322595339",
            "Sloths tend to prefer the leaves of the Cecropia tree, sometimes known as pumpwoods."
        };
    }
}
