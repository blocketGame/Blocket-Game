using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolDown : MonoBehaviour
{
    public GameObject greenBar;
    public Image weaponImage;
    public float Timer => ItemUsageHandler.Singleton.timer/((WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem()).coolDownTime;


    // Update is called once per frame
    void Update()
    {
        //[TODO] - Bugfix relational scaling while moving weapon in hand
        weaponImage.sprite = ((WeaponItem)(ItemAssets.Singleton.GetItemFromItemID(Inventory.Singleton.SelectedItemId)) ?? new WeaponItem()).itemImage;
        greenBar.transform.localScale = new Vector3(Timer, greenBar.transform.localScale.y, greenBar.transform.localScale.z);
    }
}
