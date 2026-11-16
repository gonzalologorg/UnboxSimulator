using UnityEngine;
using System.Collections;

public class StringTable : MonoBehaviour {

    [System.Serializable]
    public class Strings
    {
        public string id;
        public string data;
    }

    public Strings[] db;

    [System.Serializable]
    public class ColorTypes
    {
        public int id;
        public string color;
    }

    public ColorTypes[] colorTypes;

    public static StringTable singleton;

    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    void Start () {
        singleton = this;
	}

    public string getItemString(string id)
    {
        GameObject cr = GameObject.Find("Crates");
        for (int i = 0; i < cr.transform.childCount; i++)
        {
            foreach (crateDrops.Drops item in cr.transform.GetChild(i).GetComponent<crateDrops>().drops)
            {
                if (item.id == id)
                {
                    return item.name;       
                }
            }
        }
        return "";
    }
	
	public string getTranslation(string obj)
    {
        foreach (Strings item in db)
        {
            if (item.id == obj) return item.data;
            if (item.data == obj) return item.id;
        }
        return "%null%";
    }

    public int getQuality(string id)
    {
        GameObject cr = GameObject.Find("Crates");
        for (int i = 0; i < cr.transform.childCount; i++)
        {
            if (cr.transform.GetChild(i).GetComponent<crateDrops>() != null)
            {
                foreach (crateDrops.Drops item in cr.transform.GetChild(i).GetComponent<crateDrops>().drops)
                {
                    if (item.id == id)
                    {
                        return item.quality;
                    }
                }
            }
        }
        return 0;
    }

    public Color getColor(int id)
    {
        foreach (ColorTypes item in colorTypes)
        {
            if (item.id == id) return HexToColor(item.color);
        }
        return Color.white;
    }

}
