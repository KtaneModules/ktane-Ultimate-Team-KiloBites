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

public class UltimateTeamScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public Image[] iconRender;
	public Texture offlineSprite;
	public Sprite timer;
	public TextAsset offlineJson;

	public KMSelectable[] mainButtons;
	//public KMSelectable flipBombButton;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	private List<KtaneModule> allMods;
	private Texture spriteSheet;

	private Sprite[] icons = new Sprite[11];
	private int[] mods = new int[11];
	private bool bombFlipped, needy, boss;
	private bool[] bossIx = new bool[11];
	private bool[] needyIx = new bool[11];


	void Awake()
    {

		moduleId = moduleIdCounter++;

		/*
		foreach (KMSelectable button in Buttons)
			button.OnInteract() += delegate () { ButtonPress(button); return false; };
		*/

		//Button.OnInteract += delegate () { ButtonPress(); return false; };

    }

	
	void Start()
    {
		needy = Range(0, 100) == 50;
		boss = Range(0, 100) == 75;
		var a = Range(0, 11);
		var b = Enumerable.Range(0, 11).Where(x => x != a).PickRandom();

		needyIx[a] = needy;
		bossIx[b] = boss;
		StartCoroutine(getJson());
		StartCoroutine(getSpriteSheet());
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
			raw = request.downloadHandler.text;
		}
        allMods = JsonConvert.DeserializeObject<Root>(raw).KtaneModules;

		while (spriteSheet == null)
		{
			yield return null;
		}
		generateModule();
    }

	IEnumerator getSpriteSheet()
	{
		yield return null;

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

	void generateModule()
	{
		var maxY = allMods.Max(mod => mod.Y);

		mods[Range(0, 11)] = -1;

        for (int i = 0; i < 11; i++)
		{
			if (mods[i] == -1)
			{
				icons[i] = timer;
			}
			else
			{
				var types = new[] { "FullBoss", "Needy", "Widget" };

				if (bossIx[i])
				{
					mods[i] = Enumerable.Range(0, allMods.Count).Where(x => types[0].Equals(allMods[x].BossStatus)).PickRandom();
				}
				else if (needyIx[i])
				{
					mods[i] = Enumerable.Range(0, allMods.Count).Where(x => types[1].Equals(allMods[x].Type)).PickRandom();
				}
				else
				{
					mods[i] = Enumerable.Range(0, allMods.Count).Where(x => x != mods[i] && !types[0].Equals(allMods[x].BossStatus) && !types[1].Equals(allMods[x].Type) && !types[2].Contains(allMods[x].Type)).PickRandom();
				}

				KtaneModule usedMod = allMods[mods[i]];
				icons[i] = Sprite.Create(spriteSheet as Texture2D, new Rect(32 * usedMod.X, 32 * (maxY - usedMod.Y), 32, 32), new Vector2(0.5f, 0.5f));
			}
		}
		displaySprites();
	}

	void displaySprites()
	{
		for (int i = 0; i < 6; i++)
		{
			iconRender[i].sprite = icons[bombFlipped ? i + 6 : i];
		}

	}
	
	
	void Update()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





