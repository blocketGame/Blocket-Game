using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for Saving and Storing Data
/// </summary>
[Serializable]
public abstract class Profile{
	public int profileHash;
	public readonly string name;

	public Profile(string name)
    {
		this.name = name;
		profileHash = new System.Random().Next();
	}
}

/// <summary>
/// UNDONE
/// </summary>
public class WorldProfile : Profile
{
	public WorldProfile(string name) : base(name)
	{

	}
}

public class SettingsProfile : Profile{

	public SettingsProfile(string name) : base(name)
	{

	}
}

/// <summary>
/// Storing player data
/// </summary>
public class PlayerProfile : Profile
{
	#region Inventory
	public List<SavableItem> InventoryItems { get; set; } = new List<SavableItem>();
	public List<SavableItem> AccessoiresItems { get; set; } = new List<SavableItem>();
	public List<SavableItem> ArmorItems { get; set; } = new List<SavableItem>();
	#endregion
	#region PlayerVariables
	public ushort health, armor, maxHealth, maxArmor;
	public uint healthGained, healthLost;
	#endregion

	public PlayerProfile(string name): base(name)
    {

    }

	[Serializable]
	public struct SavableItem
	{
		public uint itemId;
		public ushort count;

		public SavableItem(uint itemId, ushort count)
		{
			this.itemId = itemId;
			this.count = count;
		}
	}
}
