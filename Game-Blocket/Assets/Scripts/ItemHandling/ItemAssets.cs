using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    public List<BlockItem> BlockItemsInGame = new List<BlockItem>();
    public List<ToolItem> ToolItemsInGame = new List<ToolItem>();
    public List<EquipableItem> EquipableItemsInGame = new List<EquipableItem>();
    public List<UseAbleItem> UseableItemsInGame = new List<UseAbleItem>();
    public List<Structure> Structures = new List<Structure>();

    /*
    private void Awake()
    {
        foreach (Structure s in Structures) {
            s.ReadStructureFromTilemap();
        }
    }*/

    public Sprite GetSpriteFromItemID(int itemId) {
        foreach(Item item in BlockItemsInGame)
            if(item.id == itemId)
                return item.itemImage;
        foreach(Item item in ToolItemsInGame)
            if(item.id == itemId)
                return item.itemImage;
        foreach(Item item in EquipableItemsInGame)
            if(item.id == itemId)
                return item.itemImage;
        foreach(Item item in UseableItemsInGame)
            if(item.id == itemId)
                return item.itemImage;
        return null;
	}
}
