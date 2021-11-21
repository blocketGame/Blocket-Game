using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static PlayerProfile;
using static Profile;
using static WorldProfile;

/// <summary>
/// UNDONE
/// Will be used for Saving and Loading data
/// </summary>
public static class FileHandler
{
	/// <summary>
	/// The Parent Directory for all profiles
	/// </summary>
	public static string profileParent = @"Profiles";
	public static string playerProfileParent = profileParent + @"\Players", worldProfileParent = profileParent + @"\Worlds";

	#region ProfileHandling
	public static void CheckParent(){
		if (!Directory.Exists(profileParent))
			Directory.CreateDirectory(profileParent);
		if (!Directory.Exists(playerProfileParent))
			Directory.CreateDirectory(playerProfileParent);
	}

	/// <summary>
	/// Exports the profile played now
	/// </summary>
	public static void ExportProfile(Profile p, bool player) {
		string prePath = player ? playerProfileParent : worldProfileParent;

		CheckParent();
		string strToWrite = JsonUtility.ToJson(p, true);
		File.WriteAllText(prePath + @$"\{p.name}.json", strToWrite);
	}

	public static void ExportProfile(bool player) {
		if(player)
			ExportProfile(SavePlayerProfile(), player);
		else
			ExportProfile(SaveWorldProfile(), player);
	}

	/// <summary>
	/// Imports the Profile played now
	/// </summary>
	public static void ImportProfile(string profileName, bool player)
	{
		CheckParent();
		string data = string.Empty;
		foreach(string iString in FindAllPlayerProfiles())
			if (iString.StartsWith(profileName))
			{
				data = File.ReadAllText(playerProfileParent + @"\" + iString);
				break;
			}
		if (data.Trim() == string.Empty)
			return;
		if (player)
			LoadPlayerProfile(JsonUtility.FromJson<PlayerProfile>(data));
		else
			return; ///UNDONE
	}

	

	#region PlayerProfiles
	/// <summary>
	/// Recognises all Profiles in the Parent dir
	/// </summary>
	/// <returns></returns>
	public static List<string> FindAllPlayerProfiles()
	{
		CheckParent();
		List<string> temp = new List<string>();
		foreach (string iString in Directory.GetFiles(playerProfileParent))
			if (iString.EndsWith(".json"))
				temp.Add(iString);
		return temp;
	}

	public static PlayerProfile SavePlayerProfile() {
		return SavePlayerProfile(GameManager.playerProfileNow);
	}

		/// <summary>
		/// Overrides the profile for saving
		/// </summary>
		/// <returns></returns>
	public static PlayerProfile SavePlayerProfile(PlayerProfile pfN) {
		Inventory inventory = GlobalVariables.Inventory;
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
		return pfN;
	}

	/// <summary>
	/// Overrides game components => matching profile
	/// </summary>
	/// <param name="profile"></param>
	public static void LoadPlayerProfile(PlayerProfile profile){
		GameManager.playerProfileNow = profile;
		Inventory inventory = GlobalVariables.Inventory;
		if (inventory != null) {
			ItemAssets iA = GlobalVariables.PlayerVariables.uIInventory.itemAssets;
			//Inventory
			for (int i = 0; i < inventory.InvSlots.Count; i++) {
				SaveAbleItem itemNow = profile.inventoryItems[i];
				inventory.InvSlots[i].Item = iA.GetItemFromItemID(itemNow.itemId);
				inventory.InvSlots[i].ItemCount = itemNow.count;
			}
				
			//Armor Slots
			for (int i = 0; i < inventory.ArmorSlots.Count; i++) {
				SaveAbleItem itemNow = profile.armorItems[i];
				inventory.ArmorSlots[i].Item = iA.GetItemFromItemID(itemNow.itemId);
				inventory.ArmorSlots[i].ItemCount = itemNow.count;
			}
			//Accessoires
			for (int i = 0; i < inventory.AccessoiresSlots.Count; i++) {
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

	public static WorldProfile SaveWorldProfile() {
		return SaveWorldProfile(GameManager.worldProfileNow);
	}

	public static WorldProfile SaveWorldProfile(WorldProfile worldProfile) {
		foreach (TerrainChunk tc in GlobalVariables.WorldData.Chunks.Values) {
			List<SaveAbleDrop> drops = new List<SaveAbleDrop>(tc.Drops.Count);
			foreach (Drop drop in tc.Drops)
				drops.Add(new SaveAbleDrop(drop.GameObject.transform.position, drop.DropID, drop.Count));
			worldProfile.chunks.Add(new SaveAbleChunk(tc.ChunkPositionWorldSpace, tc.BlockIDs, tc.BlockIDsBG, drops));
		}
		return worldProfile;
	}

	public static void LoadWorldProfile(WorldProfile worldProfile) {
		GameManager.worldProfileNow = worldProfile;

		foreach (SaveAbleChunk chunkNow in worldProfile.chunks) {
			GlobalVariables.WorldData.Chunks.Add(chunkNow.chunkPosition, new TerrainChunk(chunkNow.chunkPosition, chunkNow.blockIDs, chunkNow.blockIDsBG, new List<Drop>()));
		}

	}
	#endregion
	#endregion
}
