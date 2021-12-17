
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all items in Game
/// </summary>
public class ItemAssets : MonoBehaviour
{
    public List<BlockItem> BlockItemsInGame = new List<BlockItem>();
    public List<ToolItem> ToolItemsInGame = new List<ToolItem>();
    public List<EquipableItem> EquipableItemsInGame = new List<EquipableItem>();
    public List<UseAbleItem> UseableItemsInGame = new List<UseAbleItem>();
    public List<Structure> Structures = new List<Structure>();
    public List<Enemy> Enemies = new List<Enemy>();

	/*
    private void Awake()
    {
        foreach (Structure s in Structures) {
            s.ReadStructureFromTilemap();
        }
    }*/

	private void Awake() {
        GlobalVariables.ItemAssets = this;
	}

	/// <summary>
	/// Returns a Sprite from Item-ID
	/// </summary>
	/// <param name="itemId"></param>
	/// <returns></returns>
	public Sprite GetSpriteFromItemID(uint itemId) {
        return GetItemFromItemID(itemId)?.itemImage;
    }

    public Item GetItemFromItemID(uint itemId) {
        foreach (Item item in BlockItemsInGame)
            if (item.id == itemId)
                return item;
        foreach (Item item in ToolItemsInGame)
            if (item.id == itemId)
                return item;
        foreach (Item item in EquipableItemsInGame)
            if (item.id == itemId)
                return item;
        foreach (Item item in UseableItemsInGame)
            if (item.id == itemId)
                return item;
        return null;
    }
}
