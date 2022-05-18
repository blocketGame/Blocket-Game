using Assets.Scripts.Entities.BlockEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenCalculation : MonoBehaviour
{
    public static OvenCalculation Singleton;
    private void Awake() => Singleton=this;

    [HideInInspector] //Just to save the oven state in Runtime
    public List<OvenItemStatus> OvenItemStatuses = new List<OvenItemStatus>();

    // Update is called once per frame
    void Update()
    {
        foreach(OvenItemStatus ovenItemStatus in OvenItemStatuses)
        {
            if (ovenItemStatus.currentItemId.ItemID != 0)
            {
                ovenItemStatus.meltedProcess -= ItemAssets.Singleton.Ovens.Find(x => x.itemId == ovenItemStatus.ovenItemId).meltingSpeed*Time.deltaTime;
                if (ovenItemStatus.meltedProcess <= 0)
                {
                    ovenItemStatus.readyItem.ItemID = ItemAssets.Singleton.GetItemFromItemID(ovenItemStatus.currentItemId.ItemID).meltedItemVersion;
                    ovenItemStatus.currentItemId.ItemCount--;
                    if(ovenItemStatus.currentItemId.ItemCount<=0)
                    ovenItemStatus.currentItemId.ItemID = 0; //setting current Item to 0
                }
            }
        }
    }

    public void AddOvenToWorld(uint ovenId,Vector2 ovenposition)
    {
        OvenItem oi = ItemAssets.Singleton.Ovens.Find(x => x.itemId == ovenId);
        OvenItemStatus ovenItemStatus = new OvenItemStatus();
        ovenItemStatus.ovenItemId = ovenId;
        ovenItemStatus.ovenPosition = ovenposition;
    }
    public void AddOvenToWorld(OvenItemStatus oi)
    {
        OvenItemStatuses.Add(oi);
    }

    public void RemoveOvenFromWorld(uint ovenId, Vector2 ovenposition)
    {
        OvenItemStatuses.Remove(OvenItemStatuses.Find(x=>x.ovenPosition==ovenposition));
    }
}


[Serializable]
public class OvenItem 
{
    public uint itemId;
    public float meltingSpeed;
}