using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static PlayerProfile;
using static WorldProfile;

/// <summary>
/// UNDONE
/// Will be used for Saving and Loading data<br></br>
/// <b>2 Sections: </b><br></br>
/// Import/Export=> From/ To File <br></br>
/// Sava/Load => From/ To Component
/// </summary>
public static class FileHandler
{
	/// <summary>
	/// The Parent Directory for all profiles
	/// </summary>
	public static string profileParent = @"Profiles", playerProfileParent = profileParent + @"\Players", worldProfileParent = profileParent + @"\Worlds";

	#region ProfileHandling
	/// <summary>
	/// 
	/// </summary>
	public static void CheckParent(){
		if (!Directory.Exists(profileParent))
			Directory.CreateDirectory(profileParent);
		if (!Directory.Exists(playerProfileParent))
			Directory.CreateDirectory(playerProfileParent);
		if (!Directory.Exists(worldProfileParent))
			Directory.CreateDirectory(worldProfileParent);
		//foreach(string f in Directory.GetFiles(playerProfileParent))
		//	Debug.Log(f + File.Exists(f));
	}

	/// <summary>
	/// Exports the profile played now
	/// </summary>
	public static void ExportProfile(Profile p, bool player) {
		string prePath = player ? playerProfileParent : worldProfileParent;

		CheckParent();
		string strToWrite = JsonUtility.ToJson(p, true);
		if(p.name == null) {
			Debug.LogWarning((player ? "Player" : "World") + "profile null!");
			p.name = player ? ListContentUI.selectedBtnNameCharacter : ListContentUI.selectedBtnNameWorld;
		}
		File.WriteAllText(prePath + @$"\{p.name}.json", strToWrite);
	}

	/// <summary>
	/// 
	/// </summary>
	public static void ExportProfile(bool player) {
		if (player) {
			SavePlayerProfile();
			ExportProfile(GameManager.playerProfileNow, player);
		} else {
			SaveWorldProfile();
			ExportProfile(GameManager.worldProfileNow, player);
		}
			
	}

	/// <summary>
	/// Imports the Profile played now
	/// </summary>
	public static Profile ImportProfile(string profileName, bool player)
	{
		CheckParent();
		string data = string.Empty;
		foreach(string iString in FindAllProfiles(player)) {
			int x = iString.LastIndexOf(@"\"), y = iString.LastIndexOf('.');
			if (iString.Substring(x + 1, y - x - 1).Equals(profileName))
			{
				string path = iString;
				//Readoperation
				
				if (!File.Exists(path))
					throw new IOException("File not Found");
				data = File.ReadAllText(path);
				break;
			}
		}
		return data.Trim() == string.Empty
			? null
			: player ? JsonUtility.FromJson<PlayerProfile>(data) : (Profile)JsonUtility.FromJson<WorldProfile>(data);
	}

/// <summary>
	/// Recognises all Profiles in the Parent dir
	/// </summary>
	/// <returns></returns>
	public static List<string> FindAllProfiles(bool player)
	{
		CheckParent();
		List<string> temp = new List<string>();
		string path = player ? playerProfileParent : worldProfileParent;
		foreach (string iString in Directory.GetFiles(path))
			if (iString.EndsWith(".json"))
				temp.Add(iString);
		return temp;
	}

	#region PlayerProfiles
	

	public static void SavePlayerProfile() {
		SaveComponentsInPlayerProfile(GameManager.playerProfileNow);
	}

		/// <summary>
		/// Overrides the profile for saving
		/// </summary>
		/// <returns></returns>
	public static void SaveComponentsInPlayerProfile(PlayerProfile pfN) {
		Inventory inventory = GlobalVariables.Inventory;
		if (pfN == null)
			throw new NullReferenceException("Playerprofile is null!");
		if(inventory != null) { 
			//Inventory
			for(int i = 0; i < inventory.InvSlots.Count; i++)
				pfN.inventoryItems.Add(new SaveAbleItem(inventory.InvSlots[i].Item?.id ?? 0, inventory.InvSlots[i]?.ItemCount ?? 0));
			//Armor Slots
			for (int i = 0; i < inventory.ArmorSlots.Count; i++)
				pfN.armorItems.Add(new SaveAbleItem(inventory.ArmorSlots[i].Item?.id ?? 0, inventory.ArmorSlots[i]?.ItemCount ?? 0));
			//Accessoires
			for (int i = 0; i < inventory.AccessoiresSlots.Count; i++)
				pfN.accessoiresItems.Add(new SaveAbleItem(inventory.AccessoiresSlots[i].Item?.id ?? 0, inventory.AccessoiresSlots[i]?.ItemCount ?? 0));
		}
		if(GlobalVariables.PlayerVariables != null) { 
			//PlayerVariables
			pfN.health = GlobalVariables.PlayerVariables.Health;
			pfN.armor = GlobalVariables.PlayerVariables.Armor;
			pfN.healthGained = GlobalVariables.PlayerVariables.healthGained;
			pfN.healthLost = GlobalVariables.PlayerVariables.healthLost;
			pfN.maxHealth = GlobalVariables.PlayerVariables.MaxHealth;
		}
	}

	/// <summary>
	/// Overrides game components => matching profile
	/// </summary>
	/// <param name="profile"></param>
	public static void LoadComponentsFromPlayerProfile(PlayerProfile profile){
		if(profile != null && GameManager.playerProfileNow == null)
			GameManager.playerProfileNow = profile;
		if (GameManager.playerProfileNow == null)
			throw new NullReferenceException("PlayerProfile is null!");
		Inventory inventory = GlobalVariables.Inventory;
		if (inventory != null) {
			ItemAssets iA = GlobalVariables.PlayerVariables.uIInventory.itemAssets;
			//Inventory
			for (int i = 0; i < inventory.InvSlots.Count; i++) {
				if (profile.inventoryItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.inventoryItems[i];
				inventory.InvSlots[i].Item = iA.GetItemFromItemID(itemNow.itemId);
				inventory.InvSlots[i].ItemCount = itemNow.count;
			}
				
			//Armor Slots
			for (int i = 0; i < inventory.ArmorSlots.Count; i++) {
				if (profile.armorItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.armorItems[i];
				inventory.ArmorSlots[i].Item = iA.GetItemFromItemID(itemNow.itemId);
				inventory.ArmorSlots[i].ItemCount = itemNow.count;
			}
			//Accessoires
			for (int i = 0; i < inventory.AccessoiresSlots.Count; i++) {
				if (profile.accessoiresItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.accessoiresItems[i];
				inventory.AccessoiresSlots[i].Item = iA.GetItemFromItemID(itemNow.itemId);
				inventory.AccessoiresSlots[i].ItemCount = itemNow.count;
			}
		}
		if (GlobalVariables.PlayerVariables != null) {
			GlobalVariables.PlayerVariables.Health = profile.health;
			GlobalVariables.PlayerVariables.Armor = profile.armor;
			GlobalVariables.PlayerVariables.healthGained = profile.healthGained;
			GlobalVariables.PlayerVariables.healthLost = profile.healthLost;
			GlobalVariables.PlayerVariables.MaxHealth = profile.maxHealth;
		}
	}
	#endregion
	#region Worldprofiles

	/// <summary>
	/// 
	/// </summary>
	public static void SaveWorldProfile() {
		SaveComponentsInWorldProfile(GameManager.worldProfileNow);
	}

	/// <summary>
	/// 
	/// </summary>
	public static void SaveComponentsInWorldProfile(WorldProfile worldProfile) {
		foreach (TerrainChunk tc in GlobalVariables.WorldData.Chunks.Values) {
			List<SaveAbleDrop> drops = new List<SaveAbleDrop>(tc.Drops.Count);
			foreach (Drop drop in tc.Drops)
				drops.Add(new SaveAbleDrop(drop.GameObject.transform.position, drop.DropID, drop.Count));
			worldProfile.chunks.Add(new SaveAbleChunk(tc.ChunkPositionWorldSpace, tc.BlockIDs, tc.BlockIDsBG, drops));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static void LoadComponentsFromWorldProfile(WorldProfile worldProfile) {
		GameManager.worldProfileNow = worldProfile;

		foreach (SaveAbleChunk chunkNow in worldProfile.chunks) {
			GlobalVariables.WorldData.Chunks.Add(chunkNow.chunkPosition, new TerrainChunk(chunkNow.chunkPosition, chunkNow.blockIDs, chunkNow.blockIDsBG, new List<Drop>()));
		}

	}
	#endregion
	#endregion
}
