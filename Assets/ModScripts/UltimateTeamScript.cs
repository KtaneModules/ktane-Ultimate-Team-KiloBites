﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class UltimateTeamScript : MonoBehaviour
{
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public Image[] iconRender;
    public Image[] expertRender1, expertRender2, expertRender3, expertRender4, expertRender5, expertRender6;
    public Sprite timer, placeholder;
    public KMSelectable[] mainButtons;
    public KMSelectable flipBombButton;
    public KMSelectable[] expertCards;
    public KMSelectable statusLightButton;
    public Sprite[] arrowSprites;
    public Sprite[] expertBGSprites;
    public Image[] tickMarks;
    public Image bombCasing;
    public Image bombEdge;
    public Image throbber;
    public Image stamp;
    public MeshRenderer LED;
    public MeshRenderer surface;
    public Sprite[] profilePictures;
    public Image[] profilePictureRends;
    public Text[] expertNameRends;

    private KMAudio.KMAudioRef Sound;
    private Coroutine mainButtonsAnimCoroutine;
    private List<KtaneModule> allMods;
    private List<KtaneModule> virtualBomb = new List<KtaneModule>();
    private List<KtaneModule> realBomb = new List<KtaneModule>();
    private string[] moduleNames = new string[12];
    private List<string> expertDifficulties = new List<string>();
    private List<List<int>> expertProf = new List<List<int>>();
    private List<List<int>> moduleProf = new List<List<int>>();
    private Texture spriteSheet;
    private Sprite[] icons = new Sprite[12];
    private List<int> experts = new List<int>();
    private List<int> team = new List<int>();
    private string[] currExpertPrefDiffs = new string[6];
    private string[] currExpertNames = new string[6];
    private int[] mods = new int[12];
    private bool[] bossIx = new bool[12];
    private bool[] needyIx = new bool[12];
    private bool[] selected = new bool[6];
    private bool boss, bombFlipped, cannotPress, needy, rightMenu;
    private static string connected;

    private Image[][] renders;

    private static readonly string[] expertNames = { "1254", "AlexCorruptor", "Axo", "BigCrunch22", "Cinnabar", "Crazycaleb", "CyanixDash", "Danielstigman", "dicey", "Diffuse", "diskoQs", "Espik", "eXish", "Floofy Floofles", "GhostSalt", "GoodHood", "Gwen", "JyGein", "Kilo", "Konoko", "Kugel", "Kuro", "Lexa", "LilyFlair", "Lulu", "Mage", "Marksam", "MasQuéÉlite", "meh", "NShep", "Obvious", "Piissii", "Quinn Wuest", "redpenguin", "Rosenothorns03", "Scoping Landscape", "Setra", "Sierra", "tandyCake", "TheFullestCircle", "Timwi", "Varunaxx", "WitekWitek", "xorote", "Zaakeil", "Zaphod" };
    private static readonly string[] expertPreferredDiffs = { "Easy", "Easy", "Medium", "Hard", "Medium", "VeryHard", "Easy", "VeryHard", "Hard", "Medium", "Easy", "VeryHard", "VeryHard", "Easy", "Medium", "Easy", "Easy", "Easy", "Hard", "Medium", "Hard", "Easy", "Medium", "Easy", "Medium", "Medium", "VeryHard", "Medium", "Hard", "Hard", "VeryHard", "VeryHard", "Hard", "Hard", "Medium", "VeryHard", "Medium", "Easy", "Hard", "Easy", "Hard", "Hard", "Medium", "VeryHard", "VeryHard", "VeryHard" };

    void Awake()
    {
        moduleId = moduleIdCounter++;

        renders = new Image[][] { expertRender1, expertRender2, expertRender3, expertRender4, expertRender5, expertRender6 };

        for (int i = 0; i < mainButtons.Length; i++)
        {
            int x = i;
            mainButtons[x].OnInteract += delegate { if (!cannotPress && (rightMenu ? x == 0 : x == 1)) mainButtonPress(x); return false; };
        }
        for (int i = 0; i < expertCards.Length; i++)
        {
            int x = i;
            expertCards[x].OnInteract += delegate { if (!cannotPress && rightMenu) cardPress(x); return false; };
            expertCards[x].OnHighlight += delegate { expertCards[x].GetComponent<Image>().sprite = expertBGSprites[1]; };
            expertCards[x].OnHighlightEnded += delegate { expertCards[x].GetComponent<Image>().sprite = expertBGSprites[0]; };
            tickMarks[x].transform.localScale = Vector3.zero;
        }
        mainButtons[0].transform.localPosition = new Vector3(mainButtons[0].transform.localPosition.x, 0.0078f, mainButtons[0].transform.localPosition.z);
        flipBombButton.OnInteract += delegate { if (!cannotPress && !rightMenu) flipBombButtonPress(); return false; };
        flipBombButton.OnHighlight += delegate { flipBombButton.GetComponent<Image>().sprite = arrowSprites[1]; };
        flipBombButton.OnHighlightEnded += delegate { flipBombButton.GetComponent<Image>().sprite = arrowSprites[0]; };
        statusLightButton.OnInteract += delegate
        {
            if (!cannotPress && rightMenu && !moduleSolved)
            {
                bool correct = true;
                for (int i = 0; i < 6; i++)
                {
                    if (selected[i] != team.Contains(i))
                    {
                        correct = false;
                    }
                }
                if (correct)
                {
                    StopAllCoroutines();
                    LED.material.color = new Color32(34, 35, 38, 255);
                    StartCoroutine(solve());
                }
                else
                {
                    var strikeExperts = new List<string>();

                    for (int i = 0; i < 6; i++)
                    {
                        if (selected[i])
                        {
                            strikeExperts.Add(currExpertNames[i]);
                        }
                    }

                    var strikeMessage = strikeExperts.Count > 0 ? $"{strikeExperts.PickRandom()} has been selected and decided to strike on {moduleNames.Where(x => !"[TIMER]".Equals(x)).PickRandom()}, striking out the bomb entirely." : "You have selected nobody for this bomb, therefore blowing up the bomb as a result.";

                    Log($"[Ultimate Team #{moduleId}] {strikeMessage} Strike!");

                    Module.HandleStrike();
                }
            }
            return false;
        };
        bombEdge.transform.localScale = Vector3.zero;
        expertCards[0].transform.parent.localPosition = new Vector3(0.2f, 0, 0);
        bombCasing.transform.parent.localPosition = Vector3.zero;
        stamp.transform.localScale = Vector3.zero;
        StartCoroutine(throb());
    }

    void Start()
    {
        cannotPress = true;
        needy = Range(0, 2) == 0;
        boss = Range(0, 4) > 1;
        var a = Range(0, 12);
        var b = Enumerable.Range(0, 12).Where(x => x != a).PickRandom();

        needyIx[a] = needy;
        bossIx[b] = boss;

        bombCasing.transform.parent.localScale = Vector3.zero;

        if (spriteSheet == null && allMods == null)
            StartCoroutine(setup());
        else
        {
            bombCasing.transform.parent.localScale = Vector3.one;
            throbber.transform.parent.localScale = Vector3.zero;
            Log($"[Ultimate Team #{moduleId}] {connected}");
            generateModule();
        }
    }

    IEnumerator setup()
    {
        var utService = FindObjectOfType<UltimateTeamService>();

        if (utService == null)
            throw new Exception("It cannot find the service!");

        yield return new WaitUntil(() => utService.loaded);

        if (utService.connectedJson && utService.connectedSprite)
            StartCoroutine(LEDFlash());
        cannotPress = false;

        connected = utService.connectedJson && utService.connectedSprite ? "This module is connected to the internet, grabbing the latest modules possible from the repo." : "This module is not connected to the internet and therefore will use its backup from 7/22/23.";

        Log($"[Ultimate Team #{moduleId}] {connected}");

        allMods = utService.allMods;
        spriteSheet = utService.spriteSheet;
        bombCasing.transform.parent.localScale = Vector3.one;
        throbber.transform.parent.localScale = Vector3.zero;
        generateModule();

    }

    void mainButtonPress(int pos)
    {
        if (mainButtonsAnimCoroutine != null)
            StopCoroutine(mainButtonsAnimCoroutine);
        mainButtonsAnimCoroutine = StartCoroutine(mainButtonsAnim(pos));
        rightMenu = !rightMenu;
        Audio.PlaySoundAtTransform("main button", mainButtons[pos].transform);
        mainButtons[pos].AddInteractionPunch();
        StartCoroutine(switchMenu());
    }

    void flipBombButtonPress()
    {
        StartCoroutine(flipBombAnim());
        flipBombButton.AddInteractionPunch(0.5f);
    }

    void cardPress(int pos)
    {
        selected[pos] = !selected[pos];
        tickMarks[pos].transform.localScale = selected[pos] ? Vector3.one : Vector3.zero;
        if (Sound != null)
            Sound.StopSound();
        Sound = Audio.HandlePlaySoundAtTransformWithRef(selected[pos] ? "tick" : "erase", tickMarks[pos].transform, false);
    }

    void generateModule()
    {
        var maxY = allMods.Max(mod => mod.Y);

        mods[Range(0, 12)] = -1;

        for (int i = 0; i < 12; i++)
        {
            if (mods[i] == -1)
            {
                icons[i] = timer;
                moduleNames[i] = "[TIMER]";
                expertDifficulties.Add("[TIMER]");
            }
            else
            {
                var types = new[] { "Needy", "Widget" };

                var bossTypes = new[] { "FullBoss", "SemiBoss" };

                if (bossIx[i])
                    mods[i] = Enumerable.Range(0, allMods.Count).Where(x => bossTypes[0].Equals(allMods[x].BossStatus) || bossTypes[1].Equals(allMods[x].BossStatus) && allMods[x].X != 0 && allMods[x].Y != 0).PickRandom();
                else if (needyIx[i])
                    mods[i] = Enumerable.Range(0, allMods.Count).Where(x => types[0].Equals(allMods[x].Type) && allMods[x].X != 0 && allMods[x].Y != 0).PickRandom();
                else
                    mods[i] = Enumerable.Range(0, allMods.Count).Where(x => x != mods[i] && !bossTypes[0].Equals(allMods[x].BossStatus) && !bossTypes[1].Equals(allMods[x].BossStatus) && !types[0].Equals(allMods[x].Type) && !types[1].Equals(allMods[x].Type) && allMods[x].X != 0 && allMods[x].Y != 0).PickRandom();

                KtaneModule usedMod = allMods[mods[i]];
                virtualBomb.Add(usedMod);
                icons[i] = Sprite.Create(spriteSheet as Texture2D, new Rect(32 * usedMod.X, 32 * (maxY - usedMod.Y), 32, 32), new Vector2(0.5f, 0.5f));
                icons[i].texture.filterMode = FilterMode.Point;
                moduleNames[i] = usedMod.Name;
                expertDifficulties.Add(usedMod.ExpertDifficulty);
            }
        }
        List<string> ids = allMods.Select(x => x.ModuleID).ToList();
        foreach (string modID in Bomb.GetModuleIDs())
            if (ids.Contains(modID))
                realBomb.Add(allMods[ids.IndexOf(modID)]);

        Log($"[Ultimate Team #{moduleId}] The virtual bomb is displayed as follows: {moduleNames.Join(", ")}");

        experts = Enumerable.Range(0, profilePictures.Length).ToList();
        experts.Shuffle();
        experts = experts.Take(6).ToList();

        for (int i = 0; i < 6; i++)
        {
            currExpertNames[i] = expertNames[experts[i]];
            currExpertPrefDiffs[i] = expertPreferredDiffs[experts[i]];
        }

        Log($"[Ultimate Team #{moduleId}] The candidates of experts are: {currExpertNames.Join(", ")}");
        Log($"[Ultimate Team #{moduleId}] These experts have these preferred difficulties: {currExpertPrefDiffs.Join(", ").Replace("VeryEasy", "Very Easy").Replace("VeryHard", "Very Hard")}");

        assignProficiencies();
        displaySprites();
        calculations();
    }

    void assignProficiencies()
    {
        tryagain:
        var q = new List<List<int>>();
        var q2 = new List<int>();

        for (int i = 0; i < 6; i++)
        {
            var prof = genProficiency(q);
            q.Add(prof);
            q2.AddRange(prof);
        }
        var counts = new int[12];

        foreach (var item in q2)
            counts[item]++;

        if (counts.Where(x => x > 2).Count() > 0)
            goto tryagain;
        else
        {
            expertProf = q.ToList();
            for (int i = 0; i < 12; i++)
            {
                moduleProf.Add(new List<int>());
                for (int j = 0; j < 6; j++)
                    if (expertProf[j].Contains(i))
                        moduleProf.Last().Add(j);
            }
        }
    }

    List<int> genProficiency(List<List<int>> q)
    {
        tryagain:
        var prob = Range(0, 5);
        List<int> ixs = Enumerable.Range(0, 12).Where(x => mods[x] != -1).ToList().Shuffle().Take(prob == 0 ? 1 : prob == 4 ? 3 : 2).ToList();

        for (int i = 0; i < q.Count; i++)
            if (ixs.Count == q[i].Count)
            {
                var fail = true;
                for (int j = 0; j < q[i].Count; j++)
                    if (ixs[j] != q[i][j])
                        fail = false;
                if (fail)
                    goto tryagain;
            }

        return ixs;
    }

    void displaySprites()
    {
        for (int i = 0; i < 6; i++)
        {
            iconRender[i].sprite = icons[bombFlipped ? i + 6 : i];
            profilePictureRends[i].sprite = profilePictures[experts[i]];
            expertNameRends[i].text = currExpertNames[i].ToUpperInvariant();

            for (int j = 0; j < 2; j++)
            {
                renders[i][j].sprite = placeholder;
                renders[i][j].enabled = mods[bombFlipped ? i + 6 : i] != -1 && expertDifficulties[bombFlipped ? i + 6 : i] != "VeryEasy" && expertDifficulties[bombFlipped ? i + 6 : i] != "[TIMER]";
            }

            for (int j = 0; j < moduleProf[bombFlipped ? i + 6 : i].Count; j++)
            {
                renders[i][j].sprite = profilePictures[experts[moduleProf[bombFlipped ? i + 6 : i][j]]];
            }
        }
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 2; j++)
                if (renders[i][j].sprite.name == "!placeholder")
                    renders[i][j].enabled = false;
    }

    void calculations()
    {
        var scores = ScoringSystem.baseScores(Bomb.GetSerialNumber());
        Log($"[Ultimate Team #{moduleId}] The assigned scores for the experts in reading order: {scores.Join(", ")}");

        var ixes = new[] { "Easy", "Medium", "Hard", "VeryHard" };

        for (int i = 0; i < 6; i++)
        {
            scores[i] += expertDifficulties.Count(x => currExpertPrefDiffs[i].Equals(x)) * (Array.IndexOf(ixes, currExpertPrefDiffs[i]) + 1);

        }


        Log($"[Ultimate Team #{moduleId}] After adding proficiency scores for each expert in reading order: {scores.Join(", ")}");
        scores = ScoringSystem.modifyingScores(Bomb, virtualBomb, realBomb, scores, experts.ToArray(), moduleId, expertPreferredDiffs);
        Log($"[Ultimate Team #{moduleId}] After calculating each score by difficulties present on the virtual bomb and applying conditions for each expert in reading order: {scores.Join(", ")}");
        team = Enumerable.Range(0, 6).ToList();
        team = team.Select((x, ix) => new { x, ix }).OrderByDescending(x => scores[x.ix]).Select(x => x.x).ToList();
        if (expertDifficulties.Count(x => "VeryEasy".Equals(x)) >= 4 || team.Any(x => x == 66669420))
        {
            var log = team.Any(x => x == 66669420) ? "CyanixDash is your only expert since the firstmost condition applied. Don't take anyone else." : "Since there are at least 4 Very Easy modules on the virtual bomb, you'll be taking only one expert.";
            team = team.Take(1).ToList();
            Log($"[Ultimate Team #{moduleId}] {log}");
        }
        else
        {
            team = team.Take(2).ToList();
            Log($"[Ultimate Team #{moduleId}] Since there are less Very Easy modules on the virtual bomb, you'll be taking two experts.");
        }
        var resultFormat = team.Count == 1 ? $"According to the highest score, you should take {currExpertNames[team[0]]}." : $"According to the two highest scores, you should take {currExpertNames[team[0]]} and {currExpertNames[team[1]]}.";
        Log($"[Ultimate Team #{moduleId}] {resultFormat}");
    }

    private IEnumerator solve()
    {
        Log($"[Ultimate Team #{moduleId}] You have selected the correct expert(s). Solved!");
        moduleSolved = true;
        Module.HandlePass();
        stamp.transform.localScale = Vector3.one;
        expertCards[3].AddInteractionPunch(5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Stamp, stamp.transform);
        expertCards[0].transform.parent.localScale = Vector3.zero;
        bombCasing.transform.parent.localScale = Vector3.zero;
        cannotPress = true;
        float timer = 0;
        while (timer < 0.5f)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        Audio.PlaySoundAtTransform("solve", Module.transform);
    }

    private IEnumerator mainButtonsAnim(int pos, float duration = 0.05f)
    {
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            mainButtons[pos].transform.localPosition = Vector3.Lerp(new Vector3(mainButtons[pos].transform.localPosition.x, 0.01441f, mainButtons[pos].transform.localPosition.z), new Vector3(mainButtons[pos].transform.localPosition.x, 0.0078f, mainButtons[pos].transform.localPosition.z), timer / duration);
            mainButtons[1 - pos].transform.localPosition = Vector3.Lerp(new Vector3(mainButtons[1 - pos].transform.localPosition.x, 0.0078f, mainButtons[1 - pos].transform.localPosition.z), new Vector3(mainButtons[1 - pos].transform.localPosition.x, 0.01441f, mainButtons[1 - pos].transform.localPosition.z), timer / duration);
        }
        mainButtons[pos].transform.localPosition = new Vector3(mainButtons[pos].transform.localPosition.x, 0.0078f, mainButtons[pos].transform.localPosition.z);
        mainButtons[1 - pos].transform.localPosition = new Vector3(mainButtons[1 - pos].transform.localPosition.x, 0.01441f, mainButtons[1 - pos].transform.localPosition.z);
    }

    private IEnumerator flipBombAnim(float duration = 0.2f)
    {
        cannotPress = true;
        Audio.PlaySoundAtTransform("spin", flipBombButton.transform);
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            bombCasing.transform.localScale = new Vector3(0.9f * Mathf.Cos(0.5f * Mathf.PI * timer / duration), 0.9f, 0.9f);
            bombEdge.transform.localScale = new Vector3(0.9f * Mathf.Sin(0.5f * Mathf.PI * timer / duration), 0.9f, 0.9f);
            bombCasing.transform.localPosition = new Vector3(Easing.OutSine(timer, 0, 0.0135f, duration), bombCasing.transform.localPosition.y, 0);
            bombEdge.transform.localPosition = new Vector3(Easing.InSine(timer, -0.06975f, 0, duration), bombEdge.transform.localPosition.y, 0);
        }
        bombFlipped = !bombFlipped;
        displaySprites();
        timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            bombCasing.transform.localScale = new Vector3(0.9f * Mathf.Sin(0.5f * Mathf.PI * timer / duration), 0.9f, 0.9f);
            bombEdge.transform.localScale = new Vector3(0.9f * Mathf.Cos(0.5f * Mathf.PI * timer / duration), 0.9f, 0.9f);
            bombCasing.transform.localPosition = new Vector3(Easing.InSine(timer, -0.0135f, 0, duration), bombCasing.transform.localPosition.y, 0);
            bombEdge.transform.localPosition = new Vector3(Easing.OutSine(timer, 0, 0.06975f, duration), bombEdge.transform.localPosition.y, 0);
        }
        bombCasing.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        bombCasing.transform.localPosition = new Vector3(0, bombCasing.transform.localPosition.y, 0);
        bombEdge.transform.localScale = Vector3.zero;
        bombEdge.transform.localPosition = new Vector3(0.06975f, bombEdge.transform.localPosition.y, 0);
        cannotPress = false;
    }

    private IEnumerator switchMenu(float duration = 0.15f)
    {
        cannotPress = true;
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            expertCards[0].transform.parent.localPosition = Vector3.Lerp(new Vector3(!rightMenu ? 0 : 0.2f, 0, 0), new Vector3(!rightMenu ? 0.2f : 0, 0, 0), timer / duration);
            bombCasing.transform.parent.localPosition = Vector3.Lerp(new Vector3(!rightMenu ? -0.2f : 0, 0, 0), new Vector3(!rightMenu ? 0 : -0.2f, 0, 0), timer / duration);
            surface.material.SetTextureOffset("_MainTex", new Vector2(Mathf.Lerp(rightMenu ? 0 : 0.1f, rightMenu ? 0.1f : 0, timer / duration), 0));
        }
        expertCards[0].transform.parent.localPosition = new Vector3(!rightMenu ? 0.2f : 0, 0, 0);
        bombCasing.transform.parent.localPosition = new Vector3(!rightMenu ? 0 : -0.2f, 0, 0);
        surface.material.SetTextureOffset("_MainTex", new Vector2(rightMenu ? 0.1f : 0, 0));
        cannotPress = false;
    }

    private IEnumerator LEDFlash(float interval = 0.5f)
    {
        int i = 0;
        while (true)
        {
            LED.material.color = new[] { new Color32(34, 35, 38, 255), new Color32(186, 68, 35, 255) }[i];
            i = 1 - i;
            float timer = 0;
            while (timer < interval)
            {
                yield return null;
                timer += Time.deltaTime;
            }
        }
    }

    private IEnumerator throb(float interval = 0.05f)
    {
        int i = 0;
        while (true)
        {
            throbber.transform.localEulerAngles = throbber.transform.localEulerAngles - new Vector3(0, 0, 30);
            i = 1 - i;
            float timer = 0;
            while (timer < interval)
            {
                yield return null;
                timer += Time.deltaTime;
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use '!{0} switch' to change menus. Use '!{0} flip' to flip the virtual bomb on the Mission menu, and use '!{0} submit 1 3' to select the first and third experts in reading order in the Experts menu (and deselect any others), then press the status light.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] commandArray = command.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        if (commandArray.Length == 0)
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        if (command == "switch")
        {
            yield return null;
            mainButtons[rightMenu ? 0 : 1].OnInteract();
        }
        else if (command == "flip" && !rightMenu)
        {
            yield return null;
            flipBombButton.OnInteract();
        }
        else if (command == "flip")
        {
            yield return "sendtochaterror You can't use that command in the Experts menu!";
            yield break;
        }
        else if (commandArray.First() == "submit" && rightMenu)
        {
            if (commandArray.Length > 1 && commandArray.Length < 4)
            {
                for (int i = 0; i < commandArray.Length - 1; i++)
                {
                    int thing = 0;
                    if (!int.TryParse(commandArray[i + 1], out thing) || int.Parse(commandArray[i + 1]) < 1 || int.Parse(commandArray[i + 1]) > 6)
                    {
                        yield return "sendtochaterror I can't select " + (commandArray.Length == 2 ? "that expert!" : "those experts!");
                        yield break;
                    }
                }
                List<int> chosen = new List<int>();
                for (int i = 0; i < commandArray.Length - 1; i++)
                    chosen.Add(int.Parse(commandArray[i + 1]) - 1);
                yield return null;
                for (int i = 0; i < 6; i++)
                    if (chosen.Contains(i) != selected[i])
                    {
                        expertCards[i].OnInteract();
                        float timer = 0;
                        while (timer < 0.1f)
                        {
                            yield return null;
                            timer += Time.deltaTime;
                        }
                    }
                statusLightButton.OnInteract();
            }
            else
            {
                yield return "sendtochaterror I can't select those experts!";
                yield break;
            }
        }
        else if (commandArray.First() == "submit")
        {
            yield return "sendtochaterror You can't use that command in the Mission menu!";
            yield break;
        }
        else
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (cannotPress)
            yield return true;
        if (!rightMenu)
            mainButtons[1].OnInteract();
        while (cannotPress)
            yield return true;
        for (int i = 0; i < 6; i++)
            if (selected[i] != team.Contains(i))
                expertCards[i].OnInteract();
        yield return true;
        statusLightButton.OnInteract();
    }
}