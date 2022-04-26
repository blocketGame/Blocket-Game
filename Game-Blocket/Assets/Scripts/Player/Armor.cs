using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : MonoBehaviour
{
    public static Armor Singleton { get; private set; }

    public List<UIInventorySlot> uIInventorySlots;

    public Sprite HelmetSprite => GetSpriteBySlotId(0);
    public Sprite BreastPlateSprite => GetSpriteBySlotId(1);
    public Sprite PantsSprite => GetSpriteBySlotId(2);


    public Sprite GetSpriteBySlotId(int slotid)
    {
        return ItemAssets.Singleton.GetItemFromItemID(uIInventorySlots[slotid].ItemID).itemImage;
    }

    public float DefenseArmor
    {
        get
        {
            float armor = 0;
            foreach (UIInventorySlot slot in uIInventorySlots)
            {
                if (((EquipableItem)ItemAssets.Singleton.GetItemFromItemID(slot.ItemID)) != null)
                {
                    armor += ((EquipableItem)ItemAssets.Singleton.GetItemFromItemID(slot.ItemID)).defenseStat;
                }
            }
            return armor;
        }
    }

    public void Start() => Singleton = this;
}
