using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static PlayerProfile;
using static Profile;

/// <summary>
/// TODO:...<br></br>
/// Will be used for Saving and Loading data
/// </summary>
public class FileHandler
{
	/// <summary>
	/// The Parent Directory for all profiles
	/// </summary>
	public static string profileParent = @"..\..\Profiles";
	public static string playerProfileParent = profileParent + @"\Player";

	#region ProfileHandling
	public void CheckParent(){
		if (!Directory.Exists(profileParent))
			Directory.CreateDirectory(profileParent);
		if (!Directory.Exists(playerProfileParent))
			Directory.CreateDirectory(playerProfileParent);
	}

	/// <summary>
	/// Exports the profile played now
	/// </summary>
	public void ExportProfile(){
		CheckParent();
		Profile profileToSave = SaveProfile();
		string strToWrite = JsonUtility.ToJson(profileToSave, true);
		File.WriteAllText(playerProfileParent + @"\" + profileToSave.name + ".json", strToWrite);
	}

	/// <summary>
	/// Recognises all Profiles in the Parent dir
	/// </summary>
	/// <returns></returns>
	public List<string> FindAllProfiles()
    {
		List<string> temp = new List<string>();
		foreach (string iString in Directory.GetFiles(profileParent))
			if (iString.EndsWith(".json"))
				temp.Add(iString);
		return temp;
    }

	/// <summary>
	/// Imports the Profile played now
	/// </summary>
	public void ImportProfile(string profileName)
	{
		CheckParent();
		string data = string.Empty;
		foreach(string iString in FindAllProfiles())
            if (iString.StartsWith(profileName))
            {
				data = File.ReadAllText(playerProfileParent + @"\" + iString);
				break;
            }
		if (data.Trim() == string.Empty)
			return;
		LoadProfile(JsonUtility.FromJson<PlayerProfile>(data));
	}

	/// <summary>
	/// Overrides the profile for saving
	/// </summary>
	/// <returns></returns>
	private PlayerProfile SaveProfile(){
		PlayerProfile pfN = GlobalVariables.PlayerVariables.ProfileNow;
		//Inventory
		foreach (UIInventorySlot iS in GlobalVariables.Inventory.InvSlots)
			pfN.InventoryItems.Add(new SavableItem(iS.Item.id, iS.ItemCount));
		//Armor Slots
		foreach (UIInventorySlot iS in GlobalVariables.Inventory.ArmorSlots)
			pfN.ArmorItems.Add(new SavableItem(iS.Item.id, iS.ItemCount));
		//Accessoires
		foreach (UIInventorySlot iS in GlobalVariables.Inventory.AccessoiresSlots)
			pfN.AccessoiresItems.Add(new SavableItem(iS.Item.id, iS.ItemCount));
		//PlayerVariables
		pfN.health = GlobalVariables.PlayerVariables.Health;
		pfN.armor = GlobalVariables.PlayerVariables.Armor;
		pfN.healthGained = GlobalVariables.PlayerVariables.healthGained;
		pfN.healthLost = GlobalVariables.PlayerVariables.healthLost;
		pfN.maxHealth = GlobalVariables.PlayerVariables.MaxHealth;
		return pfN;
	}

	/// <summary>
	/// Overrides game components => matching profile
	/// </summary>
	/// <param name="profile"></param>
	private void LoadProfile(PlayerProfile profile){
		GlobalVariables.PlayerVariables.ProfileNow = profile;
		/**
		foreach (SavableItem sI in profile.)
			GlobalVariables.Inventory.AddItem()
		//Armor Slots
		foreach (UIInventorySlot iS in GlobalVariables.Inventory.ArmorSlots)
			pfN.ArmorItems.Add(new SavableItem(iS.Item.id, iS.ItemCount));
		//Accessoires
		foreach (UIInventorySlot iS in GlobalVariables.Inventory.AccessoiresSlots)
			pfN.AccessoiresItems.Add(new SavableItem(iS.Item.id, iS.ItemCount));
		//PlayerVariables
		**/
		GlobalVariables.PlayerVariables.Health = profile.health;
		GlobalVariables.PlayerVariables.Armor = profile.armor;
		GlobalVariables.PlayerVariables.healthGained = profile.healthGained;
		GlobalVariables.PlayerVariables.healthLost = profile.healthLost;
		GlobalVariables.PlayerVariables.MaxHealth = profile.maxHealth;
	}
	#endregion
}
