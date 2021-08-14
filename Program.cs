using System;
using System.Collections.Generic;
using System.Linq;

namespace VehicleDamage
{
    static class Program
    {
        const int NumDieSides = 6;

        enum HitType
        {
            Tracks,
            Hull,
            Weapon1,
            Weapon2,
            Weapon3,
            InstantDeath,
        }

        enum VehicleColor
        {
            Green,
            Yellow,
            Red,
            Black,
        }

        static bool IsWeapon(this HitType hitType) => hitType >= HitType.Weapon1 && hitType <= HitType.Weapon3;
        static bool IsTread(this HitType hitType) => hitType == HitType.Tracks;

        static void Main(string[] args)
        {
            Console.WriteLine("begin");

            var greenHitTypes = new[] { HitType.Tracks, HitType.Tracks, HitType.Hull, HitType.Hull, HitType.Hull, HitType.InstantDeath, };
            var yellowHitTypes = new[] { HitType.Tracks, HitType.Tracks, HitType.Hull, HitType.Hull, HitType.Weapon1, HitType.InstantDeath, };
            var redHitTypes = new[] { HitType.Tracks, HitType.Hull, HitType.Hull, HitType.Weapon1, HitType.Weapon2, HitType.InstantDeath, };
            var blackHitTypes = new[] { HitType.Tracks, HitType.Hull, HitType.Hull, HitType.Weapon1, HitType.Weapon2, HitType.Weapon3, };

            var coloredHitTypes = new[]
            {
                new { Color = VehicleColor.Green, HitTypes = greenHitTypes },
                new { Color = VehicleColor.Yellow, HitTypes = yellowHitTypes },
                new { Color = VehicleColor.Red, HitTypes = redHitTypes },
                new { Color = VehicleColor.Black, HitTypes = blackHitTypes },
            };

            var colorsToOnlyPrintKillStats = new[] { VehicleColor.Green, VehicleColor.Yellow, };

            foreach(var coloredHits in coloredHitTypes)
            {
                foreach(var isPiercing in new[] {false, true})
                {
                    ProbabilitiesForHits(coloredHits.HitTypes, out var toKill, out var toNeutralize, out var toDeweaponize, isPiercing);
                    var labelPrefix = coloredHits.Color + " " + (isPiercing ? "piercing" : "nonpiercing") + " ";

                    DisplayExcel(labelPrefix + " kill", toKill);

                    if(!colorsToOnlyPrintKillStats.Contains(coloredHits.Color))
                    {
                        DisplayExcel(labelPrefix + " neutralize", toNeutralize);
                        DisplayExcel(labelPrefix + " deweaponize", toDeweaponize);
                    }
                }
            }

            Console.WriteLine("end");
        }

        static void DisplayProbabilities(string label, IList<double> probabilities)
        {
            Console.Write(label + ", sum:" + probabilities.Sum() + ",  ");

            for(int i = 0; i < probabilities.Count; i++)
            {
                if(i > 0 && probabilities[i] == 0)
                {
                    break;
                }

                //Console.Write((i + 1) + ": " + probabilities[i].ToString("N3"));
                Console.Write((i + 1) + ":" + probabilities[i]);

                if(i != probabilities.Count - 1)
                {
                    Console.Write(",  ");
                }
            }

            Console.WriteLine();
        }

        static void DisplayExcel(string label, IList<double> probabilities)
        {
            var sum = probabilities.Sum();
            Console.WriteLine(label + ';' + sum + ';' + string.Join(';', probabilities));
        }

        static void ProbabilitiesForHits(
            HitType[] hitTypes,
            out double[] toKill,
            out double[] toNeutralize,
            out double[] toDeweaponize,
            bool piercing = false)
        {
            var hitTypeSet = hitTypes.ToHashSet();
            var weaponSet = hitTypes.Where(hitType => hitType.IsWeapon()).ToHashSet();
            var maxHits = hitTypeSet.Where(hitType => hitType != HitType.InstantDeath).Count() + 1;
            var numPossibilities = Math.Pow(NumDieSides, maxHits);
            var rolledHitTypes = new HashSet<HitType>();

            toKill = new double[maxHits];
            toNeutralize = new double[maxHits];
            toDeweaponize = new double[maxHits];

            for(var possibilityIdx = 0; possibilityIdx < numPossibilities; possibilityIdx++)
            {
                var remainingPossibilityIdx = possibilityIdx;
                rolledHitTypes.Clear();
                var alreadyNeutralized = false;
                var alreadyDeweaponized = false;

                for(var dieIdx = 0; dieIdx < maxHits; dieIdx++)
                {
                    var dieRoll = remainingPossibilityIdx % NumDieSides + (piercing ? 1 : 0);
                    var rolledHitType = dieRoll == NumDieSides ? HitType.InstantDeath : hitTypes[dieRoll];

                    if(rolledHitType == HitType.InstantDeath
                        || rolledHitTypes.Contains(rolledHitType))
                    {
                        toKill[dieIdx]++;

                        if(!alreadyNeutralized)
                        {
                            toNeutralize[dieIdx]++;
                        }

                        if (!alreadyDeweaponized)
                        {
                            toDeweaponize[dieIdx]++;
                        }

                        break;
                    }

                    rolledHitTypes.Add(rolledHitType);
                    remainingPossibilityIdx /= NumDieSides;

                    var weaponsGone = weaponSet.IsSubsetOf(rolledHitTypes);

                    if(!alreadyNeutralized)
                    {
                        if(weaponsGone && rolledHitTypes.Contains(HitType.Tracks))
                        {
                            toNeutralize[dieIdx]++;
                            alreadyNeutralized = true;
                        }
                    }

                    if(!alreadyDeweaponized)
                    {
                        if(weaponsGone)
                        {
                            toDeweaponize[dieIdx]++;
                            alreadyDeweaponized = true;
                        }
                    }
                }
            }
        }
    }
}
