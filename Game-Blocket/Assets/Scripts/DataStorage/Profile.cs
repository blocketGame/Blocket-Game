using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Used for Saving and Storing Data
/// </summary>
[Serializable]
public abstract class Profile{
	public int profileHash;
	public string name;

	public Profile(string name, int? profileHash = null)
    {
		this.name = name;
		this.profileHash = profileHash ?? new System.Random().Next();
	}
}

public class SettingsProfile : Profile{


	/// <summary>
	/// Default Keys for the whole game
	/// </summary>
	public Dictionary<string, KeyCode> Keys { private get; set; } = new Dictionary<string, KeyCode>() {
		///Default Keys:
		//UI
		{"InventoryKey",KeyCode.E},
		{"ChatKey", KeyCode.T },

		//Mechanics
		{"MainInteractionKey", KeyCode.Mouse0 },
		{"SideInteractionKey", KeyCode.Mouse1 },
		{"JumpKey", KeyCode.Space },
		{"RollKey", KeyCode.LeftControl },
		{"CrawlKey", KeyCode.LeftShift } ,
		{"CraftingInterface", KeyCode.F }
	};

	/// <summary>
	/// Checks if special UI is open and resitricts imput
	/// </summary>
	/// <param name="str">Name of the Command</param>
	/// <returns>keycode if exists</returns>
	public KeyCode GetKeyCode(string str){
		if(!GlobalVariables.UIInventory.ChatOpened)
			return Keys[str];
		return KeyCode.None;
    }

	public SettingsProfile(string name, int? profileHash) : base(name, profileHash) { }

}

