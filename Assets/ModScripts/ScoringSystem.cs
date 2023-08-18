using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using System.Text.RegularExpressions;

public static class ScoringSystem
{
    public static int[] baseScores(string serialNumber) => serialNumber.Select(x => ("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x)) % 6 + 1).ToArray();

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

    public static int[] modifyingScores(KMBombInfo bomb, List<KtaneModule> virtualBomb, List<KtaneModule> realBomb, int[] bases, int[] experts)
    {
        for (int i = 0; i < 6; i++)
        {
            int modifier = 0;
            switch (experts[i])
            {
                case 0: // 1254
                    var oneTwo = new[] { 1, 2, 5, 4 };
                    modifier = oneTwo.Contains(digitalRoot(bomb.GetSerialNumberNumbers().ToArray())) ? 3 : -1;
                    break;
                case 1: // AlexCorruptor
                    modifier = virtualBomb.All(mod => mod == null || mod.Name.ToLowerInvariant().Any("tepig".Contains)) ? 3 : -1;
                    break;
                case 2: // Axo
                    if (virtualBomb.Any(mod => mod?.Name == "XO") || realBomb.Any(mod => mod.Name == "XO"))
                        bases[i] = 0;
                    else
                        modifier = bomb.GetSerialNumberLetters().Any("AXO".Contains) ? 3 : -1;
                    break;
                case 3: // BigCrunch22
                    var crunch = new[] { bomb.GetBatteryHolderCount() == 2, bomb.GetIndicators().Count() == 2, bomb.GetPortPlateCount() == 2 };
                    for (int j = 0; j < crunch.Length; j++)
                        if (crunch[j])
                            modifier += 2;
                    if (bomb.GetSerialNumberNumbers().Any(num => num != 2))
                        modifier -= 2;
                    break;
                case 4: // Cinnabar
                    modifier = bomb.GetOnIndicators().Count() >= 3 ? -3 : 2 * bomb.GetOnIndicators().Count();
                    break;
                case 5: // CrazyCaleb
                    if (virtualBomb.Any(mod => mod?.Name == "Bartending") || realBomb.Any(mod => mod.Name == "Bartending"))
                        bases[i] = 0;
                    else
                        modifier = bomb.GetSerialNumberLetters().Any("AEIOU".Contains) ? 3 : -1;
                    break;
                case 6: // CyanixDash
                    if (bomb.IsIndicatorOn(Indicator.CLR) && bomb.GetBatteryCount() == 3)
                        bases[i] = 66669420; // Congration, you got the funny numbers! smhile
                    else
                    {
                        modifier = bomb.GetBatteryCount(Battery.AA);
                        modifier -= bomb.GetBatteryCount(Battery.D);
                    }
                    break;
                case 7: // Danielstigman
                    var dan = new[] { "VeryEasy", "Easy", "Medium", "Hard", "VeryHard" }
                        .Select(difficulty => virtualBomb.Count(mod => mod?.ExpertDifficulty == difficulty))
                        .ToArray();
                    int mostCommon = 0;
                    for (int j = 1; j < 5; j++)
                        if (dan[j] >= dan[mostCommon])
                            mostCommon = j;
                    mostCommon++;
                    if (bomb.GetBatteryCount() >= 5)
                        mostCommon *= -1;
                    modifier = mostCommon;
                    break;
                case 8: // dicey
                    modifier = Array.IndexOf(experts, experts[i]) == 5 ? -5 : Array.IndexOf(experts, experts[i]) + 1;
                    break;
                case 9: // Diffuse
                    modifier = virtualBomb.Any(mod => mod != null && (mod.Name == "Unfair's Cruel Revenge" || mod.Name.ContainsIgnoreCase("Cipher")))
                        || realBomb.Any(mod => mod.Name == "Unfair's Cruel Revenge" || mod.Name.ContainsIgnoreCase("Cipher")) ? 5 : -1;
                    break;
                case 10: // diskoQs
                    if (bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count())
                        modifier = 3;
                    else if (bomb.GetOffIndicators().Count() > bomb.GetOnIndicators().Count())
                        modifier = -2;
                    break;
                case 11: // Espik
                    if (virtualBomb.Any(mod => mod?.Name == "Forget Me Now") || realBomb.Any(mod => mod.Name == "Forget Me Now"))
                        bases[i] = 0;
                    else
                        modifier = !bomb.IsPortPresent(Port.Parallel) || !bomb.IsPortPresent(Port.Serial) ? 3 : -1;
                    break;
                case 12: // eXish
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                        bases[i] = 0;
                    else
                        modifier = bomb.GetSerialNumberNumbers().Last() % 2 != 0 ? 2 : 1;
                    break;
                case 13: // Floofy Floofles
                    if (bomb.GetIndicators().Count() == 1)
                        modifier = 3;
                    else if (bomb.GetIndicators().Count() == 0)
                        modifier = -2;
                    break;
                case 14: // GhostSalt
                    if (virtualBomb.Count(mod => mod?.Type == "Needy") + realBomb.Count(mod => mod.Type == "Needy") > 0)
                        modifier = 3;
                    modifier += bomb.GetOffIndicators().Count() - bomb.GetOnIndicators().Count();
                    break;
                case 15: // GoodHood
                    modifier = virtualBomb.Count(mod => mod?.ExpertDifficulty == "VeryEasy") - virtualBomb.Count(mod => mod?.ExpertDifficulty == "VeryHard") / 2;
                    break;
                case 16: // Gwen
                    modifier = bomb.GetSerialNumberNumbers().Count() % 2 != 0 ? 4 : -1;
                    break;
                case 17: // JyGein
                    modifier = (bomb.GetSerialNumberNumbers().Sum() / 5) - 2;
                    break;
                case 18: // Kilo
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                        bases[i] = 0;
                    else
                        modifier = virtualBomb.Any(mod => mod?.Name == "Unfair's Revenge") || realBomb.Any(mod => mod.Name == "Unfair's Revenge") ? 5 : 1;
                    break;
                case 19: // Konoko
                    modifier = virtualBomb.Any(mod => mod != null && Regex.IsMatch(mod.Name, "^[A-Za-z]{6}$")) ? 4 : -1;
                    break;
                case 20: // Kugel
                    modifier = bomb.GetBatteryCount() >= 6 ? -3 : bomb.GetBatteryCount();
                    break;
                case 21: // Kuro
                    if (virtualBomb.Any(mod => mod?.Name == "Procedural Maze") || realBomb.Any(x => x.Name == "Procedural Maze"))
                        bases[i] = 0;
                    else
                        modifier = bomb.GetPortCount(Port.Parallel) + bomb.GetPortCount(Port.Serial) - 1;
                    break;
                case 22: // Lexa
                    modifier = bomb.GetPortPlates().Any(x => x.Length == 0) ? 4 : -2;
                    break;
                case 23: // LilyFlair
                    modifier = virtualBomb.Any(mod => mod?.Type == "Needy") ? 4 : -2;
                    break;
                case 24: // Lulu
                    var lulu = new[] { virtualBomb.Count(mod => mod != null && (mod.Name.Contains("Simon") || mod.Name.Contains("Tasha"))), realBomb.Count(mod => mod.Name.Contains("Simon") || mod.Name.Contains("Tasha")) };
                    int[] lulu2 = new[] { virtualBomb.Count(mod => mod != null && mod.Quirks != null && mod.Quirks.Contains("TimeDependent")), realBomb.Count(mod => mod.Quirks != null && mod.Quirks.Contains("TimeDependent")) };
                    modifier = 2 * lulu.Sum() - 3 * lulu2.Sum();
                    break;
                case 25: // Mage
                    modifier = bomb.GetPortCount() >= 6 ? -2 : bomb.GetPortCount();
                    break;
                case 26: // Marksam
                    modifier = bomb.GetPortCount() - bomb.GetPortPlateCount();
                    break;
                case 27: // MasQuéÉlite
                    if (bomb.GetSerialNumberLetters().Any(x => "MQE".Contains(x)))
                        modifier = 4;
                    else if (bomb.GetBatteryCount() < 2)
                        modifier = 2;
                    break;
                case 28: // meh
                    var meh = bomb.GetSerialNumberNumbers().ToArray();
                    if (meh[1] % 2 == 0)
                        modifier = 1;
                    break;
                case 29: // NShep
                    modifier = virtualBomb.Count(mod => mod != null && "RPSJ".Contains(mod.Name[0])) - virtualBomb.Count(mod => mod != null && !"RPSJ".Contains(mod.Name[0])) / 2;
                    break;
                case 30: // Obvious
                    if (virtualBomb.Any(mod => mod?.Name == "Yoshi Egg") || realBomb.Any(x => x.Name == "Yoshi Egg"))
                        bases[i] = 0;
                    else
                        modifier = virtualBomb.Where(mod => mod?.Author == "Obvious").Count();
                    break;
                case 31: // Piissii
                    modifier = bomb.GetSerialNumber().Distinct().Count() != 6 ? 4 : -1;
                    break;
                case 32: // Quinn Wuest
                    modifier = bomb.GetIndicators().Count();
                    if (bomb.GetIndicators().Count() == 0)
                        modifier = -2;
                    break;
                case 33: // redpenguin
                    modifier = virtualBomb.All(mod => mod?.BossStatus != "FullBoss") ? 4 : -1;
                    break;
                case 34: // Rosenothorns03
                    modifier = virtualBomb.Any(mod => mod != null && mod.Name.StartsWith("The ")) ? 3 : -1;
                    break;
                case 35: // Scoping Landscape
                    if (virtualBomb.Any(mod => mod != null && (mod.Name.ContainsIgnoreCase("cipher") || mod.Name.ContainsIgnoreCase("cycle") || mod.Name.ContainsIgnoreCase("unfair")))
                            || realBomb.Any(mod => mod.Name.ContainsIgnoreCase("cipher") || mod.Name.ContainsIgnoreCase("cycle") || mod.Name.ContainsIgnoreCase("unfair")))
                        modifier = 5;
                    break;
                case 36: // Setra
                    modifier = realBomb.Count() >= 47 ? 4 : -1;
                    break;
                case 37: // Sierra
                    var sierraColors = new[] { "green", "blue", "brown", "purple", "orange", "black", "white" };
                    if (virtualBomb.Any(mod => mod != null && sierraColors.Any(color => mod.Name.ContainsIgnoreCase(color))) || realBomb.Any(x => sierraColors.Any(color => x.Name.ContainsIgnoreCase(color))))
                        modifier = 5;
                    else if (bomb.GetSerialNumberLetters().Any(x => x == 'S'))
                        modifier = -1;
                    break;
                case 38: // tandyCake
                    if (virtualBomb.Any(mod => mod?.Name == "The Pink Button") || realBomb.Any(mod => mod.Name == "The Pink Button"))
                        modifier = -bomb.GetBatteryCount(Battery.D);
                    else
                        modifier = bomb.GetSerialNumberNumbers().First() % 2 == 0 ? 3 : -1;
                    break;
                case 39: // TheFullestCircle
                    modifier = virtualBomb.Any(mod => mod?.Name == "Watch the Clock") || realBomb.Any(mod => mod.Name == "Watch the Clock")
                        ? 5 : bomb.GetBatteryCount() == 0 ? 4 : -1;
                    break;
                case 40: // Timwi
                    var nums = bomb.GetSerialNumberNumbers();
                    modifier = nums.Contains(4) && nums.Contains(7) ? 5 : nums.Any(num => num == 4 || num == 7) ? 3 : -1;
                    break;
                case 41: // Varunaxx
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        modifier = 4;
                    else
                        modifier = (int) DateTime.Now.DayOfWeek - 3;
                    break;
                case 42: // WitekWitek
                    modifier = bomb.GetSerialNumberLetters().Any(x => "WI".Contains(x)) || realBomb.Any(mod => mod.Name == "Simon Sends") ? 3 : -2;
                    break;
                case 43: // xorote
                    modifier = virtualBomb.Any(mod => mod?.Author == "Speakingevil") || realBomb.Any(mod => mod.Author == "Speakingevil") ? 4 : -1;
                    break;
                case 44: // Zaakeil
                    modifier = 5 - bomb.GetBatteryCount() - bomb.GetPortCount();
                    break;
                case 45: // Zaphod
                    if (virtualBomb.Any(mod => mod?.Name == "42") || realBomb.Any(mod => mod.Name == "42"))
                        bases[i] = 0;
                    else
                        modifier = bomb.IsPortPresent(Port.StereoRCA) ? 3 : -1;
                    break;
            }
            modifier = Mathf.Min(5, Mathf.Max(-5, modifier));
            bases[i] += modifier;
        }

        return bases;
    }
}
