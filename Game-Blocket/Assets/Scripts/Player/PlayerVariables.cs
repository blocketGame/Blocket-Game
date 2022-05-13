using System;

using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerVariables : MonoBehaviour{
	public static PlayerVariables Singleton { get; private set; }

	public BoxCollider2D BoxCollider2D => GetComponentInChildren<BoxCollider2D>();

	public static Dimension Dimension { get; set; }

	public static Gamemode Gamemode { get => _gamemode; set {
		switch(value){
				case Gamemode.SURVIVAL: 
					
				break;
				case Gamemode.CREATIVE: 
					
				break;
				case Gamemode.ADVENTURE:

				break;
			}
			_gamemode = value;
		}
	}
	private static Gamemode _gamemode;

    private void Awake() => Singleton = this;

    public void Update() {
		if(GameManager.State != GameState.INGAME)
			return;
		if(Input.GetKeyDown(KeyCode.F3)){
			string cmd = Gamemode == Gamemode.SURVIVAL ? "1" : "0";
			ConsoleHandler.Handle($"/gamemode {cmd}");
		}
		if(Input.GetKeyDown(KeyCode.F4))
			ConsoleHandler.Handle($"/collision {!HasCollision}");

	}

    #region Static Resources
    public GameObject playerModel, playerLogic;
	public SpriteRenderer holdingItemPlaceholder;
	#endregion

	#region Dyniamic Variables
	private int _health, _maxHealth, _maxArmor, _maxStrength, _armor, _strength;
	public Inventory inventory;
	#endregion

	#region Statistics Variables
	[HideInInspector]
	public uint healthGained, healthLost;
	#endregion

	#region References
	public Light2D playerLight;
    #endregion

    #region Properties

	public bool HasCollision{ get => BoxCollider2D.enabled; set => BoxCollider2D.enabled = value; }

    public int Health { get => _health; 
		set 
		{ 
			_health = value;
			if(UIInventory.Singleton?.heartStat != null)
				UIInventory.Singleton.heartStat.text = $"{_health}/{_maxHealth}";
			PlayerHealth.Singleton.CurrentHealth = _health;
		} 
	}
	public int MaxHealth {
		get => _maxHealth;
		set 
		{ 
			_maxHealth = value;
			if (UIInventory.Singleton?.heartStat != null)
				UIInventory.Singleton.heartStat.text = $"{_health}/{_maxHealth}";
			PlayerHealth.Singleton.maxHealth = _maxHealth;
			PlayerHealth.Singleton.InitiateSprites();
		}
	}
	public int MaxArmor {
		get => _maxArmor;
		set { _maxArmor = value; UIInventory.Singleton.shieldStat.text = $"{_armor}/{_maxArmor}";}
	}
	public int Armor {
		get => _armor;
		set { _armor = value; UIInventory.Singleton.shieldStat.text = $"{_armor}/{_maxArmor}"; }
	}
	public int Strength {
		get => _strength;
		set { _strength = value; UIInventory.Singleton.swordStat.text = $"{_strength}/{_maxStrength}"; }
	}
	public int MaxStrength {
		get => _maxStrength;
		set { _maxStrength = value; UIInventory.Singleton.swordStat.text = $"{_strength}/{_maxStrength}"; }
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
		holdingItemPlaceholder.sprite = Inventory.Singleton.SelectedItemObj?.itemImage;
	}

	public void Init(){
		MaxHealth = GameManager.PlayerProfileNow.maxHealth == 0 ? 100 : PlayerVariables.Singleton.MaxHealth;
		Health = GameManager.PlayerProfileNow.health != 0 ? GameManager.PlayerProfileNow.health : MaxHealth==0? 100 : MaxHealth; //THIS IS THE PROBLEM AHHHHHH
		Armor = GameManager.PlayerProfileNow.armor;
		healthGained = GameManager.PlayerProfileNow.healthGained;
		healthLost = GameManager.PlayerProfileNow.healthLost;
	}

	
}
public enum Gamemode{
	SURVIVAL, CREATIVE, ADVENTURE
}

public enum Dimension{
	OVERWORLD, DUNGEON, OTHER, NULL
}

public enum CharacterRace
{
	MAGICIAN, HUMAN
}