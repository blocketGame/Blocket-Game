using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVariables : MonoBehaviour
{
	#region Static Resources
	public UIInventory uIInventory;
	public HealthScript healthScript;
	public GameObject playerModel, playerLogic;
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
			uIInventory.heartStat.text = $"{_health}/{_maxHealth}";
			healthScript.CurrentHealth = _health;
		} 
	}
	public ushort MaxHealth {
		get => _maxHealth;
		set 
		{ 
			_maxHealth = value;
			uIInventory.heartStat.text = $"{_health}/{_maxHealth}";
			healthScript.maxHealth = _maxHealth;
			healthScript.InitiateSprites();
		}
	}
	public ushort MaxArmor {
		get => _maxArmor;
		set { _maxArmor = value;	uIInventory.shieldStat.text = $"{_armor}/{_maxArmor}";}
	}
	public ushort Armor {
		get => _armor;
		set { _armor = value; uIInventory.shieldStat.text = $"{_armor}/{_maxArmor}"; }
	}
	public ushort Strength {
		get => _strength;
		set { _strength = value; uIInventory.swordStat.text = $"{_strength}/{_maxStrength}"; }
	}
	public ushort MaxStrength {
		get => _maxStrength;
		set { _maxStrength = value; uIInventory.swordStat.text = $"{_strength}/{_maxStrength}"; }
	}
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

	public void Awake1()
	{
		healthScript?.InitiateSprites();
		MaxHealth = 40;
		MaxArmor = 40;
		MaxStrength = 40;
		Health = MaxHealth;
		Armor = 1;
		Strength = 1;
	}
}
