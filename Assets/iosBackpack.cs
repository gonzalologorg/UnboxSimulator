using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class iosBackpack : MonoBehaviour {

    public Text text;
#if UNITY_IOS || UNITY_EDITOR
    void Start()
    {
        text.text = "$5.00";
        GetComponent<purchasableItem>().price = 5;
    }
#endif

}
