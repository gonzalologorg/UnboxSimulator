using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class itemStore : MonoBehaviour
{

    public string Name;
    public int Type;
    public bool isStrange;
    public bool isUnusual;
    public Text priceLabel;

    WWW web;

    void Start()
    {
        priceLabel = GetComponent<Text>();
    }

    string[] states = { "","Battle Scarred", "Well-Worn", "Field-Tested", "Minimal Wear", "Factory New" };

    public void startPerform()
    {
        StartCoroutine("perform");
    }

    public IEnumerator perform()
    {
        GetComponent<Text>().text = "Loading...";
        Name = crateComposer.singleton.itemName;
        Type = crateComposer.singleton.itemState;
        isStrange = crateComposer.singleton.isStrange;
        isUnusual = crateComposer.singleton.isUnusual;

        string url = "http://steamcommunity.com/market/priceoverview/?currency=1&appid=440&market_hash_name=" + System.Uri.EscapeUriString((isStrange ? "Strange " : "") + (isUnusual ? "Unusual " : "") + Name + (Type==0 ? "" : " (" + states[Type] + ")"));
        url = url.Replace("(", "%28");
        url = url.Replace(")", "%29");
        web = new WWW(url);
        yield return web;
        try
        {
            if (JsonUtility.FromJson<itemData>(web.text).success)
            {
                priceLabel.text = JsonUtility.FromJson<itemData>(web.text).lowest_price == null ? "Priceless!!!": JsonUtility.FromJson<itemData>(web.text).lowest_price;
            }
        }
        catch (Exception)
        {
            GetComponent<Text>().text = "Not Found :(";
            throw;
        }
        
    }
}

public class itemData
{
    public bool success;
    public string lowest_price;
    public string volume;
    public string median_price;
}
