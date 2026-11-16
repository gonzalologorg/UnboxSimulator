using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using SQLite4Unity3d;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif

public class crateComposer : MonoBehaviour {

    public static crateComposer singleton;

    public GameObject main;
    public GameObject store;
    public GameObject unlock;
    public GameObject checkingOut;
    public GameObject checkOutPanel;
    public GameObject options;
    public GameObject getLoot;
    public GameObject noClick;
    public GameObject invAccess;
    public GameObject freePanel;
    public GameObject settings;
    public GameObject deletePanel;
    public GameObject collectionPanel;
    public GameObject versioner;
    public GameObject contracter;
    public GameObject badgeCreator;
    public GameObject meltButton;

    public string itemName;
    public int itemState;
    public bool isStrange;
    public bool isUnusual;

    public DataService DataService;

    public itemBase toDelete;
    public itemBase collection;

    List<ItemDB> items;
    List<ItemDB> badgeItems;

    public int contractQuality = 0;
    public List<itemBase> contractList = new List<itemBase>();

    public bool canDelete;
    public bool isPlaying;
    public bool isFree = false;

    public bool isMeelting = false;

#region adsWorkaround

    public bool usePauseOverride;
    private static bool _isPaused;

    void OnApplicationPause(bool isPaused)
    {
        if (!usePauseOverride || isPaused == _isPaused) return;

        if (isPaused) Debug.Log("App was paused.");
        else Debug.Log("App was resumed.");

        if (usePauseOverride) PauseOverride(isPaused);
    }

    public static void PauseOverride(bool pause)
    {

        if (pause) Debug.Log("Pause game while ad is shown.");
        else Debug.Log("Resume game after ad is closed.");

        AudioListener.volume = pause ? 0f : 1f;
        Time.timeScale = pause ? 0f : 1f;

        _isPaused = pause;
    }

#endregion

    //Que llave abre tal caja y que caja abre tal llave
    [System.Serializable]
    public class CrateSystem
    {
        public string crate;
        public string key;
    }

    //Sistema de optimizacion de textura (Evita que se cargue 2 veces la misma textura
    [System.Serializable]
    public class TexturePool
    {
        public string id;
        public Sprite sprite;
    }

    //Ya que las texturas son dinamicas, entonces usamos una lista
    public List<TexturePool> texturePool;
    //Los elementos no tantos, entonces esto esta bien
    public CrateSystem[] crateDefinition;

    void Start()
    {
        contractQuality = -1;

        PlayerPrefs.SetInt("page", PlayerPrefs.GetInt("page", 0) == 0? 1: PlayerPrefs.GetInt("page"));
        PlayerPrefs.SetInt("pages_unlocked", PlayerPrefs.GetInt("pages_unlocked", 0) == 0? 2: PlayerPrefs.GetInt("pages_unlocked"));

        singleton = this;
        //Conseguimos la base de datos de items
        DataService = new DataService("ItemDB.db");
        updatePrice();
        canDelete = false;
        main.transform.GetChild(1).Find("pages").GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("page").ToString() + "/" + PlayerPrefs.GetInt("pages_unlocked").ToString();

        updateItemsBase(PlayerPrefs.GetInt("page"));

        if (PlayerPrefs.GetInt("RunTimes", 0) == 0)
        {
            DataService.erase();
            updateItemsBase(1);
            PlayerPrefs.SetInt("RunTimes", 1);
            PlayerPrefs.SetInt("page", 1);
            PlayerPrefs.SetInt("pages_unlocked", 2);
            main.transform.GetChild(1).Find("pages").GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("page").ToString() + "/" + PlayerPrefs.GetInt("pages_unlocked").ToString();
        }

        int w = Screen.width;
        int h = Screen.height;

        if (w == 480 && h == 800)
            main.transform.GetChild(1).GetChild(2).GetComponent<GridLayoutGroup>().cellSize = new Vector2(88,80);
        if (w == 480 && h == 854)
            main.transform.GetChild(1).GetChild(2).GetComponent<GridLayoutGroup>().cellSize = new Vector2(82, 80);
        if (w == 320)
            main.transform.GetChild(1).GetChild(2).GetComponent<GridLayoutGroup>().cellSize = new Vector2(100, 83);
        if (w == 600)
            main.transform.GetChild(1).GetChild(2).GetComponent<GridLayoutGroup>().cellSize = new Vector2(84, 83);
        if (w == 800)
            main.transform.GetChild(1).GetChild(2).GetComponent<GridLayoutGroup>().cellSize = new Vector2(92, 83);
        if (w == 1080)
            main.transform.GetChild(1).GetChild(2).GetComponent<GridLayoutGroup>().cellSize = new Vector2(82, 83);

        float vol = PlayerPrefs.GetFloat("volume", 1);
        GameObject.Find("playsound").GetComponent<AudioSource>().volume = vol*0.7f;
        GameObject.Find("selectsound").GetComponent<AudioSource>().volume = vol * 0.7f;
        GameObject.Find("picksound").GetComponent<AudioSource>().volume = vol * 0.7f;

        settings.transform.GetChild(0).GetComponent<Slider>().value = vol;

        store.transform.Find("checkout").GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("autocheck", 0)==1;
        settings.transform.Find("confirm_delete").GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("askdelete", 1) == 1;

        if(PlayerPrefs.GetString("Version","") != Application.version)
        {
            versioner.gameObject.SetActive(true);
        }

        Advertisement.Initialize("1014670");
    }

    private void ProccessAuthentification(bool obj)
    {
        if (obj)
        {
            Debug.Log("Authenticated, checking achievements");

            // Request loaded achievements, and register a callback for processing them
            Social.LoadAchievements(ProcessLoadedAchievements);
        }
        else
            Debug.Log("Failed to authenticate");
    }

    private void ProcessLoadedAchievements(IAchievement[] obj)
    {
    }

    public void openScoreboard()
    {
        Social.ShowLeaderboardUI();
    }

    //Update version
    public void updateVersion()
    {
        PlayerPrefs.SetString("Version", Application.version);
    }
    

    public void rateThisApp()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.rgbmill.unboxsim");
    }


    public void setVolume(float vol)
    {
        PlayerPrefs.SetFloat("volume", vol);
        GameObject.Find("playsound").GetComponent<AudioSource>().volume = vol;
        GameObject.Find("selectsound").GetComponent<AudioSource>().volume = vol;
        GameObject.Find("picksound").GetComponent<AudioSource>().volume = vol;
    }

    public void exitGame()
    {
        Application.Quit();
    }

    //Verificamos que podemos ver el video, si no podemos, entonces desactivamos el item del video
    public void checkIfVideo()
    {
        store.transform.GetChild(3).Find("itemad").GetChild(0).gameObject.SetActive(Advertisement.IsReady("rewardedVideo"));
        store.transform.GetChild(3).Find("itemad").GetChild(1).gameObject.SetActive(Advertisement.IsReady("rewardedVideo"));
        store.transform.GetChild(3).Find("itemad").GetComponent<purchasableItem>().unique = Advertisement.IsReady("rewardedVideo")? "back_extra":"";

        store.transform.GetChild(3).Find("itemadb").GetChild(0).gameObject.SetActive(Advertisement.IsReady("rewardedVideo"));
        store.transform.GetChild(3).Find("itemadb").GetChild(1).gameObject.SetActive(Advertisement.IsReady("rewardedVideo"));
        store.transform.GetChild(3).Find("itemadb").GetComponent<purchasableItem>().unique = Advertisement.IsReady("rewardedVideo") ? "back_extra" : "";
    }

    //Limpiamos la informacion de pagos
    static void clearPurchase()
    {
        PlayerPrefs.SetString("act_obj", "");
        PlayerPrefs.SetFloat("act_price", 0);
    }

    //Realizamos la compra
    public void doPurchase()
    {
        if (PlayerPrefs.GetString("act_obj") == "back_extra" || PlayerPrefs.GetString("act_obj") == "turbo_mode")
        {
            if (Advertisement.IsReady("rewardedVideo"))
            {
                isTurboMode = PlayerPrefs.GetString("act_obj") == "turbo_mode";
                var options = new ShowOptions { resultCallback = HandleShowResult };
                Advertisement.Show("rewardedVideo", options);
            }
            return;
        }
        //Revisamos si tenemos espacio para comprar un item mas
        if ((DataService.GetItems().Count+1) < 20*(PlayerPrefs.GetInt("pages_unlocked",1)))
        {
            //El objeto es gratis?
            if (!isFree)
            {
                PlayerPrefs.SetFloat("money", PlayerPrefs.GetFloat("money", 0) + PlayerPrefs.GetFloat("act_price"));
                webService.singleton.sendUpdate("money", Mathf.RoundToInt(PlayerPrefs.GetFloat("money")));

                PlayerPrefs.SetFloat("money_wasted", PlayerPrefs.GetFloat("money_wasted", 0) + PlayerPrefs.GetFloat("act_price"));
                
                if (PlayerPrefs.GetFloat("money_wasted", 0) >= 20)
                {
                    PlayerPrefs.SetFloat("money_wasted", 0);
                    freePanel.SetActive(true);
                    isFree = true;
                }
            }else
            {
                isFree = false;
            }
            //Insertamos el objeto en la base de datos
            DataService.addItem(PlayerPrefs.GetString("act_obj"));
            foreach (CrateSystem dcrate in crateDefinition)
            {
                if(dcrate.crate == PlayerPrefs.GetString("act_obj"))
                {
                    DataService.addItem(dcrate.key);
                    break;
                }
            }
            updatePrice();
            updateItemsBase(PlayerPrefs.GetInt("page"));
            clearPurchase();
        }
    }

    bool isTurboMode = false;

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                if (!isTurboMode)
                {
                    PlayerPrefs.SetInt("pages_unlocked", PlayerPrefs.GetInt("pages_unlocked", 1) + 1);
                    main.transform.GetChild(1).Find("pages").GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("page").ToString() + "/" + PlayerPrefs.GetInt("pages_unlocked").ToString();
                }
                else
                {
                    PlayerPrefs.SetInt("turboMode", Random.Range(3,7));
                }
                break;
            case ShowResult.Skipped:
                GameObject.Find("video").SetActive(false);
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }


    //Actualizamos los items y asignamos su calidad
    public void updateItemsBase(int page)
    {
        for (int i = 0; i < invAccess.transform.childCount; i++)
        {
            invAccess.transform.GetChild(i).GetComponent<itemBase>().item_name = "";
            invAccess.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
            invAccess.transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
            invAccess.transform.GetChild(i).GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
            invAccess.transform.GetChild(i).GetChild(0).GetChild(2).GetComponent<Image>().enabled = false;

            invAccess.transform.GetChild(i).GetComponent<itemBase>().quality = 0;
            invAccess.transform.GetChild(i).GetComponent<itemBase>().strange = 0;
            invAccess.transform.GetChild(i).GetComponent<itemBase>().unusual = false;
        }

        items = DataService.GetItems();
        int it = 0;
        int el = 0;

        foreach (ItemDB item in items)
        {
            if (item.name == "") continue;

            if (el >= 20*(page-1) && el < 20*page && (contractQuality == -1 || (contractQuality == StringTable.singleton.getQuality(item.name))))
            {
                Texture2D img = Resources.Load<Texture2D>("files/nograde/" + item.name);
                if (img != null)
                {
                    invAccess.transform.GetChild(it).GetComponent<itemBase>().item_name = item.name;
                    invAccess.transform.GetChild(it).GetComponent<itemBase>().quality = 0;
                    invAccess.transform.GetChild(it).GetComponent<itemBase>().buildSprite(item.name,img);
                    invAccess.transform.GetChild(it).GetComponent<itemBase>().state = item.state;
                    invAccess.transform.GetChild(it).GetComponent<itemBase>().page = PlayerPrefs.GetInt("page");
                    invAccess.transform.GetChild(it).GetComponent<itemBase>().pos = it;
                }

                if (item.state >= 0)
                {
                    Texture2D wmg = null;
                    if (true)
                    {
                        for (int z = 0; z < GameObject.Find("Crates").transform.childCount; z++)
                        {
                            wmg = Resources.Load<Texture2D>("files/" + GameObject.Find("Crates").transform.GetChild(z).name + "/" + item.name + ((item.state > 0) ? "_" + crateDrops.qli[item.state] : ""));
                            if (wmg != null)
                                break;
                        }
                    }

                    if(wmg == null)
                    {
                        wmg = Resources.Load<Texture2D>("static/crates/gold/" + item.name);
                        if (wmg != null && item.name.EndsWith("gold"))
                            invAccess.transform.GetChild(it).GetComponent<itemBase>().itemClass = 4;
                    }

                    if (wmg != null)
                    {
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().item_name = item.name;
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().itemClass = 0;
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().buildSprite(item.name, wmg,item.state);
                        invAccess.transform.GetChild(it).GetChild(0).GetComponent<Image>().enabled = true;
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().strange = item.strange;
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().state = item.state;
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().unusual = item.unusual > 0;
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().page = PlayerPrefs.GetInt("page");
                        invAccess.transform.GetChild(it).GetComponent<itemBase>().pos = it;

                        //invAccess.transform.GetChild(it).GetComponent<itemBase>().quality = item.quality;
                        invAccess.transform.GetChild(it).GetChild(0).GetComponent<Image>().color = StringTable.singleton.getColor(item.quality);

                        bool found = false;

                        for (int z = 0; z < GameObject.Find("Crates").transform.childCount; z++)
                        {
                            foreach (crateDrops.Drops wep in GameObject.Find("Crates").transform.GetChild(z).GetComponent<crateDrops>().drops)
                            {
                                if (wep.id == item.name)
                                {
                                    found = true;
                                    invAccess.transform.GetChild(it).GetComponent<itemBase>().quality = wep.quality;
                                    bool isForceSelected = false;
                                    if (contractQuality > -1)
                                    {
                                        foreach (itemBase dit in contractList)
                                        {
                                            if (dit.item_name == item.name && dit.state == item.state && dit.strange == item.strange && dit.page == PlayerPrefs.GetInt("page") && dit.pos == it)
                                                isForceSelected = true;
                                        }
                                    }
                                    invAccess.transform.GetChild(it).GetChild(0).GetComponent<Image>().color = canDelete ? new Color(186f / 255, 29f / 255, 29f / 255) : isForceSelected ? new Color(1, 182f / 255, 26f / 255) : StringTable.singleton.getColor(wep.quality);
                                    break;
                                }
                            }
                            if (found) break;
                        }
                        
                        if (item.unusual > 0)
                            invAccess.transform.GetChild(it).GetChild(0).GetChild(1).GetComponent<Image>().enabled = true;
                        if(item.strange > 0)
                            invAccess.transform.GetChild(it).GetChild(0).GetChild(2).GetComponent<Image>().enabled = true;

                    }
                }
                it++;
            }
            if (contractQuality == -1 || (contractQuality == StringTable.singleton.getQuality(item.name)))
                el++;
        }

        GameObject crates = GameObject.Find("Coordinator").transform.GetChild(1).gameObject;
        foreach (ItemDB item in items)
        {
            if (item.quality == -1)
            {
                bool forceSet = true;
                for (int i = 0; i < crates.transform.childCount - 1; i++)
                {
                    foreach (crateDrops.Drops dropitem in crates.transform.GetChild(i).GetComponent<crateDrops>().drops)
                    {
                        if (dropitem.id == item.name)
                        {
                            item.quality = dropitem.quality;
                            item.crate = crates.transform.GetChild(i).name;
                            DataService.update(item);
                            forceSet = false;
                            break;
                        }
                    }
                }
                if (forceSet)
                {
                    item.quality = -1;
                    item.crate = "Store";
                    DataService.update(item);
                }
            }
        }
    }

    public int CalculateChild(itemBase obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            if (obj.transform.GetChild(i) == obj)
                return i;
        }
        return 1;
    }

    //Funcion que redonda x digitos
    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    //Actualizamos el Label de lo que gastamos
    public void updatePrice()
    {
        store.transform.Find("spent").GetComponent<Text>().text = @"SPENT:
<size=32><color=#FFF2DC73>$" + Round(PlayerPrefs.GetFloat("money", 0),2).ToString() + "</color></size>";
    }
	
    //Buscamos el sprite para un id que ya conseguimos antes
	Sprite getSpriteFor(string id)
    {
        foreach (TexturePool item in texturePool)
        {
            if (item.id == id)
                return item.sprite;
        }
        return null;
    }

    //Muestra el mensaje de que se va a abrir una caja
    public void openCrate(string crate, string key, int order)
    {
        PlayerPrefs.SetInt("stat_unb", PlayerPrefs.GetInt("stat_unb", 0) + 1);
        if (PlayerPrefs.GetFloat("stat_unb") > 4999)
        {
            Social.ReportProgress("grp.lot_unbox", 100, crateComposer.singleton.reportScore);
        }

        unlock.SetActive(true);
        //Si el orden es 1, significa que es caja o llave, sino es llave y caja
        if (order==1)
        {
            PlayerPrefs.SetString("crateToOpen", crate);
            unlock.transform.GetChild(0).Find("crate").GetChild(0).GetComponent<Image>().sprite = getSpriteFor(crate);
            unlock.transform.GetChild(0).Find("key").GetChild(0).GetComponent<Image>().sprite = getSpriteFor(key);
            unlock.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = string.Format(@"USING <color=#9AFD59FF>{0}</color>
ON <color=#71A2EDFF>{1}</color>",StringTable.singleton.getTranslation(crate), StringTable.singleton.getTranslation(key));

        }
        else
        {
            PlayerPrefs.SetString("crateToOpen", key);
            unlock.transform.GetChild(0).Find("crate").GetChild(0).GetComponent<Image>().sprite = getSpriteFor(key);
            unlock.transform.GetChild(0).Find("key").GetChild(0).GetComponent<Image>().sprite = getSpriteFor(crate);
            unlock.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = string.Format(@"USING <color=#9AFD59FF>{0}</color>
ON <color=#71A2EDFF>{1}</color>", StringTable.singleton.getTranslation(key), StringTable.singleton.getTranslation(crate));
        }
    }

    //Quitamos la llave que usamos para abrir tal caja
    public void takeKeys()
    {
        string crate = PlayerPrefs.GetString("crateToOpen");

        foreach (CrateSystem item in crateDefinition)
        {
            if(item.crate == crate)
            {
                DataService.removeItem(item.key);
                break;
            }
        }

        DataService.removeItem(crate);
    }

    //Activamos la opcion de eliminar
    public void disableDelete()
    {
        if (canDelete)
        {
            doDelete();
        }
    }

    public void doDelete()
    {
        if (canDelete)
        {
            canDelete = false;
            ColorBlock cb = new ColorBlock();
            cb = transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<Button>().colors;
            cb.highlightedColor = new Color(1,1,1);
            cb.normalColor = new Color(1,1,1);
            transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<Button>().colors = cb;
            transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<Text>().color = new Color(0.2f, 0.2f, 0.2f);
        }
        else
        {
            canDelete = true;
            ColorBlock cb = new ColorBlock();
            cb = transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<Button>().colors;
            cb.highlightedColor = new Color(252f / 255, 103f / 255, 63f / 255);
            cb.normalColor = new Color(252f / 255, 103f / 255, 63f / 255);
            transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<Button>().colors = cb;
            transform.GetChild(0).GetChild(1).GetChild(4).GetChild(0).GetComponent<Text>().color = new Color(1,1,1);
        }

        for (int i = 0; i < invAccess.transform.childCount; i++)
        {
            if (invAccess.transform.GetChild(i).GetComponent<itemBase>().item_name != "")
            {
                invAccess.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = true;
                if (invAccess.transform.GetChild(i).GetComponent<itemBase>().quality != 0 || canDelete)
                    invAccess.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = canDelete ? new Color(186f / 255, 29f / 255, 29f / 255) : StringTable.singleton.getColor(invAccess.transform.GetChild(i).GetComponent<itemBase>().quality);
                else
                    invAccess.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
            }
        }

    }

    string[] tp = new string[] { "merc", "comm", "ass", "elite" };
    string[] grade = new string[] { "Mercenary", "Commando", "Assasin", "Elite" };

    public void setCrateContract(string contract)
    {
        PlayerPrefs.SetString("CrateContract", contract);
    }

    public void reportScore(bool could)
    {
        Debug.Log("We " + (could ? " make" : " couldn't make") + " it");
    }

    //Abrimos nuestra caja...Alfin!
    public void doOpenCrate()
    {
        if (PlayerPrefs.GetInt("turboMode", 0) > 7)
            PlayerPrefs.SetInt("turboMode", 0);
        PlayerPrefs.SetInt("turboMode", Mathf.Clamp(PlayerPrefs.GetInt("turboMode", 0) - 1, 0, 7));
        sortButton.transform.parent.gameObject.SetActive(true);
        isFree = false;
        transform.Find("loot").Find("content").GetChild(5).GetComponent<Image>().enabled = false;
        transform.Find("loot").Find("content").GetChild(6).GetComponent<Image>().enabled = false;

        string crate = PlayerPrefs.GetString("CrateContract","");

        crateDrops.Drops drop = null;
        if (contractQuality > 0)
            drop = GameObject.Find("contracts").GetComponent<contractDrop>().returnItem(contractQuality + 1, false, crate);
        else
        {
            drop = GameObject.Find("Coordinator").transform.Find("Crates").Find(PlayerPrefs.GetString("crateToOpen", "")).GetComponent<crateDrops>().openItem();
        }
        crateDrops.Drops dropItem = drop;

        if (dropItem.unusual)
        {
            PlayerPrefs.SetInt("stat_unus", PlayerPrefs.GetInt("stat_unus", 0) + 1);
            webService.singleton.sendUpdate("unusuals", PlayerPrefs.GetInt("stat_unus"));
#if UNITY_IOS
            Social.ReportScore((long)PlayerPrefs.GetInt("stat_unus",0), "grp.unusual", reportScore);
            Social.ReportProgress("grp.first_unusual", 100, reportScore);
#endif
        }

        if (dropItem.strange > 0)
            PlayerPrefs.SetInt("stat_stra", PlayerPrefs.GetInt("stat_stra", 0) + 1);

        if(contractQuality != -1)
        {
            foreach (itemBase item in contractList)
            {
                DataService.removeItem(item);
            }
        }
        contractList.Clear();

        transform.Find("loot").Find("content").GetChild(0).GetComponent<Image>().sprite = dropItem.spr;
        transform.Find("loot").GetChild(1).GetComponent<Text>().text = contractQuality == -1 ? "YOU <color=#FC673FFF>UNBOXED</color>:" : "YOU <color=#FC673FFF>RECEIVED</color>:";

        transform.Find("loot").Find("content").GetChild(1).GetComponent<Text>().text = (dropItem.unusual ? "UNUSUAL " : "")+(dropItem.strange != 0 ? "STRANGE ":"") + dropItem.name + "\n<size=20>"+ grade[Mathf.Clamp(dropItem.quality-1,0,3)]+ " Grade.</size>";
        transform.Find("loot").Find("content").GetChild(1).GetComponent<Text>().color = StringTable.singleton.getColor(dropItem.quality);
        transform.Find("loot").Find("content").GetChild(2).GetComponent<Text>().text = "Level "+Random.Range(1,100);
        transform.Find("loot").Find("content").GetChild(3).GetComponent<Text>().text = dropItem.item_class==2? crateDrops.qld[dropItem.state]:dropItem.description;

        if (dropItem.unusual)
        {
            transform.Find("loot").Find("content").GetChild(5).GetComponent<Image>().enabled = true;
        }

        if(dropItem.strange != 0)
        {
            transform.Find("loot").Find("content").GetChild(6).GetComponent<Image>().enabled = true;
        }

        DataService.addItem(dropItem.id, dropItem.state,dropItem.unusual ? Random.Range(1,4):0,dropItem.strange);
        PlayerPrefs.SetInt("stat_" + tp[dropItem.quality-1], PlayerPrefs.GetInt("stat_" + tp[dropItem.quality-1], 0) + 1);

        if (tp[dropItem.quality - 1] == "elite")
            webService.singleton.sendUpdate("elite", PlayerPrefs.GetInt("stat_elite"));

        if (dropItem.quality - 1 == 4)
        {
#if UNITY_IOS
            Social.ReportScore((long)PlayerPrefs.GetInt("stat_" + tp[dropItem.quality - 1], 0), "grp.elite_grade", crateComposer.singleton.reportScore);
#endif
        }
#if UNITY_IOS
        if (dropItem.quality - 1 == 3)
            Social.ReportScore((long)PlayerPrefs.GetInt("stat_" + tp[dropItem.quality - 1], 0), "grp.assasin_grade", crateComposer.singleton.reportScore);
#endif
        cancelContract();
        updateItemsBase(PlayerPrefs.GetInt("page"));

        List<crateDrops.Drops> allObjects = GameObject.Find("Crates").transform.Find(PlayerPrefs.GetString("crateToOpen", "")).GetComponent<crateDrops>().drops;
        bool canCreateBadge = true;
        badgeItems = new List<ItemDB>();

        foreach (crateDrops.Drops item in allObjects)
        {
            bool found = false;
            foreach (ItemDB tempItem in items)
            {
                if(tempItem.name == item.id)
                {
                    bool canAdd = true;
                    foreach (ItemDB bItem in badgeItems)
                    {
                        if(bItem.name == tempItem.name)
                        {
                            canAdd = false;
                        }
                    }
                    if (canAdd)
                    {
                        badgeItems.Add(tempItem);
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                canCreateBadge = false;
                break;
            }
        }

        if (canCreateBadge)
        {
            badgeCreator.SetActive(true);
            PlayerPrefs.SetInt("badges", PlayerPrefs.GetInt("badges", 0) + 1);
            Social.ReportScore((long)PlayerPrefs.GetInt("badges",1), "grp.aussie_badge", reportScore);
            Social.ReportProgress("grp.aussie_unbox", 100, reportScore);
        }

        doAd();
    }

    public void purgeBadge()
    {
        bool unusual = false;
        bool strange = false;
        foreach (ItemDB delItem in badgeItems)
        {
            if (delItem.unusual == 1)
                unusual = true;
            if (delItem.strange > 0)
                strange = true;

            DataService.removeItem(delItem);
        }
        DataService.addItem(PlayerPrefs.GetString("crateToOpen", "") + "_gold",0,unusual ? 1:0,strange ? 1:0,6,"Badge");
        updateItemsBase(PlayerPrefs.GetInt("page"));
    }

    public void movePage(int pos)
    {
        PlayerPrefs.SetInt("page", Mathf.Clamp(PlayerPrefs.GetInt("page")+pos, 1, PlayerPrefs.GetInt("pages_unlocked")));
        main.transform.GetChild(1).Find("pages").GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("page").ToString() + "/" + PlayerPrefs.GetInt("pages_unlocked").ToString();
        updateItemsBase(PlayerPrefs.GetInt("page"));
    }

    //Borrar stats
    public void deleteStats()
    {
        for (int i = 0; i < GameObject.Find("fields").transform.childCount; i++)
        {
            PlayerPrefs.SetInt("stat_"+ GameObject.Find("fields").transform.GetChild(i).name, 0);
        }

        DataService.erase();
        PlayerPrefs.SetInt("RunTimes", 1);
        PlayerPrefs.SetInt("page", 1);
        PlayerPrefs.SetInt("pages_unlocked", 2);
        PlayerPrefs.SetInt("money", 0);
        updateStats();
        main.SetActive(true);
        store.SetActive(false);
        
        main.transform.GetChild(1).Find("pages").GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("page").ToString() + "/" + PlayerPrefs.GetInt("pages_unlocked").ToString();
        updateItemsBase(1);

    }

    //
    // 420 Bizvhuel, dulce 

    //Mostramos los stats
    public void updateStats()
    {
        float allUnbox = Mathf.Max(PlayerPrefs.GetInt("stat_unb"),1);
        for (int i = 0; i < GameObject.Find("fields").transform.childCount; i++)
        {
            string stat = GameObject.Find("fields").transform.GetChild(i).gameObject.name;
            if (stat != "title")
            {
                string[] bsa = GameObject.Find("fields").transform.GetChild(i).GetComponent<Text>().text.Split(':');
                GameObject.Find("fields").transform.GetChild(i).GetComponent<Text>().text = bsa[0] + ": " + PlayerPrefs.GetInt("stat_" + stat, 0).ToString() + ((stat != "unb" && stat != "title") ? " %" + decimal.Round((decimal)((float)PlayerPrefs.GetInt("stat_" + stat, 0) / allUnbox)*100, 2) : "");
            }
        }
    }

    int crates = 0;
    //Mostrar publicidad cada 10 cajas
    public void doAd()
    {
        if (isMeelting)
        {
            contractQuality = 2;
            contracter.SetActive(true);
            isMeelting = false;
        }
        crates++;
        if (crates == Random.Range(5,10))
        {
            isTurboMode = false;
            if (Advertisement.IsReady())
            {
                Advertisement.Show();
            }
            crates = 0;
        }
    }

    public void doTurbo()
    {
        if (Advertisement.IsReady())
        {
            isTurboMode = true;
            Advertisement.Show();
        }
    }

    //Activamos la opcion para contrato
    public bool inContract = false;
    public int tempContract = 0;

    public void doMelting()
    {
        DataService.removeItem(collection);
        isMeelting = true;
        contractQuality = 2;
        contracter.SetActive(true);
        options.SetActive(false);
    }

    public void openContract()
    {
        if (isPlaying)
        {
            openMessage(2);
            return;
        }

        for (int i = 0; i < invAccess.transform.childCount; i++)
        {
            invAccess.transform.GetChild(i).GetComponent<itemBase>().selected = false;
        }

        inContract = true;
        contractQuality = tempContract; 
        contractList.Clear();
        contractList = new List<itemBase>();
        main.transform.GetChild(1).Find("cancelContract").gameObject.SetActive(true);
        options.SetActive(false);
        main.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        main.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = "0/10 Selected";
        //main.transform.GetChild(1).FindChild("pages").GetChild(1).GetComponent<Button>().interactable = false;
        updateItemsBase(1);
        sortButton.transform.parent.gameObject.SetActive(false);
        sortButton.SetActive(false);


    }

    //Cancelamos el contrato
    public void cancelContract()
    {
        inContract = false;
        main.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        contractQuality = -1;
        updateItemsBase(PlayerPrefs.GetInt("page",1));
        //sortButton.SetActive(true);
    }

    //Mandamos el contrato!
    public void sendContract()
    {
        inContract = false;
        contracter.SetActive(true);
        main.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        //sortButton.SetActive(true);
        //cancelContract();
    }

    //Activamos y desactivamos el modo idle
    public void doGameplay()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<Button>().interactable = false;

            ColorBlock cb = new ColorBlock();
            cb = transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<Button>().colors;
            cb.highlightedColor = new Color(252f/255, 103f/255, 63f/255);
            cb.normalColor = new Color(252f / 255, 103f / 255, 63f / 255);
            transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<Button>().colors = cb;

            transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1);

            transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<Button>().interactable = false;
        }
        else
        {
            isPlaying = false;
            transform.GetChild(0).GetChild(1).GetChild(4).GetComponent<Button>().interactable = true;
            transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<Button>().interactable = true;

            ColorBlock cb = new ColorBlock();
            cb = transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<Button>().colors;
            cb.highlightedColor = new Color(1,1,1);
            cb.normalColor = new Color(1, 1, 1);

            transform.GetChild(0).GetChild(1).GetChild(3).GetChild(0).GetComponent<Text>().color = new Color(0.1f, 0.1f, 0.1f);

            transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<Button>().colors = cb;
        }
        updateItemsBase(PlayerPrefs.GetInt("page"));
    }

    //Playtime, drops y leveleado de armas
    float nextLevel = 2;
    float nextCrate = -1;

    public string[] droplist;

    void Update()
    {
        if (isPlaying)
        {
            if (nextLevel < Time.fixedTime)
            {
                nextLevel = Time.fixedTime + Random.Range(2, 5);
                List<ItemDB> mit = new List<ItemDB>();
                foreach (ItemDB cit in items)
                {
                    if (cit.strange > 0)
                        mit.Add(cit);
                }
                if (mit.Count > 0)
                {
                    ItemDB tst = mit[Random.Range(0, mit.Count)];
                    foreach (ItemDB cit in items)
                    {
                        if (tst.name == cit.name && tst.strange == cit.strange && tst.state == cit.state)
                        {
                            cit.strange += Random.Range(1, 3);
                            DataService.update(cit);
                        }
                    }
                    mit.Clear();
                    updateItemsBase(PlayerPrefs.GetInt("page", 1));
                }
            }

            if (nextCrate == -1)
                nextCrate = Time.fixedTime + Random.Range(30, 50);

            if(nextCrate < Time.fixedTime)
            {
                if (DataService.GetItems().Count < 20 * (PlayerPrefs.GetInt("pages_unlocked", 1)))
                {
                    bool isDrop = Random.Range(0, 100) < 30;
                    DataService.addItem(isDrop ? GameObject.Find("contracts").GetComponent<contractDrop>().returnItem(1,true).id : droplist[Random.Range(0,droplist.Length)], isDrop ? Random.Range(3,5):0);
                    updateItemsBase(PlayerPrefs.GetInt("page"));
                }
                nextCrate = Time.fixedTime + Random.Range(30, 50);
            }
        }
    }

    public GameObject playingWarning;
    //Estamos jugando, por ende, no podemos abrir cajas!
    public void openMessage(int typ)
    {
        playingWarning.SetActive(true);
        if(typ==1)
            playingWarning.transform.GetChild(0).GetComponent<Text>().text = "You can't open a crate while you idle";
        else
            playingWarning.transform.GetChild(0).GetComponent<Text>().text = "You can't open a contract while you idle";
        noClick.SetActive(true);
    }

    //Activar o desactivar el auto-checkout
    public void setCheckOut(bool ck)
    {
        PlayerPrefs.SetInt("autocheck", ck ? 1 : 0);
    }

    //Eliminamos el objeto que vamos a eliminar cuando abrimos el panel de confirmación
    public void deleteObject()
    {
        if (toDelete.unusual)
        {
            Social.ReportProgress("grp.never_achievement", 100, reportScore);
        }
        DataService.removeItem(toDelete);
        updateItemsBase(PlayerPrefs.GetInt("page"));
    }

    //Activamos o desactivamos la opcion para que se muestre el panel de borrado
    public void setDelete(bool ck)
    {
        PlayerPrefs.SetInt("askdelete", ck ? 1 : 0);
    }

    //Mostrar la lista de items colleccionables con respecto a este item
    public Font font_to_use;
    public void showCollection()
    {
        collectionPanel.SetActive(true);

        GameObject crt = GameObject.Find("Crates");
        List<crateDrops.Drops> org = new List<crateDrops.Drops>();
        string cr_str = "";

        for (int i = 0; i < crt.transform.childCount-1; i++)
        {
            foreach (crateDrops.Drops item in crt.transform.GetChild(i).GetComponent<crateDrops>().drops)
            {
                if(cr_str == "" && item.id == collection.item_name)
                {
                    cr_str = crt.transform.GetChild(i).name;
                    org = crt.transform.GetChild(i).GetComponent<crateDrops>().drops;
                    collectionPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = StringTable.singleton.getTranslation(cr_str);
                }
            }
        }
        Transform pos = collectionPanel.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0);

        if (pos.childCount > 0)
            Destroy(pos.GetChild(0).gameObject);

        GameObject pnl = GameObject.Find("contentCollection");
        int fontSize = Mathf.RoundToInt(Screen.height / 45f);

        for (int i = 0; i < pnl.transform.childCount; i++)
        {
            Destroy(pnl.transform.GetChild(i).gameObject);
        }

        if (org.Count > 0)
        {
            int tpos = 0;
            foreach (crateDrops.Drops item in org)
            {
                bool it = false;
                foreach (ItemDB dit in items)
                {
                    if(dit.name == item.id)
                    {
                        it = true;
                        break;
                    }
                }
                GameObject txt = new GameObject(item.id);
                txt.transform.SetParent(pnl.transform, true);
                txt.AddComponent<Text>();
                txt.GetComponent<Text>().font = font_to_use;
                txt.GetComponent<Text>().text = (it?"►":"") + item.name;
                txt.GetComponent<Text>().fontSize = fontSize - 6;
                txt.GetComponent<Text>().color = StringTable.singleton.getColor(item.quality);
                txt.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                txt.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, fontSize);
                tpos += 1;
            }
            pnl.GetComponent<RectTransform>().sizeDelta = new Vector2(pnl.GetComponent<RectTransform>().sizeDelta.x, (fontSize) * (tpos + 1));
        }
    }

    public GameObject sortButton;
    //toggleamos el panel
    public void toggleSorting()
    {
        sortButton.SetActive(!sortButton.activeInHierarchy);
    }

    //Ordenamos todos los elementos
    public void orderElements(string how)
    {
        DataService.sort(how);
        updateItemsBase(PlayerPrefs.GetInt("page"));
    }

}
