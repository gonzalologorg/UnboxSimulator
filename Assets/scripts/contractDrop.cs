using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class contractDrop : MonoBehaviour {

    public List<string> merc;
    public List<string> comm;
    public List<string> assa;
    public List<string> elit;

    public crateDrops.Drops returnItem(int quality,bool drop=false, string crate = "")
    {
        string title = "";

        List <crateDrops.Drops> drp = new List<crateDrops.Drops>();

        foreach (crateDrops.Drops item in transform.parent.Find(crate).GetComponent<crateDrops>().drops)
        {
            if (item.quality == quality)
                drp.Add(item);
        }

        crateDrops.Drops mItem = drp[Random.Range(0, drp.Count)];
        title = mItem.id;

        crateDrops.Drops fItem = new crateDrops.Drops();
        fItem.quality = quality;
        fItem.item_class = mItem.item_class;
        fItem.state = mItem.item_class != 2 ? 0 : drop ? Random.Range(3,5):1;
        fItem.id = title;
        fItem.strange = drop ? 0 : Random.Range(1,100) > 80 ? 1 : 0;
        fItem.name = StringTable.singleton.getItemString(title);

        createImage(fItem);

        if (!drop)
        {
            PlayerPrefs.SetInt("stat_contr", PlayerPrefs.GetInt("stat_contr", 0) + 1);
            webService.singleton.sendUpdate("contracts", PlayerPrefs.GetInt("stat_contr"));
            Social.ReportScore((long)PlayerPrefs.GetFloat("stat_contr", 0), "grp.contractsdone", crateComposer.singleton.reportScore);
            if (PlayerPrefs.GetFloat("stat_contr") > 49)
            {
                Social.ReportProgress("grp.sign_here", 100, crateComposer.singleton.reportScore);
            }
        }

        return fItem;
    }

    void createImage(crateDrops.Drops item)
    {

        string extra = "_" + crateDrops.qli[Mathf.Clamp(item.state, 1, 5)];

        bool canCreate = true;
        Sprite spr;
        foreach (var pool in crateComposer.singleton.texturePool)
        {
            if (pool.id + "_new" == item.id || pool.id == item.id)
            {
                spr = pool.sprite;
                item.spr = spr;
                canCreate = false;
            }
        }

        if (canCreate)
        {
            Texture2D img = null;

            for (int z = 0; z < GameObject.Find("Crates").transform.childCount; z++)
            {
                img = Resources.Load<Texture2D>("files/" + GameObject.Find("Crates").transform.GetChild(z).name + "/" + item.id + (item.item_class == 4 ? "" : item.item_class != 2 ? "" : "_new"));
                if(img != null)
                {
                    break;
                }
            }

            Rect rec = new Rect(0, 0, img.width, img.height);
            spr = Sprite.Create(img, rec, Vector2.zero);

            crateComposer.TexturePool tp = new crateComposer.TexturePool();
            tp.id = item.id + extra;
            tp.sprite = spr;

            crateComposer.singleton.texturePool.Add(tp);
            item.spr = spr;
        }
    }
}
