using System;
using System.Collections.Generic;

using UnityEngine;

using static UnityEditor.Progress;

/// <summary>
/// Handles all items in Game
/// </summary>
public class ItemAssets : MonoBehaviour{
	public static ItemAssets Singleton { get; private set; }

	/// <summary>Block-Ids: 0 - 999</summary>
	[Header("The id must be from 0 to 999!")]
	public List<BlockItem> BlockItemsInGame = new List<BlockItem>();

	/// <summary>ToolItem-Ids: 1000 - 1999</summary>
	[Header("The id must be from 1000 to 1999!")]
	public List<ToolItem> ToolItemsInGame = new List<ToolItem>();

	/// <summary>EquipableItem-Ids: 2000 - 2999</summary>
	[Header("The id must be from 2000 to 2999!")]
	public List<EquipableItem> EquipableItemsInGame = new List<EquipableItem>();

	/// <summary>UsableItem-Ids: 3000 - 3999</summary>
	[Header("The id must be from 3000 to 3999!")]
	public List<UseAbleItem> UseableItemsInGame = new List<UseAbleItem>();

	/// <summary>CommonItem-Ids: 4000 - 4999</summary>
	[Header("The id must be from 4000 to 4999!")]
	public List<CommonItem> CommonItems = new List<CommonItem>();

	/// <summary>Weapon-Ids: 5000 - 5999</summary>
	[Header("The id must be from 5000 to 5999!")]
	public List<WeaponItem> WeaponItems = new List<WeaponItem>();

	/// <summary>Projectile-Ids: 6000 - 6999</summary>
	[Header("The id must be from 6000 to 6999!")]
	public List<Projectile> ProjectileItems = new List<Projectile>();

	//TODO: Move somewhere else
	public List<CraftingRecipe> Recipes = new List<CraftingRecipe>();
	public List<CraftingStation> CraftingStations = new List<CraftingStation>();
	public List<EnemySO> Enemies = new List<EnemySO>();
	public List<Buff> Buffs = new List<Buff>();
	public List<OvenItem> Ovens = new List<OvenItem>();

	
	public Sprite InventoryCursor;
	public Sprite MiningCursor;
	public Sprite AttackingCursor;

	public Sprite nullSprite;

	private void Awake() { 
		Singleton = this;
		MapItems();
	}

	/// <summary>
	/// Map all Items
	/// </summary>
    public void MapItems() {
		if(DebugVariables.ItemCkeckStat)
			Debug.Log("Checking Items");

		foreach(Item item in BlockItemsInGame)
			if(item.id == 0 || item.id > 999)
				throw new NotSupportedException();
		foreach(Item item in ToolItemsInGame)
			if(item.id < 1000 || item.id > 1999)
				throw new NotSupportedException();
		foreach(Item item in EquipableItemsInGame)
			if(item.id < 2000 || item.id > 2999)
				throw new NotSupportedException();
		foreach(Item item in UseableItemsInGame)
			if(item.id < 3000 || item.id > 3999)
				throw new NotSupportedException();
		foreach(Item item in CommonItems)
			if(item.id < 4000 || item.id > 4999)
				throw new NotSupportedException();
		foreach(Item item in WeaponItems)
			if(item.id < 5000 || item.id > 5999)
				throw new NotSupportedException();
		foreach(Item item in ProjectileItems)
			if(item.id < 6000 || item.id > 6999)
				throw new NotSupportedException();
		if(DebugVariables.ItemCkeckStat)
			Debug.Log("Items checked");
	}

    /// <summary>
    /// Returns a Sprite from Item-ID
    /// </summary>
    /// <param name="itemId"></param>	
    /// <returns></returns>
    public Sprite GetSpriteFromItemID(uint itemId) => GetItemFromItemID(itemId, null)?.itemImage ?? nullSprite;
	
	/// <summary>
	/// If Type is null => search through all itemtypes
	/// </summary>
	/// <param name="itemId">Id of item</param>
	/// <param name="type">Itemclass</param>
	/// <returns>Iteminstance</returns>
	public Item GetItemFromItemID(uint itemId, Type type){
			//Always ckeck null first! Or => NullRefExc
			if(type == null || BlockItemsInGame.GetType().Name == type.Name)
				foreach(Item item in BlockItemsInGame)
					if(item.id == itemId)
						return item;
			if(type == null || ToolItemsInGame.GetType().Name == type.Name)
				foreach(Item item in ToolItemsInGame)
					if(item.id == itemId)
						return item;
			if(type == null || EquipableItemsInGame.GetType().Name == type.Name)
				foreach(Item item in EquipableItemsInGame)
					if(item.id == itemId)
						return item;
			if(type == null || UseableItemsInGame.GetType().Name == type.Name)
				foreach(Item item in UseableItemsInGame)
					if(item.id == itemId)
						return item;
			if(type == null || CommonItems.GetType().Name == type.Name)
				foreach(Item item in CommonItems)
					if(item.id == itemId)
						return item;
			if(type == null || WeaponItems.GetType().Name == type.Name)
				foreach(Item item in WeaponItems)
					if(item.id == itemId)
						return item;
			if(type == null || ProjectileItems.GetType().Name == type.Name)
				foreach(Item item in ProjectileItems)
					if(item.id == itemId)
						return item;
		Debug.LogWarning($"Item not found: {itemId}");
		return null;
	}

	public Item GetItemFromItemID(uint itemId) {
		if (itemId == 0)
			return null;
		if(itemId > 0 && itemId <= 999)
			foreach (Item item in BlockItemsInGame)
				if (item.id == itemId)
					return item;
		if(itemId >= 1000 && itemId <= 1999)
			foreach (Item item in ToolItemsInGame)
				if (item.id == itemId)
					return item;
		if(itemId >= 2000 && itemId <= 2999)
			foreach (Item item in EquipableItemsInGame)
				if (item.id == itemId)
					return item;
		if(itemId >= 3000 && itemId <= 3999)
			foreach (Item item in UseableItemsInGame)
				if (item.id == itemId)
					return item;
		if(itemId >= 4000 && itemId <= 4999)
			foreach (Item item in CommonItems)
				if (item.id == itemId)
					return item;
		if(itemId >= 5000 && itemId <= 5999)
			foreach (Item item in WeaponItems)
				if (item.id == itemId)
					return item;
		if(itemId >= 6000 && itemId <= 6999)
			foreach (Item item in ProjectileItems)
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
