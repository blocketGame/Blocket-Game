using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

/// <summary>
/// Basic item Class<br></br>
/// </summary>
[Serializable]
public abstract class Item{
	public string name;
	public uint id;
	public string description;
	public ItemType itemType;
	public Sprite itemImage;

	/// <summary>How much of the Item can be hold.</summary>
	public enum ItemType {
		SINGLE, STACKABLE
	}

	/// <summary>
	/// Euqals...
	/// </summary>
	/// <param name="obj">Other Object...</param>
	/// <returns><see langword="true"/> if the <see cref="Item.id"/> is the same</returns>
	public override bool Equals(object obj) {
		if(obj is Item)
			if(this.id == (obj as Item).id)
				return true;
		return false;
	}

	public override int GetHashCode() {
		return base.GetHashCode();
	}
}

[Serializable]
public class BlockItem : Item {
	public uint blockId;
}

[Serializable]
public class ToolItem : Item {
	public ushort durability, damage;
	public ToolType toolType;

	public enum ToolType {
		SWORD, SHOVEL, AXE, BOW, PICKAXE
	}
}

[Serializable]
public class EquipableItem : Item {
	//TODO:
	
}

[Serializable]
public class UseAbleItem : Item{

}