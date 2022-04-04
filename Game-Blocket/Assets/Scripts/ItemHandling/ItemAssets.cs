using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Handles all items in Game
/// </summary>
public class ItemAssets : MonoBehaviour{
	public static ItemAssets Singleton { get; private set; }

	public List<BlockItem> BlockItemsInGame = new List<BlockItem>();
	public List<ToolItem> ToolItemsInGame = new List<ToolItem>();
	public List<EquipableItem> EquipableItemsInGame = new List<EquipableItem>();
	public List<UseAbleItem> UseableItemsInGame = new List<UseAbleItem>();
	public List<CommonItem> CommonItems = new List<CommonItem>();
	public List<WeaponItem> WeaponItems = new List<WeaponItem>();
	public List<Projectile> Projectiles = new List<Projectile>();

	//TODO: Move somewhere else
	public List<CraftingRecipe> Recipes = new List<CraftingRecipe>();
	public List<CraftingStation> CraftingStations = new List<CraftingStation>();
	public List<EnemySO> Enemies = new List<EnemySO>();
	
	public Sprite InventoryCursor;
	public Sprite MiningCursor;
	public Sprite AttackingCursor;

	public Sprite nullSprite;

	private void Awake() => Singleton = this;


	/// <summary>
	/// Returns a Sprite from Item-ID
	/// </summary>
	/// <param name="itemId"></param>	
	/// <returns></returns>
	public Sprite GetSpriteFromItemID(uint itemId) => GetItemFromItemID(itemId)?.itemImage ?? nullSprite;
	

	public Item GetItemFromItemID(uint itemId) {
		if (itemId == 0)
			return null;
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
		foreach (Item item in CommonItems)
			if (item.id == itemId)
				return item;
		foreach (Item item in WeaponItems)
			if (item.id == itemId)
				return item;
		foreach (Item item in Projectiles)
			if (item.id == itemId)
				return item;
		Debug.LogWarning($"Item not found: {itemId}");
		return null;
	}

	public uint GetItemIdFromBlockID(byte blockID)
	{
		foreach (BlockItem item in BlockItemsInGame)
			if (item.blockId == blockID)
				return item.id;
		return 0;
	}

	public byte GetBlockIdFromItemID(uint itemId){
		foreach (BlockItem item in BlockItemsInGame)
			if (item.id == itemId)
				return item.blockId;
		return 0;
	}
}
