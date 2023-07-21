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

    public static int[] modifyingScores(KMBombInfo bomb, List<KtaneModule> virtualBomb, List<KtaneModule> realBomb, int[] bases, int[] experts, int modId, string[] expertPrefs)
    {
        for (int i = 0; i < 6; i++)
        {
            var modifier = 0;
            var contained = new[] { "Easy", "Medium", "Hard", "VeryHard" };
            var difficulties = virtualBomb.Select(x => x.ExpertDifficulty).ToArray();

            switch (expertPrefs[i])
            {
                case "Easy":
                    modifier = difficulties.Count(x => "Easy".Equals(x));
                    break;
                case "Medium":
                    modifier = 2 * difficulties.Count(x => "Medium".Equals(x));
                    break;
                case "Hard":
                    modifier = 3 * difficulties.Count(x => "Hard".Equals(x));
                    break;
                case "VeryHard":
                    modifier = 4 * difficulties.Count(x => "VeryHard".Equals(x));
                    break;
            }

            bases[i] += modifier;
        }

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
                    modifier = virtualBomb.All(x => x.Name.ToLower().Any(y => "tepig".Contains(y))) ? 3 : -1;
                    break;
                case 2: // Axo
                    if (virtualBomb.Any(x => "XO".Equals(x.Name)) || realBomb.Any(x => "XO".Equals(x.Name)))
                        bases[i] = 0;
                    else
                        modifier = bomb.GetSerialNumberLetters().Any(x => "AXO".Contains(x)) ? 3 : -1;
                    break;
                case 3: // BigCrunch22
                    var crunch = new[] { bomb.GetBatteryHolderCount() == 2, bomb.GetIndicators().Count() == 2, bomb.GetPortPlateCount() == 2 };
                    for (int j = 0; j < crunch.Length; j++)
                        if (crunch[j])
                            modifier += 2;
                    if (bomb.GetSerialNumberNumbers().Any(x => x != 2))
                        modifier -= 2;
                    break;
                case 4: // Cinnabar
                    modifier = bomb.GetOnIndicators().Count() >= 3 ? -3 : 2 * bomb.GetOnIndicators().Count();
                    break;
                case 5: // CrazyCaleb
                    if (virtualBomb.Any(x => "Bartending".Equals(x.Name)) || realBomb.Any(x => "Bartending".Equals(x.Name)))
                        bases[i] = 0;
                    else
                        modifier = bomb.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x)) ? 3 : -1;
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
                    var dan = new[] { virtualBomb.Where(x => "VeryEasy".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "Easy".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "Medium".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "Hard".Equals(x.ExpertDifficulty)).Count(), virtualBomb.Where(x => "VeryHard".Equals(x.ExpertDifficulty)).Count() };
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
                    modifier = virtualBomb.Any(x => "Unfair's Cruel Revenge".Equals(x.Name) || x.Name.ToLower().Contains("Cipher")) || realBomb.Any(x => "Unfair's Cruel Revenge".Equals(x.Name) || x.Name.ToLower().Contains("Cipher")) ? 5 : -1;
                    break;
                case 10: // diskoQs
                    if (bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count())
                        modifier = 3;
                    else if (bomb.GetOffIndicators().Count() > bomb.GetOnIndicators().Count())
                        modifier = -2;
                    break;
                case 11: // Espik
                    if (virtualBomb.Any(x => "Forget Me Now".Equals(x.Name)) || realBomb.Any(x => "Forget Me Now".Equals(x.Name)))
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
                    if(virtualBomb.Where(x => "Needy".Equals(x.Type)).Select(x => x.Name).Count() + realBomb.Where(x => "Needy".Equals(x.Type)).Select(x => x.Name).Count() > 0)
                        modifier = 3;
                    modifier += bomb.GetOffIndicators().Count() - bomb.GetOnIndicators().Count();
                    break;
                case 15: // GoodHood
                    modifier = virtualBomb.Where(x => "VeryEasy".Equals(x.ExpertDifficulty)).Count() - virtualBomb.Where(x => "VeryHard".Equals(x.ExpertDifficulty)).Count() / 2;
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
                        modifier = virtualBomb.Any(x => "Unfair's Revenge".Equals(x.Name)) || realBomb.Any(x => "Unfair's Revenge".Equals(x.Name)) ? 5 : 1;
                    break;
                case 19: // Konoko
                    modifier = virtualBomb.Where(x => Regex.IsMatch(x.Name, "^[A-Za-z]{6}")).Count() > 0 ? 4 : -1;
                    break;
                case 20: // Kugel
                    modifier = bomb.GetBatteryCount() >= 6 ? -3 : bomb.GetBatteryCount();
                    break;
                case 21: // Kuro
                    if (virtualBomb.Any(x => "Procedural Maze".Equals(x.Name)) || realBomb.Any(x => "Procedural Maze".Equals(x.Name)))
                        bases[i] = 0;
                    else
                        modifier = bomb.GetPortCount(Port.Parallel) + bomb.GetPortCount(Port.Serial) - 1;
                    break;
                case 22: // Lexa
                    modifier = bomb.GetPortPlates().Any(x => x.Length == 0) ? 4 : -2;
                    break;
                case 23: // LilyFlair
                    modifier = virtualBomb.Any(x => "Needy".Equals(x.Type)) ? 4 : -2;
                    break;
                case 24: // Lulu
                    var lulu = new[] { virtualBomb.Where(x => x.Name.Contains("Simon") && x.Name.Contains("Tasha")).Count(), realBomb.Where(x => x.Name.Contains("Simon") && x.Name.Contains("Tasha")).Count() };
                    int[] lulu2 = new[] { virtualBomb.Count(x => x.Quirks != null && x.Quirks.Contains("TimeDependent")), realBomb.Count(x => x.Quirks != null && x.Quirks.Contains("TimeDependent")) };
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
                    modifier = virtualBomb.Where(x => x.Name.StartsWith("The")).Count() - virtualBomb.Where(x => !x.Name.StartsWith("The")).Count() / 2;
                    break;
                case 30: // Obvious
                    if (virtualBomb.Any(x => "Yoshi Egg".Equals(x.Name)) || realBomb.Any(x => "Yoshi Egg".Equals(x.Name.Contains("Simon"))))
                        bases[i] = 0;
                    else
                        modifier = virtualBomb.Where(x => "Obvious".Equals(x.Author)).Count();
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
                    modifier = virtualBomb.All(x => !"FullBoss".Equals(x.BossStatus)) ? 4 : -1;
                    break;
                case 34: // Rosenothorns03
                    modifier = virtualBomb.Any(x => x.Name.StartsWith("The")) ? 3 : -1;
                    break;
                case 35: // Scoping Landscape
                    if (virtualBomb.Any(x => x.Name.ToLower().Contains("cipher") || x.Name.ToLower().Contains("cycle") || x.Name.ToLower().Contains("unfair")) || realBomb.Any(x => x.Name.ToLower().Contains("cipher") || x.Name.ToLower().Contains("cycle") || x.Name.ToLower().Contains("unfair")))
                        modifier = 5;
                    break;
                case 36: // Setra
                    modifier = realBomb.Count() >= 47 ? 4 : -1;
                    break;
                case 37: // Sierra
                    var sierraColors = new[] { "Green", "Blue", "Brown", "Purple", "Orange", "Black", "White" };
                    if (virtualBomb.Any(x => sierraColors.Where(y => x.Name.ToLower().Contains(y)).Count() > 0) || realBomb.Any(x => sierraColors.Where(y => x.Name.ToLower().Contains(y)).Count() > 0))
                        modifier = 5;
                    else if (bomb.GetSerialNumberLetters().Any(x => x == 'S'))
                        modifier = -1;
                    break;
                case 38: // tandyCake
                    if (virtualBomb.Any(x => "The Pink Button".Equals(x.Name)) || realBomb.Any(x => "The Pink Button".Equals(x.Name)))
                        modifier = -bomb.GetBatteryCount(Battery.D);
                    else
                        modifier = bomb.GetSerialNumberNumbers().First() % 2 == 0 ? 3 : -1;
                    break;
                case 39: // TheFullestCircle
                    modifier = virtualBomb.Any(x => "Watch the Clock".Equals(x.Name)) || realBomb.Any(x => "Watch the Clock".Equals(x.Name)) ? 5 :
                        bomb.GetBatteryCount() == 0 ? 4 : -1;
                    break;
                case 40: // Timwi
                    var timwiSer = UnityEngine.Object.FindObjectOfType<UltimateTeamService>();

                    if (timwiSer == null)
                        Debug.Log($"[Ultimate Team #{modId}] Cannot find Souvenir supported modules, so not applying Timwi's rule.");
                    else if (timwiSer.connectedJson && timwiSer.connectedSprite)
                    {
                        modifier = virtualBomb.Count(x => x.Souvenir != null && "Supported".Equals(x.Souvenir.Status));
                        modifier -= virtualBomb.Count(x => x.Souvenir != null && !"Supported".Equals(x.Souvenir.Status)) / 2;
                    }
                    break;
                case 41: // Varunaxx
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        modifier = 4;
                    else
                        modifier = (int)DateTime.Now.DayOfWeek - 3;
                    break;
                case 42: // WitekWitek
                    modifier = bomb.GetSerialNumberLetters().Any(x => "WI".Contains(x)) || realBomb.Any(x => "Simon Sends".Equals(x)) ? 3 : -2;
                    break;
                case 43: // xorote
                    modifier = virtualBomb.Any(x => x.Author.Contains("SpeakingEvil")) || realBomb.Any(x => x.Author.Contains("SpeakingEvil")) ? 4 : -1;
                    break;
                case 44: // Zaakeil
                    modifier = 5 - (bomb.GetBatteryCount() + bomb.GetPortCount());
                    break;
                case 45: // Zaphod
                    if (virtualBomb.Any(x => "42".Equals(x.Name)) || realBomb.Any(x => "42".Equals(x.Name)))
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
