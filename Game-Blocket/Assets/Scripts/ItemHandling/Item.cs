using System;
using System.Collections.Generic;
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
	public String swingingAnimation;

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
		onSideInteractionKey = () => PlayerInteraction.Singleton.BlockPlace();
	}
}

[Serializable]
public class ToolItem : Item {
	public ushort toolPower, damage;
	public float ToolPower => (100 + this.toolPower) / 100;
	public ToolType toolType;

	[Serializable]
	public enum ToolType {
		NONE, SHOVEL, AXE, PICKAXE
	}
}

[Serializable]
public class WeaponItem : Item
{
	//Animations for each kombo
	public List<String> swingingAnimations;
	public ushort durability, damage;
	public bool dmgOnColliderHit;
	public bool holdShooting;

	[SerializeField]
	private float coolDownTime;
	public float CoolDownTime {
		get => float.IsInfinity(coolDownTime) ? 0 : coolDownTime;
		set => coolDownTime = value;
	}
	

	[Header("Ranged-Settings")]
	//Projectile (0 => no projectile)
	public uint projectile;
	public int maxHeight, maxDistance;
	[Header("Custom")]
	public CustomWeaponBehaviour behaviour;

	public WeaponType weaponType;
	public enum WeaponType
	{
		MELEE , RANGE , MAGE , BENDER
	}
}

[Serializable]
public class Projectile : Item
{
	public ushort durability, damage;
	[Header("Flying-Behaviour")]
	//Realtive to the player
	public Vector2 spawningPos;
	public Vector3 SpawningPos3 { get => spawningPos;}
	public float flyingSpeed;
	[Header("Hit-Attributes")]
	//0 => Destroy on Block Hit
	public float bounciness;
	public float lightEmission;
	public bool goThroughBlocks, pierce;
	//In percent to actual Velocity (9,8..)
	//=> 100% - normal
	//=> 0% - no Velocity
	//=> 200% fall really fast
	public float gravityScale;

}

[Serializable]
public class EquipableItem : Item {
	public float defenseStat;
	public AccessoryInfliction inflictedStat;
	public float value;
	public EquipableType type;

	public void InflictStat(bool direction)
    {
        switch (inflictedStat)
        {
			case AccessoryInfliction.RAISESPEEDBY: {
					if(direction)
					Movement.Singleton.MovementSpeed += value;
					else
					Movement.Singleton.MovementSpeed -= value;
					break;
				}
			case AccessoryInfliction.RAISEJUMPBY:
				{
					if (direction)
						Movement.Singleton.JumpForce += value;
					else
						Movement.Singleton.JumpForce -= value;
					break;
				}
		}
    }

	public enum AccessoryInfliction
    {
		RAISESPEEDBY, RAISEJUMPBY
    }

	public enum EquipableType
    {
		None, Armor , Accessory
    }
	
}

[Serializable]
public class UseAbleItem : Item{

}

[Serializable]
public class CommonItem : Item{

}
