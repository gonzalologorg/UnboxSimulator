using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class webService : MonoBehaviour {

    public GameObject leaderBoard;
    public Dropdown dropDown;

    public static webService singleton;

    public void Start()
    {
        singleton = this;
    }


    string cField;
    int cValue;
    public void sendUpdate(string field, int value)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            cField = field;
            cValue = value;
            if(PlayerPrefs.GetString("steamid", "") != "")
                StartCoroutine("cSendUpdate");
        }
    }

    public IEnumerator cSendUpdate()
    {
        string url = "http://rgbmill.net/unboxer/update.php";
        web = new WWWForm();
        web.AddField("action", cField.Substring(cField.Length - 3));
        web.AddField("s", PlayerPrefs.GetString("steamid", ""));
        web.AddField("nt", cValue.ToString());
        WWW w = new WWW(url, web);
        yield return w;
        try { }
        catch (Exception)
        { throw; }
    }

    WWWForm web;

    public void verifyRegister()
    {
        PlayerPrefs.SetString("steamid", transform.GetChild(1).GetChild(1).GetComponent<InputField>().text);
        transform.GetChild(1).gameObject.SetActive(false);
        StartCoroutine("checkRegister");
    }

    public IEnumerator checkRegister()
    {
        string url = "http://rgbmill.net/unboxer/check.php?s=" + PlayerPrefs.GetString("steamid", "");
        web = new WWWForm();
        web.AddField("unusuals", PlayerPrefs.GetInt("stat_unus"));
        web.AddField("contracts", PlayerPrefs.GetInt("stat_contr"));
        web.AddField("money", Mathf.RoundToInt(PlayerPrefs.GetFloat("money")));
        web.AddField("elite", PlayerPrefs.GetInt("stat_elite"));

        WWW w = new WWW(url, web);
        yield return w;
        try
        {
            if (w.text == "1")
            {
                transform.GetChild(3).gameObject.SetActive(true);
                startRequest();
            }
            else
            {
                PlayerPrefs.SetString("steamid", "");
            }
        }
        catch (Exception)
        {

            throw;
        }

    }

    public void openRegister()
    {
        Application.OpenURL("http://rgbmill.net/unboxer");
    }

    public void checkIfSteamID()
    {
        StartCoroutine("checkRegister");

        if (PlayerPrefs.GetString("steamid", "") != "")
        {
            
            transform.GetChild(3).gameObject.SetActive(true);
            startRequest();
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    WWW fetchJSON;
    public IEnumerator doWebFetching()
    {
        string action = dropDown.options[dropDown.value].text;
        string url = "http://rgbmill.net/unboxer/fetch.php?action=" + action.ToLower() + "&page=" + PlayerPrefs.GetInt("RankPage",1).ToString();
        fetchJSON = new WWW(url);

        yield return fetchJSON;
        try
        {
            rankData[] info = JsonHelper.getJsonArray<rankData>(fetchJSON.text);

            for (int i = 0; i < info.Length; i++)
            {
                int am = 0;
                switch (action.ToLower())
                {
                    case "unusuals":
                        am = info[i].unusuals;
                        break;
                    case "money":
                        am = info[i].money;
                        break;
                    case "contracts":
                        am = info[i].contracts;
                        break;
                    case "elite":
                        am = info[i].elite;
                        break;
                    default:
                        am = info[i].unusuals;
                        break;
                }
                leaderBoard.transform.GetChild(i).GetComponent<userRank>().setPlayer(info[i].nick, action.ToUpper() + ": " + (action.ToUpper() == "MONEY" ? "$" : "") + am, info[i].avatar);
            }

        }
        catch (Exception)
        {
            throw;
        }
    }

    public void startRequest()
    {
        StartCoroutine("doWebFetching");
    }

    [System.Serializable]
    public class rankData
    {
        public string nick;
        public string avatar;
        public int unusuals;
        public int contracts;
        public int money;
        public int elite;
    }
}

public class JsonHelper
{
    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}