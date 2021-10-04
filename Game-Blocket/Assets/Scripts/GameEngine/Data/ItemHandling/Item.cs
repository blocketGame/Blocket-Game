using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Item{
	public int id;
	public string name, description;
	public Sprite itemImage;
}

[Serializable]
public class BlockItem : Item {
	public int blockId;
}

[Serializable]
public class ToolItem : Item {
	public short durability, damage;
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
public class UseableItem : Item{

}