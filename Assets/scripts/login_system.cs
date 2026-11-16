using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public static class utils
{
    public static string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}

[System.Serializable]
public class ServerResponse
{
    public string state;
    public string message;
    public string body;

    public static ServerResponse CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ServerResponse>(jsonString);
    }
}

[System.Serializable]
public class UserResponseData
{
    public string email;
    public string password;
    public int money;
    public string moneyHash;
    public string dlc;

    public static UserResponseData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<UserResponseData>(jsonString);
    }
}

public class login_system : MonoBehaviour {

    public InputField loginEmail;
    public InputField loginPass;

    public InputField regEmail;
    public InputField regPass;
    public InputField regRePass;

    public GameObject regPanel;

    public Text title;
    public Text message;

    void Start()
    {
        ZPlayerPrefs.Initialize("rgbmill", "iZyDERma");
    }

    public void login()
    {
        StartCoroutine(doLogin());
    }

    public void register()
    {
        StartCoroutine(doRegister());
    }

    public IEnumerator doLogin()
    {
        string url = "http://localhost/login.php?user=" + loginEmail.text + "&pass=" + loginPass.text;
        UnityWebRequest request = new UnityWebRequest(url);
        yield return request;
        UserResponseData result = JsonUtility.FromJson<UserResponseData>(request.downloadHandler.text);
        title.text = result.email;
        message.text = result.password;
        if (result.email != "Error")
        {
            Debug.Log(result.email);
            ZPlayerPrefs.SetInt("campaing_money", result.money);
            ZPlayerPrefs.SetString("email", result.email);
            ZPlayerPrefs.SetString("password", result.password);
            if (utils.Md5Sum(result.email + "1" + "unboxer") == result.dlc)
            {
                ZPlayerPrefs.SetInt("campaing_dlc", 1);
            }
        }
    }

    public IEnumerator doRegister()
    {
        if (regPass.text == regRePass.text && regPass.text.Length < 4)
        {
            string url = "http://localhost/register.php?user=" + regEmail.text + "&pass=" + regPass.text;
            UnityWebRequest request = new UnityWebRequest(url);
            yield return request;
            Debug.Log(request.downloadHandler.text);
            ServerResponse result = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
            title.text = result.state;
            message.text = result.message;
            if (result.state == "Success")
            {
                ZPlayerPrefs.SetString("email", regEmail.text);
                ZPlayerPrefs.SetString("password", regPass.text);
                ZPlayerPrefs.SetInt("campaing_dlc", 0);
            }
        } else
        {
            regPanel.SetActive(true);
            title.text = "Error";
            message.text = "Passwords mismatch!";
        }
    }
}
