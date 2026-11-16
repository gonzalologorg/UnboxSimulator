using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class crateDrops : MonoBehaviour {

    //Que llave abre tal caja y que caja abre tal llave
    [System.Serializable]
    public class Drops
    {
        public int quality;
        public int item_class;
        public int state = 0;
        public string id;
        public string name;
        public string description;
        public bool unusual = false;
        public int strange;
        public Sprite spr;
    }

    public static string[] qli = new string[] {"new","new","wear","tested","worn","scarred" };

    public static string[] qld = new string[] { "Factory New", "Factory New", "Minimal Wear", "Field-Tested", "Well-Worn", "Battle Scarred" };

    public bool isWeapon;

    public List<Drops> drops;
    
    public Drops openItem()
    {
        int chance = 1;
        int mxChance = Random.Range(0, 100);
        if (mxChance >= 96)
            chance = 4;
        else if (mxChance >= 90)
            chance = 3;
        else if (mxChance >= 80)
            chance = 2;

        List<Drops> pDrops = new List<Drops>();
        foreach (Drops pitem in drops)
        {
            if(pitem.quality == chance)
            {
                pDrops.Add(pitem);
            }
        }

        Drops fItem = pDrops[Random.Range(0, pDrops.Count)];
        fItem.unusual = false;
        fItem.strange = 0;

        fItem.strange = Random.Range(0, 100) < 25 ? 1 : 0;

        if (Random.Range(0, 400) >= 397)
        {
            fItem.unusual = true;
            if(fItem.strange == 0)
                fItem.strange = Random.Range(0, 3);
        }

        if (isWeapon)
        {
            fItem.state = Random.Range(1, 5);
            fItem.item_class = 2;
        }
        else
        {
            fItem.item_class = 3;
        }

        createImage(fItem);

        return fItem;
    }

    public void createImage(Drops item)
    {
        string extra = isWeapon ? ("_" + qli[Mathf.Clamp(item.state, 1, 5)]) : "";
        bool canCreate = true;
        Sprite spr;
        foreach (var pool in crateComposer.singleton.texturePool)
        {
            if ((pool.id + extra) == item.id)
            {
                spr = pool.sprite;
                item.spr = spr;
                canCreate = false;
            }
        }

        if (canCreate)
        {
            Texture2D img = Resources.Load<Texture2D>("files/"+name+"/" + item.id + extra);
            try
            {
                Rect rec = new Rect(0, 0, img.width, img.height);
                spr = Sprite.Create(img, rec, Vector2.zero);

                crateComposer.TexturePool tp = new crateComposer.TexturePool();
                tp.id = item.id + extra;
                tp.sprite = spr;

                crateComposer.singleton.texturePool.Add(tp);
                item.spr = spr;
            }catch(System.Exception e)
            {
                Debug.Log(name);
                Debug.Log(item.id);
                Debug.Log(extra);
            }

        }

    }
}

