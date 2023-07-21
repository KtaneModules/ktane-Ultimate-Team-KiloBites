using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using static UnityEngine.Debug;

public class UltimateTeamService : MonoBehaviour {

    public bool loaded = false;
    public bool connectedJson, connectedSprite = false;
    public Texture offlineSprite;
    public TextAsset offlineJson;
    public Texture spriteSheet;
    public List<KtaneModule> allMods;

    void Start()
    {
        name = "Ultimate Team Service";
        StartCoroutine(getRawJson());
        StartCoroutine(getSpriteSheet());
    }

    IEnumerator getRawJson()
    {
        string raw;
        var request = UnityWebRequest.Get("https://ktane.timwi.de/json/raw");

        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Log("Connection error! Using offline raw JSON from 7/21/23.");
            raw = offlineJson.text;
        }
        else
        {
            connectedJson = true;
            Log("JSON has now been obtained.");
            raw = request.downloadHandler.text;
        }

        allMods = JsonConvert.DeserializeObject<Root>(raw).KtaneModules;

        while (spriteSheet == null)
        {
            yield return null;
        }
        loaded = true;
    }

    IEnumerator getSpriteSheet()
    {
        var request = UnityWebRequestTexture.GetTexture("https://ktane.timwi.de/iconsprite");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Log("Connection error! Using default spritesheet from 7/21/23.");
            spriteSheet = offlineSprite;
        }
        else
        {
            Log("The spritesheet has now been obtained.");
            connectedSprite = true;
            spriteSheet = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}