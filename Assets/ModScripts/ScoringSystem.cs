using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;

public static class ScoringSystem
{
    public static int[] baseScores(string serialNumber) => serialNumber.Select(x => ("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x)) % 6 + 1).ToArray();

    public static int[] finalScores(int[] baseScore, string[] modules, string[] experts)
    {
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (experts[j].Equals(modules[i]))
                {
                    switch (modules[i])
                    {
                        case "Easy":
                            baseScore[j] += 1;
                            break;
                        case "Medium":
                            baseScore[j] += 2;
                            break;
                        case "Hard":
                            baseScore[j] += 3;
                            break;
                        case "VeryHard":
                            baseScore[j] += 4;
                            break;
                    }
                }
            }
        }

        return baseScore;
    }

    private static int digitalRoot(int[] digits)
    {
        switch (digits.Length)
        {
            case 2:
                return (((digits[0] * 10 + digits[1]) - 1) % 9) + 1;
            case 3:
                return (((digits[0] * 100 + digits[1] * 10 + digits[1]) - 1) % 9) + 1;
            case 4:
                return (((digits[0] * 1000 + digits[1] * 100 + digits[2] * 10 + digits[3]) - 1) % 9) + 1;
        }
        return -1;
    }

    public static int[] modifyingScores(KMBombInfo bomb, List<KtaneModule> rawInfo, int[] bases, int[] experts, int[] modIx)
    {
        var virtualBomb = new KtaneModule[11];
        var realBomb = bomb.GetModuleNames();

        for (int i = 0; i < 12; i++)
        {
            if (modIx[i] != -1)
            {
                virtualBomb[i] = rawInfo[modIx[i]];
            }
        }

        for (int i = 0; i < 6; i++)
        {
            switch (experts[i])
            {
                case 0: // 1254
                    var oneTwo = new[] { 1, 2, 5, 4 };
                    bases[i] += oneTwo.Contains(digitalRoot(bomb.GetSerialNumberNumbers().ToArray())) ? 3 : -1;
                    break;
                case 1: // AlexCorruptor
                    bases[i] += virtualBomb.Any(x => "TEPIG".ContainsIgnoreCase(x.Name)) ? 3 : -1;
                    break;
                case 2: // Axo
                    if (virtualBomb.Any(x => "XO".Equals(x.Name)) || realBomb.Any(x => "XO".Equals(x)))
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += bomb.GetSerialNumberLetters().Any(x => "AXO".Contains(x)) ? 3 : -1;
                    }
                    break;
                case 3: // BigCrunch22
                    var crunch = new[] { bomb.GetBatteryHolderCount() == 2, bomb.GetIndicators().Count() == 2, bomb.GetPortPlateCount() == 2 };
                    for (int j = 0; j < crunch.Length; j++)
                    {
                        if (crunch[j])
                        {
                            bases[i] += 2;
                        }
                    }
                    if (bomb.GetSerialNumberNumbers().Any(x => x != 2))
                    {
                        bases[i] -= 2;
                    }
                    break;
                case 4: // Cinnabar
                    bases[i] += bomb.GetOnIndicators().Count() >= 3 ? -3 : 2 * bomb.GetOnIndicators().Count();
                    break;
                case 5: // CrazyCaleb
                    if (virtualBomb.Any(x => "Bartending".Equals(x.Name)) || realBomb.Any(x => "Bartending".Equals(x)))
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += bomb.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x)) ? 3 : -1;
                    }
                    break;
                case 6: // CyanixDash
                    if (bomb.IsIndicatorOn(Indicator.CLR) && bomb.GetBatteryCount() == 3)
                    {
                        bases[i] = 69; // Congration, you got the funny number! smhile
                    }
                    else
                    {
                        bases[i] += bomb.GetBatteryCount(Battery.AA);
                        bases[i] -= bomb.GetBatteryCount(Battery.D);
                    }
                    break;
                case 7: // Danielstigman
                    var dan = new[] { virtualBomb.Where(x => "VeryEasy".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "Easy".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "Medium".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "Hard".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "VeryHard".Equals(x.ExpertDifficulty)).Count() };
                    // Not sure what to do with this, so I'll leave that to you.
                    break;
                case 8: // dicey
                    bases[i] += Array.IndexOf(experts, experts[i]) == 5 ? -5 : Array.IndexOf(experts, experts[i]) + 1;
                    break;
                case 9: // Diffuse
                    bases[i] += virtualBomb.Any(x => "Unfair's Cruel Revenge".Equals(x.Name) || "Cipher".ContainsIgnoreCase(x.Name)) || realBomb.Any(x => "Unfair's Cruel Revenge".Equals(x) || "Cipher".ContainsIgnoreCase(x)) ? 5 : -1;
                    break;
                case 10: // diskoQs
                    if (bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count())
                    {
                        bases[i] += 3;
                    }
                    else if (bomb.GetOffIndicators().Count() > bomb.GetOnIndicators().Count())
                    {
                        bases[i] -= 2;
                    }
                    break;
                case 11: // Espik
                    if (virtualBomb.Any(x => "Forget Me Now".Equals(x.Name)) || realBomb.Any(x => "Forget Me Now".Equals(x)))
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += !bomb.IsPortPresent(Port.Parallel) || !bomb.IsPortPresent(Port.Serial) ? 3 : -1;
                    }
                    break;
                case 12: // eXish
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] = bomb.GetSerialNumberNumbers().Last() % 2 != 0 ? 2 : 1;
                    }
                    break;
                case 13: // Floofy Floofles
                    if (bomb.GetIndicators().Count() == 1)
                    {
                        bases[i] += 3;
                    }
                    else if (bomb.GetIndicators().Count() == 0)
                    {
                        bases[i] -= 2;
                    }
                    break;
                case 14: // GhostSalt
                    var salt = rawInfo.Where(x => "Needy".Equals(x.Type)).Select(x => x.Name).ToArray();
                    var salt2 = realBomb.Where(x => salt.Equals(x)).Count();
                    if (salt2 >= 1)
                    {
                        bases[i] += 3;
                    }
                    bases[i] += bomb.GetOffIndicators().Count();
                    bases[i] -= bomb.GetOnIndicators().Count();
                    break;
                case 15: // GoodHood
                    // I'll let you handle this one.
                    break;
                case 16: // Gwen
                    bases[i] += bomb.GetSerialNumberNumbers().Count() % 2 != 0 ? 4 : -1;
                    break;
                case 17: // JyGein
                    bases[i] += bomb.GetSerialNumberNumbers().Sum() / 5 - 2;
                    break;
                case 18: // Kilo
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += virtualBomb.Any(x => "Unfair's Revenge".Equals(x.Name)) || realBomb.Any(x => "Unfair's Revenge".Equals(x)) ? 5 : 1;
                    }
                    break;
                case 19: // Konoko
                    bases[i] += virtualBomb.Any(x => "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(x.Name) && x.Name.Length == 6) ? 4 : -1;
                    break;
                case 20: // Kugel
                    bases[i] += bomb.GetBatteryCount() >= 6 ? -3 : bomb.GetBatteryCount();
                    break;
                case 21: // Kuro
                    if (virtualBomb.Any(x => "Procedural Maze".Equals(x.Name)) || realBomb.Any(x => "Procedural Maze".Equals(x)))
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += bomb.GetPortCount(Port.Parallel);
                        bases[i] += bomb.GetPortCount(Port.Serial);
                    }
                    break;
                case 22: // Lexa
                    bases[i] += bomb.GetPortPlates().Any(x => x.Length == 0) ? 4 : -2;
                    break;
                case 23: // LilyFlair
                    bases[i] += virtualBomb.Any(x => "Needy".Equals(x.Type)) ? 4 : -2;
                    break;
                case 24: // Lulu
                    var lulu = new[] { virtualBomb.Where(x => "Simon".Contains(x.Name) && "Tasha".Contains(x.Name)).Count(), realBomb.Where(x => "Simon".Contains(x) && "Tasha".Contains(x)).Count() };
                    var timeDependent = rawInfo.Where(x => "TimeDependent".Equals(x.Quirks)).Select(x => x.Name).ToArray();
                    var lulu2 = new[] { virtualBomb.Where(x => timeDependent.Equals(x)).Count(), realBomb.Where(x => timeDependent.Equals(x)).Count() };
                    bases[i] += 2 * lulu.Sum();
                    bases[i] -= 3 * lulu2.Sum();
                    break;
                case 25: // Mage
                    bases[i] += bomb.GetPortCount() >= 6 ? -2 : bomb.GetPortCount();
                    break;
                case 26: // Marksam
                    bases[i] += bomb.GetPortCount();
                    bases[i] -= bomb.GetPortPlateCount();
                    break;
                case 27: // MasQuéÉlite
                    if (bomb.GetSerialNumberLetters().Any(x => "MQE".Contains(x)))
                    {
                        bases[i] += 4;
                    }
                    else if (bomb.GetBatteryCount() < 2)
                    {
                        bases[i] -= 2;
                    }
                    break;
                case 28: // meh
                    var meh = bomb.GetSerialNumberNumbers().ToArray();
                    if (meh[1] % 2 == 0)
                    {
                        bases[i]++;
                    }
                    break;
                case 29: // NShep
                    // Not sure how I would do that.
                    break;
                case 30: // Obvious
                    if (virtualBomb.Any(x => "Yoshi Egg".Equals(x.Name)) || realBomb.Any(x => "Yoshi Egg".Equals(x)))
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += virtualBomb.Where(x => "Obvious".Equals(x.Author)).Count();
                    }
                    break;
                case 31: // Piissii
                    bases[i] += bomb.GetSerialNumber().Distinct().Count() != 6 ? 4 : -1;
                    break;
                case 32: // Quinn Wuest
                    bases[i] += bomb.GetIndicators().Count();
                    if (bomb.GetIndicators().Count() == 0)
                    {
                        bases[i] -= 2;
                    }
                    break;
                case 33: // redpenguin
                    bases[i] += virtualBomb.Any(x => !"FullBoss".Equals(x.BossStatus)) ? 4 : -1;
                    break;
                case 34: // Rosenothorns03
                    bases[i] += virtualBomb.Any(x => "The".StartsWith(x.Name)) ? 3 : -1;
                    break;
                case 35: // Scoping Landscape
                    if (virtualBomb.Any(x => "Cipher".Contains(x.Name) || "Cycle".Contains(x.Name) || "Unfair".Contains(x.Name)) || realBomb.Any(x => "Cipher".Contains(x) || "Cycle".Contains(x) || "Unfair".Contains(x)))
                    {
                        bases[i] += 5;
                    }
                    break;
                case 36: // Setra
                    bases[i] += realBomb.Count() >= 47 ? 4 : -1;
                    break;
                case 37: // Sierra
                    var sierraColors = new[] { "Green", "Blue", "Brown", "Purple", "Orange", "Black", "White" };
                    if (virtualBomb.Any(x => sierraColors.Contains(x.Name)) || realBomb.Any(x => sierraColors.Contains(x)))
                    {
                        bases[i] += 5;
                    }
                    else if (bomb.GetSerialNumberLetters().Any(x => "S".Contains(x)))
                    {
                        bases[i]--;
                    }
                    break;
                case 38: // tandyCake
                    if (virtualBomb.Any(x => "The Pink Button".Equals(x.Name)) || realBomb.Any(x => "The Pink Button".Equals(x)))
                    {
                        bases[i] -= bomb.GetBatteryCount(Battery.D);
                    }
                    else
                    {
                        bases[i] += bomb.GetSerialNumberNumbers().First() % 2 == 0 ? 3 : -1;
                    }
                    break;
                case 39: // TheFullestCircle
                    bases[i] += virtualBomb.Any(x => "Watch the Clock".Equals(x.Name)) || realBomb.Any(x => "Watch the Clock".Equals(x)) ? 5 :
                        bomb.GetBatteryCount() == 0 ? 4 : -1;
                    break;
                case 40: // Timwi
                    // Unsure of where to go with this :skull:
                    var timwiSer = UnityEngine.Object.FindObjectOfType<UltimateTeamService>();

                    if (timwiSer == null)
                    {
                        throw new Exception("Cannot find service!");
                    }
                    if (timwiSer.connectedJson && timwiSer.connectedSprite)
                    {
                        bases[i] += virtualBomb.Where(x => "Supported".Equals(x.Souvenir.Status)).Count();
                    }
                    break;
                case 41: // Varunaxx
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        bases[i] += 4;
                    }
                    else
                    {
                        bases[i] += (int) DateTime.Now.DayOfWeek - 3;
                    }
                    break;
                case 42: // WitekWitek
                    bases[i] += bomb.GetSerialNumberLetters().Any(x => "WI".Contains(x)) || realBomb.Any(x => "Simon Sends".Equals(x)) ? 3 : -2;
                    break;
                case 43: // xorote
                    var speakingevilFunny = rawInfo.Where(x => "Speakingevil".Equals(x.Author)).Select(x => x.Name).ToArray();
                    bases[i] += virtualBomb.Any(x => speakingevilFunny.Equals(x.Name)) || realBomb.Any(x => speakingevilFunny.Equals(x)) ? 4 : -1;
                    break;
                case 44: // Zaakeil
                    bases[i] += 5 - (bomb.GetBatteryCount() + bomb.GetPortCount());
                    break;
                case 45: // Zaphod
                    if (virtualBomb.Any(x => "42".Equals(x.Name)) || realBomb.Any(x => "42".Equals(x)))
                    {
                        bases[i] = 0;
                    }
                    else
                    {
                        bases[i] += bomb.IsPortPresent(Port.StereoRCA) ? 3 : -1;
                    }
                    break;
            }
        }

        return bases;
    }
}
