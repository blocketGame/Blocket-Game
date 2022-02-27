using System;

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

	protected Action onMainInteractionKey;
	protected Action onSideInteractionKey;
	protected Action on2SideInteractionKey;

	public Action OnMainInteractionKey => onMainInteractionKey ?? GlobalVariables.DoNothing;
	public Action OnSideInteractionKey => onSideInteractionKey ?? GlobalVariables.DoNothing;

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

	public override int GetHashCode() => base.GetHashCode();
	
}

[Serializable]
public class BlockItem : Item {
	public byte blockId;

	public BlockItem(){
		onSideInteractionKey = () => GlobalVariables.Interaction.BlockPlace();
	}
}

[Serializable]
public class ToolItem : Item {
	public ushort durability, damage;
	public byte toolHardness;
	public ToolType toolType;

	public enum ToolType {
		MEELE, RANGE, SHOVEL, AXE, PICKAXE, DEFAULT
	}
}

[System.Serializable]
public class MeeleItem : ToolItem{
	
}

[Serializable]
public class EquipableItem : Item {
	//TODO:
	
}

[Serializable]
public class UseAbleItem : Item{

}

[Serializable]
public class CommonItem : Item{

}
