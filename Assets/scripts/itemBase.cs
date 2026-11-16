using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SQLite4Unity3d;

public class itemBase : MonoBehaviour {

    public int quality = 0;
    public int strange = 0;
    public int state = 0;
    public int page = 0;
    public int pos = 0;
    public bool unusual = false;
    public int itemClass = 0;
    public string item_name;
    public string crate;

    public Color oColor;

    private Button.ButtonClickedEvent doClick = new Button.ButtonClickedEvent();

    public void buildSprite(string id,Texture2D img, int state=0)
    {
        bool canCreate = true;
        transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
        Sprite spr;
        foreach (var item in crateComposer.singleton.texturePool){
            if ((state == 0 && item.id == id) || (item.id+crateDrops.qli[state]== id))
            {
                spr = item.sprite;
                canCreate = false;
                transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = spr;
            }
        }

        if (canCreate)
        {
            Rect rec = new Rect(0, 0, img.width, img.height);
            spr = Sprite.Create(img, rec, Vector2.zero);

            crateComposer.TexturePool tp = new crateComposer.TexturePool();
            tp.id = state == 0 ? id : id + crateDrops.qli[state];
            tp.sprite = spr;

            crateComposer.singleton.texturePool.Add(tp);
            transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = spr;
        }
        
    }

    void Start()
    {
        GetComponent<Button>().onClick = doClick;
        doClick.AddListener(checkOpen);
    }

    string key;
    public bool selected = false;

    void checkOpen()
    {
        if (item_name.EndsWith("gold"))
        {
            itemClass = 4;
        }else
        {
            itemClass = 0;
        }

        GameObject.Find("selectsound").GetComponent<AudioSource>().Play();

        if (crateComposer.singleton.inContract)
        {
            if (crateComposer.singleton.contractQuality != quality)
                return;

            crateComposer.singleton.contractQuality = quality == 0 ? -1 : quality;

            oColor = transform.GetChild(0).GetComponent<Image>().color;

            if (!selected)
            {
                selected = true;
                transform.GetChild(0).GetComponent<Image>().color = new Color(1, 182f / 255, 26f / 255);
                crateComposer.singleton.contractList.Add(this);
            }
            else
            {
                selected = false;
                transform.GetChild(0).GetComponent<Image>().color = StringTable.singleton.getColor(quality);
                crateComposer.singleton.contractList.Remove(this);
            }

            crateComposer.singleton.main.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = crateComposer.singleton.contractList.Count+"/10 Selected";
            if(crateComposer.singleton.contractList.Count == 10)
            {
                crateComposer.singleton.sendContract();
            }
            return;
        }

        crateComposer.singleton.tempContract = quality;

        if (crateComposer.singleton.inContract)
        {
            crateComposer.singleton.contractQuality = quality == 0 ? -1 : quality;
            crateComposer.singleton.options.transform.GetChild(4).gameObject.SetActive(quality < 4);
            crateComposer.singleton.options.transform.Find("melt").gameObject.SetActive(quality == 4);
        }
        else
            crateComposer.singleton.contractQuality = -1;


        if (crateComposer.singleton.canDelete)
        {
            crateComposer.singleton.toDelete = this;

            if(PlayerPrefs.GetInt("askdelete",1) == 0)
            {
                crateComposer.singleton.deleteObject();
                return;
            }
            crateComposer.singleton.deletePanel.SetActive(true);

            string nm = "";

            bool found = false;
            for (int i = 0; i < GameObject.Find("Crates").transform.childCount; i++)
            {
                foreach (crateDrops.Drops item in GameObject.Find("Crates").transform.GetChild(i).GetComponent<crateDrops>().drops)
                {
                    if (item.id == item_name)
                    {
                        nm = item.name;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            crateComposer.singleton.deletePanel.transform.GetChild(0).GetComponent<Text>().text = "Delete "+(unusual ? "UNUSUAL " : "") + (strange != 0 ? "STRANGE " : "") + nm + "?";
            return;
        }

        if (quality > 0 || itemClass == 4)
        {
            crateComposer.singleton.noClick.SetActive(true);
            crateComposer.singleton.collection = this;
            GameObject op = crateComposer.singleton.options;
            op.SetActive(true);

            crateComposer.singleton.options.transform.GetChild(4).gameObject.SetActive(quality < 4 && itemClass != 4);
            crateComposer.singleton.options.transform.Find("melt").gameObject.SetActive(quality == 4);

            op.transform.Find("collection").GetComponent<Button>().interactable = itemClass != 4;
            op.transform.Find("prices").GetComponent<Button>().interactable = itemClass != 4;

            op.transform.GetChild(0).GetComponent<Image>().sprite = transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite;
            if (strange > 0)
            {
                op.transform.GetChild(1).GetComponent<Text>().enabled = true;
                op.transform.GetChild(1).GetComponent<Text>().text = "Kills: "+(strange - 1).ToString();
                op.transform.GetChild(3).GetChild(0).GetComponent<Image>().enabled = true;
            }
            else
            {
                op.transform.GetChild(3).GetChild(0).GetComponent<Image>().enabled = false;
                op.transform.GetChild(1).GetComponent<Text>().enabled = false;
            }

            op.transform.GetChild(3).GetChild(1).GetComponent<Image>().enabled = unusual;

            string nm = "";

            bool found = false;
            for (int i = 0; i < GameObject.Find("Crates").transform.childCount; i++)
            {
                foreach (crateDrops.Drops item in GameObject.Find("Crates").transform.GetChild(i).GetComponent<crateDrops>().drops)
                {
                    if(item.id == item_name)
                    {
                        nm = item.name;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            if(nm == "")
            {
                nm = GameObject.Find("Translator").GetComponent<StringTable>().getTranslation(item_name);
            }


            crateComposer.singleton.itemName = nm;
            crateComposer.singleton.itemState = state;
            crateComposer.singleton.isStrange = strange>=1;
            crateComposer.singleton.isUnusual = unusual;
            
            op.transform.GetChild(2).GetComponent<Text>().text = (unusual ? "UNUSUAL " : "") + (strange != 0 ? "STRANGE " : "") + nm;
            op.transform.GetChild(2).GetComponent<Text>().color = StringTable.singleton.getColor(itemClass == 4 ? 6 : quality);

            return;
        }

        if (crateComposer.singleton.isPlaying)
        {
            crateComposer.singleton.openMessage(1);
            return;
        }

        foreach (crateComposer.CrateSystem item in crateComposer.singleton.crateDefinition)
        {
            if(item.crate == item_name)
            {
                itemClass = 1;
                key = item.key;
                break;
            }
            if(item.key == item_name)
            {
                itemClass = 0;
                key = item.crate;
                break;
            }
        }

        List<ItemDB> items = crateComposer.singleton.DataService.GetItems();

        for (int i = 0; i < items.Count; i++)
        {
            if(items[i].name == key)
            {
                crateComposer.singleton.openCrate(item_name, key, itemClass);
                break;
            }
        }
    }


}
