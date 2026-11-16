using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class userRank : MonoBehaviour {

    int id;
    Image icon;
    Text nick;
    Text stat;
    Text pos;

    string imageURL;

	void Start () {
        icon = transform.GetChild(0).GetComponent<Image>();
        icon.color = new Color(1, 1, 1, 0.15f);

        nick = transform.GetChild(1).GetComponent<Text>();
        nick.text = "Empty Slot";

        stat = transform.GetChild(2).GetComponent<Text>();
        stat.text = "No Stat";

        pos = transform.GetChild(3).GetComponent<Text>();
        pos.text = "#" + ((PlayerPrefs.GetInt("rank_page",1) - 1) * 10 + Convert.ToInt32(name)).ToString() ;
    }
	
	public void setPlayer(string nick, string amount, string avatar)
    {
        this.nick.text = nick;
        imageURL = avatar;
        stat.text = amount;
        StartCoroutine("setAvatarImage");
    }

    IEnumerator setAvatarImage()
    {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        WWW www = new WWW(imageURL);
        yield return www;
        www.LoadImageIntoTexture(tex);
        icon.color = new Color(1, 1, 1, 1);
        icon.sprite = Sprite.Create(tex,new Rect(0,0,tex.width, tex.width), Vector2.zero,100);
    }
}
