using System;

using UnityEngine;

public class PlayerVariables : MonoBehaviour
{

	public static Gamemode Gamemode { get => gamemode; set {	
		switch(value){
				case Gamemode.SURVIVAL: 
					
				break;
				case Gamemode.CREATIVE: 
					
				break;
        }
		gamemode = value;
	} }
	private static Gamemode gamemode;


	#region Static Resources
	public GameObject playerModel, playerLogic;
	public SpriteRenderer holdingItemPlaceholder;
	#endregion

	#region Dyniamic Variables
	private ushort _health, _maxHealth, _maxArmor, _maxStrength, _armor, _strength;
	public Inventory inventory;
	#endregion

	#region Statistics Variables
	[HideInInspector]
	public uint healthGained, healthLost;
	#endregion

	#region Properties

	public ushort Health { get => _health; 
		set 
		{ 
			_health = value;
			if(GlobalVariables.UIInventory?.heartStat != null)
				GlobalVariables.UIInventory.heartStat.text = $"{_health}/{_maxHealth}";
			GlobalVariables.PlayerHealth.CurrentHealth = _health;
		} 
	}
	public ushort MaxHealth {
		get => _maxHealth;
		set 
		{ 
			_maxHealth = value;
			if (GlobalVariables.UIInventory?.heartStat != null)
				GlobalVariables.UIInventory.heartStat.text = $"{_health}/{_maxHealth}";
			GlobalVariables.PlayerHealth.maxHealth = _maxHealth;
			GlobalVariables.PlayerHealth.InitiateSprites();
		}
	}
	public ushort MaxArmor {
		get => _maxArmor;
		set { _maxArmor = value; GlobalVariables.UIInventory.shieldStat.text = $"{_armor}/{_maxArmor}";}
	}
	public ushort Armor {
		get => _armor;
		set { _armor = value; GlobalVariables.UIInventory.shieldStat.text = $"{_armor}/{_maxArmor}"; }
	}
	public ushort Strength {
		get => _strength;
		set { _strength = value; GlobalVariables.UIInventory.swordStat.text = $"{_strength}/{_maxStrength}"; }
	}
	public ushort MaxStrength {
		get => _maxStrength;
		set { _maxStrength = value; GlobalVariables.UIInventory.swordStat.text = $"{_strength}/{_maxStrength}"; }
	}
	private CharacterRace race = CharacterRace.HUMAN;
    public CharacterRace Race { get=> race; set => race = value; } 
	#endregion

	/// <summary>For configuring the health of the player</summary>
	/// <param name="add">If you want to loose health: make it below 0</param>
	public void AddHealth(byte add) {
		if(add == 0)
			return;
		if(add > 0)
			healthGained += add;
		else
			healthLost -= add;
		Health += add;
		if(Health <= 0)
			Death();
	}

	public void Death() {
		//TODO
	}

	public void ReloadItemInHand(){
		holdingItemPlaceholder.sprite = GlobalVariables.Inventory.SelectedItemObj?.itemImage;
	}

	public void Init(){
		MaxHealth = GameManager.PlayerProfileNow.maxHealth;
		Health = GameManager.PlayerProfileNow.health != 0 ? GameManager.PlayerProfileNow.health : MaxHealth;
		Armor = GameManager.PlayerProfileNow.armor;
		healthGained = GameManager.PlayerProfileNow.healthGained;
		healthLost = GameManager.PlayerProfileNow.healthLost;
	}

	
}
public enum Gamemode{
	SURVIVAL, CREATIVE
}

public enum CharacterRace
{
	MAGICIAN, HUMAN
}