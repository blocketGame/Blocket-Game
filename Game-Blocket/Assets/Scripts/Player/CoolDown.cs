using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolDown : MonoBehaviour
{
    public GameObject greenBar;
    public Image weaponImage;
    
    public float Timer => ItemUsageHandler.Singleton.timer/((Inventory.Singleton.SelectedItemObj ?? new WeaponItem()) as WeaponItem)?.CoolDownTime ?? 1;

    // Update is called once per frame
    void Update()
    {
        //[TODO] - Bugfix relational scaling while moving weapon in hand
        weaponImage.sprite = ((Inventory.Singleton.SelectedItemObj ?? new WeaponItem()) as WeaponItem)?.itemImage ?? null;//Paste DebugImg instead of null
        greenBar.transform.localScale = new Vector3(Timer, greenBar.transform.localScale.y, greenBar.transform.localScale.z);
    }
}
