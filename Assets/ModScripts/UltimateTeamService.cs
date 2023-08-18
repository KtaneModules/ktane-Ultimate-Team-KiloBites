using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using static UnityEngine.Debug;

public class UltimateTeamService : MonoBehaviour
{
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
        Log("[Ultimate Team Service] Downloading JSON...");

        string raw;
        var request = UnityWebRequest.Get("https://ktane.timwi.de/json/raw");

        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Log("[Ultimate Team Service] JSON download failed. Using raw JSON from 8/12/23.");
            raw = offlineJson.text;
        }
        else
        {
            connectedJson = true;
            Log("[Ultimate Team Service] JSON download succeeded.");
            raw = request.downloadHandler.text;
        }

        allMods = JsonConvert.DeserializeObject<Root>(raw).KtaneModules;

        while (spriteSheet == null)
            yield return null;
        loaded = true;
    }

    IEnumerator getSpriteSheet()
    {
        Log("[Ultimate Team Service] Downloading Sprite Sheet...");

        var request = UnityWebRequestTexture.GetTexture("https://ktane.timwi.de/iconsprite");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Log("[Ultimate Team Service] Sprite sheet download failed. Using spritesheet from 8/12/23.");
            spriteSheet = offlineSprite;
        }
        else
        {
            Log("[Ultimate Team Service] Sprite sheet download succeeded.");
            connectedSprite = true;
            spriteSheet = ((DownloadHandlerTexture) request.downloadHandler).texture;
        }
    }
}