using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for Saving and Storing Data
/// </summary>
[Serializable]
public class Profile{
	public int profileHash;

	public readonly string name;

	#region Inventory
	public List<SavableItem> InventoryItems { get; set; } = new List<SavableItem>();
	public List<SavableItem> AccessoiresItems { get; set; } = new List<SavableItem>();
	public List<SavableItem> ArmorItems { get; set; } = new List<SavableItem>();
	#endregion
	#region PlayerVariables
	public ushort health, armor, maxHealth, maxArmor;
	public uint healthGained, healthLost;
	#endregion

	public Profile(string name){
		profileHash = new System.Random().Next();
		this.name = name;
	}

	[Serializable]
	public struct SavableItem{
		public uint itemId;
		public ushort count;

		public SavableItem(uint itemId, ushort count){
			this.itemId = itemId;
			this.count = count;
		}
	}
}
