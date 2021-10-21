using System.Collections;
using System.Collections.Generic;

using MLAPI.NetworkVariable;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerVariables : MonoBehaviour { 

	public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings {
		WritePermission = NetworkVariablePermission.Everyone,
		ReadPermission = NetworkVariablePermission.Everyone
	});

	#region Static Resources
	public UIInventory uIInventory;
	#endregion

	#region Dyniamic Variables
	private ushort _health, _maxHealth, _maxArmor, _maxStrength, _armor, _strength;
	#endregion

	#region Statistics Variables
	public uint healthGained, healthLost;
	#endregion

	#region Properties
	public ushort Health { get => _health; 
		set { _health = value;uIInventory.heartStat.text = $"{_health}/{_maxHealth}";} 
	}
	public ushort MaxHealth {
		get => _maxHealth;
		set { _maxHealth = value; uIInventory.heartStat.text = $"{_health}/{_maxHealth}";}
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

	public void Awake() {
		Position.OnValueChanged += (x, y) => { transform.position = y; };

		MaxHealth = 40;
		MaxArmor = 40;
		MaxStrength = 40;
		Health = MaxHealth;
		Armor = 1;
		Strength = 1; 
	}


}
