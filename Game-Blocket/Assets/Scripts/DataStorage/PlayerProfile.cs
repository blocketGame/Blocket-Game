using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Storing player data
/// </summary>
public class PlayerProfile : Profile {
	#region Inventory
	/// <summary>
	/// TODO capcity not hardcoded
	/// </summary>
	public List<SaveAbleItem> inventoryItems = new List<SaveAbleItem>(40);
	public List<SaveAbleItem> accessoiresItems = new List<SaveAbleItem>(7);
	public List<SaveAbleItem> armorItems = new List<SaveAbleItem>(3);
	#endregion
	#region PlayerVariables
	public ushort health, armor, maxHealth, maxArmor;
	public uint healthGained, healthLost;
	#endregion

	public PlayerProfile(string name, int? profileHash = null) : base(name, profileHash) {

	}

	[Serializable]
	public struct SaveAbleItem {
		public uint itemId;
		public ushort count;

		public SaveAbleItem(uint itemId, ushort count) {
			this.itemId = itemId;
			this.count = count;
		}
	}

	#region PlayerProfiles
	public static void SavePlayerProfile() {
		SaveComponentsInPlayerProfile(GameManager.PlayerProfileNow);
	}

	/// <summary>
	/// Overrides the profile for saving
	/// </summary>
	/// <returns></returns>
	public static void SaveComponentsInPlayerProfile(PlayerProfile pfN) {
		Inventory inv = Inventory.Singleton;
		if (pfN == null)
			throw new NullReferenceException("Playerprofile is null!");
		if (inv != null) {
			//Inventory
			for (int i = 0; i < inv.InvSlots.Count; i++)
				pfN.inventoryItems.Add(new SaveAbleItem(inv.InvSlots[i].ItemID, inv.InvSlots[i]?.ItemCount ?? 0));
			//Armor Slots
			for (int i = 0; i < inv.ArmorSlots.Count; i++)
				pfN.armorItems.Add(new SaveAbleItem(inv.ArmorSlots[i].ItemID, inv.ArmorSlots[i]?.ItemCount ?? 0));
			//Accessoires
			for (int i = 0; i < inv.AccessoiresSlots.Count; i++)
				pfN.accessoiresItems.Add(new SaveAbleItem(inv.AccessoiresSlots[i].ItemID, inv.AccessoiresSlots[i]?.ItemCount ?? 0));
		}
		if (PlayerVariables.Singleton != null) {
			//PlayerVariables
			pfN.health = PlayerVariables.Singleton.Health;
			pfN.armor = PlayerVariables.Singleton.Armor;
			pfN.healthGained = PlayerVariables.Singleton.healthGained;
			pfN.healthLost = PlayerVariables.Singleton.healthLost;
			pfN.maxHealth = PlayerVariables.Singleton.MaxHealth;
		} else
			throw new NullReferenceException();
	}

	/// <summary>
	/// Overrides game components => matching profile
	/// </summary>
	/// <param name="profile"></param>
	public static void LoadComponentsFromPlayerProfile(PlayerProfile profile) {
		if (profile != null && GameManager.PlayerProfileNow == null)
			GameManager.PlayerProfileNow = profile;
		if (GameManager.PlayerProfileNow == null)
			throw new NullReferenceException("PlayerProfile is null!");
		Inventory inventory = Inventory.Singleton;
		if (inventory != null) {
			//Inventory
			for (int i = 0; i < inventory.InvSlots.Count; i++) {
				if (profile.inventoryItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.inventoryItems[i];
				inventory.InvSlots[i].ItemID = itemNow.itemId;
				inventory.InvSlots[i].ItemCount = itemNow.count;
			}

			//Armor Slots
			for (int i = 0; i < inventory.ArmorSlots.Count; i++) {
				if (profile.armorItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.armorItems[i];
				inventory.ArmorSlots[i].ItemID = itemNow.itemId;
				inventory.ArmorSlots[i].ItemCount = itemNow.count;
			}
			//Accessoires
			for (int i = 0; i < inventory.AccessoiresSlots.Count; i++) {
				if (profile.accessoiresItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.accessoiresItems[i];
				inventory.AccessoiresSlots[i].ItemID = itemNow.itemId;
				inventory.AccessoiresSlots[i].ItemCount = itemNow.count;
			}
		} else
			throw new NullReferenceException();
	}

	#endregion

}

