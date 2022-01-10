using System;
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
public static class ProfileHandler {

	/// <summary>
	/// TODO...
	/// </summary>
	public static int chunkWidth = 32, chunkHeight = 32;

	/// <summary>The Parent Directory for all profiles</summary>
	public static string profileParent = @"Profiles", playerProfileParent = profileParent + @"\Players", worldProfileParent = profileParent + @"\Worlds";

	#region ProfileHandling
	/// <summary></summary>
	public static void CheckParent() {
		if (!Directory.Exists(profileParent))
			Directory.CreateDirectory(profileParent);
		if (!Directory.Exists(playerProfileParent))
			Directory.CreateDirectory(playerProfileParent);
		if (!Directory.Exists(worldProfileParent))
			Directory.CreateDirectory(worldProfileParent);
		//foreach(string f in Directory.GetFiles(playerProfileParent))
		//	Debug.Log(f + File.Exists(f));
	}

	/// <summary>Exports the profile played now</summary>
	public static void ExportProfile(Profile p, bool player) {
		string prePath = player ? playerProfileParent : worldProfileParent;

		CheckParent();
		string strToWrite = JsonUtility.ToJson(p, true);
		if (p.name == null) {
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
			ExportProfile(GameManager.playerProfileNow, player);
			SavePlayerProfile();
		} else {
			SaveWorldProfile();
		}

	}

	/// <summary>Imports the Profile played now</summary>
	public static Profile ImportProfile(string profileName, bool player) {
		CheckParent();
		string data = string.Empty;
		foreach (string iString in FindAllProfiles(player)) {
			int x = iString.LastIndexOf(@"\"), y = iString.LastIndexOf('.');
			if (iString.Substring(x + 1, y - x - 1).Equals(profileName)) {
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

	/// <summary>Recognises all Profiles in the Parent dir</summary>
	/// <returns></returns>
	public static List<string> FindAllProfiles(bool player) {
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
		if (inventory != null) {
			//Inventory
			for (int i = 0; i < inventory.InvSlots.Count; i++)
				pfN.inventoryItems.Add(new SaveAbleItem(inventory.InvSlots[i].ItemID, inventory.InvSlots[i]?.ItemCount ?? 0));
			//Armor Slots
			for (int i = 0; i < inventory.ArmorSlots.Count; i++)
				pfN.armorItems.Add(new SaveAbleItem(inventory.ArmorSlots[i].ItemID, inventory.ArmorSlots[i]?.ItemCount ?? 0));
			//Accessoires
			for (int i = 0; i < inventory.AccessoiresSlots.Count; i++)
				pfN.accessoiresItems.Add(new SaveAbleItem(inventory.AccessoiresSlots[i].ItemID, inventory.AccessoiresSlots[i]?.ItemCount ?? 0));
		}
		if (GlobalVariables.PlayerVariables != null) {
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
	public static void LoadComponentsFromPlayerProfile(PlayerProfile profile) {
		if (profile != null && GameManager.playerProfileNow == null)
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
				inventory.InvSlots[i].ItemID = itemNow.itemId;
				inventory.InvSlots[i].ItemCount = itemNow.count;
			}

			//Armor Slots
			for (int i = 0; i < inventory.ArmorSlots.Count; i++) {
				if (profile.armorItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.armorItems[i];
				inventory.ArmorSlots[i].ItemID = itemNow.itemId;
				inventory.ArmorSlots[i].ItemCount = itemNow.count;
			}
			//Accessoires
			for (int i = 0; i < inventory.AccessoiresSlots.Count; i++) {
				if (profile.accessoiresItems.Count <= i)
					break;
				SaveAbleItem itemNow = profile.accessoiresItems[i];
				inventory.AccessoiresSlots[i].ItemID = itemNow.itemId;
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
		foreach (TerrainChunk tc in TerrainHandler.Chunks.Values) { 
			if (tc == null || tc?.ChunkData == null)
				throw new ArgumentNullException();
			worldProfile.chunks.Add(tc.ChunkData);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static void LoadComponentsFromWorldProfile(WorldProfile worldProfile) {
		GameManager.worldProfileNow = worldProfile;

		foreach (ChunkData chunkNow in worldProfile.chunks) {
			if (TerrainHandler.Chunks.ContainsKey(chunkNow.ChunkPositionInt)) {
				Debug.LogWarning($"Key already Contained: {chunkNow.chunkPosition}");
				continue;
			}
			TerrainHandler.Chunks.Add(chunkNow.ChunkPositionInt, new TerrainChunk(chunkNow));
		}
	}
	#endregion
	#endregion
	#region NewWorldProfileHandling

	/// <summary>UNDONE: Check if Worlddire is vaild</summary>
	/// <returns></returns>
	public static List<string> FindAllWorldDirectories() {
		return new List<string>(Directory.GetDirectories(worldProfileParent));
	}
	/// <summary>TODO: Move to ConfigFile</summary>
	public static readonly string chunkLocation = "chunks";

	public static string GetWorldDirFromName(string name) => $@"{worldProfileParent}\{name}";

	public static string GetChunkLocationFromMainDir(string mainDirName) => $@"{mainDirName}\{chunkLocation}";

	/// <summary>
	/// Checks if the Worlddirectory is valid
	/// </summary>
	/// <param name="mainDirName">Path to the Worlddirecotry (NOT general perent)</param>
	/// <returns>true if not null</returns>
	private static bool CheckWorldDirectory(string mainDirName) {
		CheckParent();
		if (string.IsNullOrEmpty(mainDirName.Trim()))
			return false;
		if (!Directory.Exists(mainDirName))
			Directory.CreateDirectory(mainDirName);
		if (!Directory.Exists(GetChunkLocationFromMainDir(mainDirName)))
			Directory.CreateDirectory(GetChunkLocationFromMainDir(mainDirName));
		return true;
	}

	#region WorldSave
	/// <summary>
	/// Saves the World<br></br>
	/// Used if new World created
	/// </summary>
	/// <param name="worldName">Worldname</param>
	/// <returns></returns>
	public static WorldProfile SaveWorld(string worldName) {
		CheckWorldDirectory(worldName);
		WorldProfile w = new WorldProfile(worldName, null);
		SaveWorldProfile(GetWorldDirFromName(worldName), w);
		return w;
	}

	/// <summary>
	/// Saves the World<br></br>
	/// Used if World is available
	/// </summary>
	/// <param name="wpToSave">Worldprofile</param>
	/// <returns>TODO</returns>
	public static bool SaveWorld(WorldProfile wpToSave) {
		SaveWorldProfile(GetWorldDirFromName(wpToSave.name), wpToSave);
		return true;
	}

	/// <summary>
	/// Converts a single bytearray to a string
	/// </summary>
	/// <param name="chunk">Chunkarray</param>
	/// <returns>string with lines</returns>
	private static string ConvertChunkArrToString(byte[,] chunk) {
		if (chunk == null)
			throw new ArgumentNullException("Chunk is null!");
		string data = string.Empty;
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++) {
				data += $"{chunk[x, y]},";
			}
			//Remove last ,
			data.Remove(data.Length - 1);
			data += "\n";
		}
		return data;
	}

	/// <summary>
	/// Converts a whole chunk to a string (IDs, IDBackgrounds, Drops) 
	/// </summary>
	/// <param name="sc"><see cref="SaveAbleChunk"/></param>
	/// <returns>the string which will directly be writeable</returns>
	private static string ConvertChunkDataToString(ChunkData cd) {
		string data = string.Empty;
		data += "Chunks:\n";
		data += ConvertChunkArrToString(cd.blocks);
		data += "ChunksBG:\n";
		data += ConvertChunkArrToString(cd.bgBlocks);
		data += "Drops:\n";	
		//Drops
		string tempDrops = string.Empty;
		foreach (Drop d in cd.drops)
			tempDrops += $"{d.DropID},{d.Count},{d.Position};";
		//Remove last ;
		data.Remove(data.Length - 1);
		data += tempDrops;

		return data;
	}

	/// <summary>
	/// Inner Method of Worldsaving
	/// </summary>
	/// <param name="mainDirName">Worlddirectory (NOT Parent)</param>
	/// <param name="worldToSave">WorldProfile</param>
	private static void SaveWorldProfile(string mainDirName, WorldProfile worldToSave) {
		CheckWorldDirectory(mainDirName);
		foreach (ChunkData cd in worldToSave.chunks) {
			string chunkPathI = GetChunkLocationFromMainDir(mainDirName) + @$"\Chunk {cd.ChunkPositionInt.x} {cd.ChunkPositionInt.y}";
			StreamWriter sw = File.Exists(chunkPathI) ? new StreamWriter(chunkPathI, false) : File.CreateText(chunkPathI);
			string data = ConvertChunkDataToString(cd);
			sw.Write(data);
			sw.Close();
			if (DebugVariables.showLoadAndSave)
				Debug.Log($"Chunk saved: {cd.chunkPosition}");
		}

	}
	#endregion

	#region WorldLoad
	/// <summary>
	/// Converts a string[] to chunks
	/// </summary>
	/// <param name="stringlines"></param>
	/// <returns>Byte Array</returns>
	/// <exception cref="ArgumentException">If False Input is given</exception>
	private static byte[,] ConvertStringLinesToByteArr(string[] stringlines) {
		byte[,] bytes = new byte[chunkWidth, chunkHeight];
		for (int x = 0; x < chunkWidth; x++) {
			string[] smallStringArr = stringlines[x].Split(',');
			for (int y = 0; y < chunkHeight; y++) {
				bytes[x, y] = byte.TryParse(smallStringArr[y].Trim(), out byte result) ? result : throw new ArgumentException("Chunk Loading: Parse Error");
			}
		}
		return bytes;
	}

	/// <summary>
	/// TODO: Exceptionhandling, DROPS
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static ChunkData GetChunkFromFile(string path) {
		List<string> dataLines = new List<string>(File.ReadAllLines(path));

		//Static Number bc of heading in File
		byte[,] blockIDs = ConvertStringLinesToByteArr(dataLines.GetRange(1, chunkWidth).ToArray());
		byte[,] blockIDBGs = ConvertStringLinesToByteArr(dataLines.GetRange(2 + chunkWidth, chunkWidth).ToArray());

		//Drops: 3 + GlobalVariables.WorldData.ChunkWidth
		List<Drop> drops = new List<Drop>();


		//Position
		string posString = path.Substring(path.LastIndexOf('\\') + "Chunk ".Length);
		string stringX = posString.Trim().Split(' ')[0], stringY = posString.Trim().Split(' ')[1];

		return new ChunkData(blockIDs, blockIDBGs, drops.ToArray(),new Vector2Int(int.Parse(stringX),int.Parse(stringY)));
	}

	/// <summary>
	/// TODO: Dont Load all Chunks!!
	/// </summary>
	/// <param name="worldname"></param>
	/// <returns></returns>
	public static WorldProfile LoadWorldProfile(string worldname) {
		CheckWorldDirectory(worldname);
		string chunkPath = GetChunkLocationFromMainDir(GetWorldDirFromName(worldname));

		List<ChunkData> chunks = new List<ChunkData>();

		foreach (string pathI in Directory.GetFiles(chunkPath)) {
			chunks.Add(GetChunkFromFile(pathI));
			if (DebugVariables.showLoadAndSave)
				Debug.Log($"Loaded Cunk: {pathI}");
		}

		return new WorldProfile(worldname, null) { chunks = chunks };
	}
	#endregion
	#endregion
}