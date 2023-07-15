using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using KModkit;
using Newtonsoft.Json;
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
    public Texture offlineSprite;
    public Sprite timer;
    public TextAsset offlineJson;
    public KMSelectable[] mainButtons;
    public KMSelectable flipBombButton;
    public KMSelectable[] expertCards;
    public Sprite[] arrowSprites;
    public Sprite[] expertBGSprites;
    public Image[] tickMarks;
    public Image bombCasing;
    public Image bombEdge;
    public Image throbber;
    public MeshRenderer LED;
    public MeshRenderer surface;
    public Sprite[] profilePictures;
    public Image[] profilePictureRends;
    public Text[] expertNameRends;

    private KMAudio.KMAudioRef Sound;
    private Coroutine mainButtonsAnimCoroutine;
    private List<KtaneModule> allMods;
    private string[] moduleNames = new string[12];
    private Texture spriteSheet;
    private Sprite[] icons = new Sprite[12];
    private List<int> experts = new List<int>();
    private int[] mods = new int[12];
    private bool[] bossIx = new bool[12];
    private bool[] needyIx = new bool[12];
    private bool[] selected = new bool[6];
    private bool boss, bombFlipped, cannotPress, needy, rightMenu;

    void Awake()
    {
        moduleId = moduleIdCounter++;
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
        bombEdge.transform.localScale = Vector3.zero;
        expertCards[0].transform.parent.localPosition = new Vector3(0.2f, 0, 0);
        bombCasing.transform.parent.localPosition = Vector3.zero;
        StartCoroutine(throb());
    }

    void Start()
    {
        needy = Range(0, 2) == 0;
        boss = Range(0, 4) > 1;
        var a = Range(0, 12);
        var b = Enumerable.Range(0, 12).Where(x => x != a).PickRandom();

        needyIx[a] = needy;
        bossIx[b] = boss;
        StartCoroutine(getJson());
        StartCoroutine(getSpriteSheet());
        bombCasing.transform.parent.localScale = Vector3.zero;
    }

    IEnumerator getJson()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://ktane.timwi.de/json/raw");

        string raw;
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Log("Connection error! Using obtained RAW JSON from 7/14/23.");
            raw = offlineJson.text;
        }
        else
        {
            Log("The JSON has now been obtained.");
            StartCoroutine(LEDFlash());
            raw = request.downloadHandler.text;
        }
        bombCasing.transform.parent.localScale = Vector3.one;
        throbber.transform.parent.localScale = Vector3.zero;
        allMods = JsonConvert.DeserializeObject<Root>(raw).KtaneModules;

        while (spriteSheet == null)
            yield return null;
        generateModule();
    }

    IEnumerator getSpriteSheet()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://ktane.timwi.de/iconsprite");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Log("Connection error! Using default spritesheet from 7/14/23.");
            spriteSheet = offlineSprite;
        }
        else
        {
            Log("The spritesheet has now been obtained.");
            spriteSheet = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
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
                icons[i] = timer;
            else
            {
                var types = new[] { "FullBoss", "Needy", "Widget" };

                if (bossIx[i])
                    mods[i] = Enumerable.Range(0, allMods.Count).Where(x => types[0].Equals(allMods[x].BossStatus)).PickRandom();
                else if (needyIx[i])
                    mods[i] = Enumerable.Range(0, allMods.Count).Where(x => types[1].Equals(allMods[x].Type)).PickRandom();
                else
                    mods[i] = Enumerable.Range(0, allMods.Count).Where(x => x != mods[i] && !types[0].Equals(allMods[x].BossStatus) && !types[1].Equals(allMods[x].Type) && !types[2].Contains(allMods[x].Type)).PickRandom();

                KtaneModule usedMod = allMods[mods[i]];
                icons[i] = Sprite.Create(spriteSheet as Texture2D, new Rect(32 * usedMod.X, 32 * (maxY - usedMod.Y), 32, 32), new Vector2(0.5f, 0.5f));
                icons[i].texture.filterMode = FilterMode.Point;
            }
        }
        experts = Enumerable.Range(0, profilePictures.Length).ToList();
        experts.Shuffle();
        experts = experts.Take(6).ToList();
        displaySprites();
    }

    void displaySprites()
    {
        for (int i = 0; i < 6; i++)
        {
            iconRender[i].sprite = icons[bombFlipped ? i + 6 : i];
            profilePictureRends[i].sprite = profilePictures[experts[i]];
            expertNameRends[i].text = profilePictures[experts[i]].name.ToUpperInvariant();
        }
    }

    void Update()
    {

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

    private IEnumerator throb (float interval = 0.05f)
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
    private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}