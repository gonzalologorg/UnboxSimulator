using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class purchasableItem : MonoBehaviour {

    public float price;
    public string unique;
    public string item_name;

    private Button.ButtonClickedEvent doClick = new Button.ButtonClickedEvent();

    void Start()
    {
        GetComponent<Button>().onClick = doClick;
        doClick.AddListener(openStore);
    }

    private void openStore()
    {
        GameObject.Find("picksound").GetComponent<AudioSource>().Play();
        PlayerPrefs.SetString("act_obj", unique);
        PlayerPrefs.SetFloat("act_price", price);

        if (price != -1)
        {
            if (PlayerPrefs.GetInt("autocheck", 0) == 0)
            {
                crateComposer.singleton.checkingOut.transform.Find("main").Find("mini").Find("top").Find("ITEM_PRICE").GetComponent<Text>().text = crateComposer.singleton.isFree ? "FREE" : "$" + price.ToString();
                crateComposer.singleton.checkingOut.transform.Find("main").Find("ITEM_PRICE_B").GetComponent<Text>().text = crateComposer.singleton.isFree ? "FREE" : "$" + price.ToString();
                crateComposer.singleton.checkingOut.SetActive(true);
            }
            else
            {
                crateComposer.singleton.noClick.SetActive(true);
                crateComposer.singleton.checkOutPanel.SetActive(true);
                crateComposer.singleton.doPurchase();
            }
        }
        else
        {
            crateComposer.singleton.checkingOut.SetActive(true);
            crateComposer.singleton.checkingOut.transform.Find("main").Find("mini").Find("top").Find("ITEM_PRICE").GetComponent<Text>().text = "Video Ad";
            crateComposer.singleton.checkingOut.transform.Find("main").Find("ITEM_PRICE_B").GetComponent<Text>().text = "Video Ad";
        }
        crateComposer.singleton.checkingOut.transform.Find("main").Find("mini").Find("top").Find("ITEM_NAME").GetComponent<Text>().text = item_name;
    }
}
