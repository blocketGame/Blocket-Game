using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

/// <summary>
/// UNDONE: Drops
/// </summary>
public class WorldProfile : Profile {
	public List<ChunkData> chunks = new List<ChunkData>();

	public WorldProfile(string name, int? profileHash = null) : base(name, profileHash) { }

	public static ChunkData CastSAChunkToChunk(SaveAbleChunk s) => new ChunkData(s.blockIDs, s.blockIDsBG, null, s.chunkPosition);
	public static SaveAbleChunk CastSAChunkToChunk(ChunkData s) => new SaveAbleChunk(TerrainHandler.CastVector2ToInt(s.chunkPosition), s.blocks, s.bgBlocks, null);

	[Serializable]
	public class SaveAbleChunk {
		public Vector2Int chunkPosition;
		public byte[,] blockIDs, blockIDsBG;
		public List<SaveAbleDrop> drops;

		public SaveAbleChunk(Vector2Int chunkPosition, byte[,] blockIDs, byte[,] blockIDsBG, List<SaveAbleDrop> drops) {
			this.chunkPosition = chunkPosition;
			this.blockIDs = blockIDs;
			this.blockIDsBG = blockIDsBG;
			this.drops = drops;
		}
	}

	[Serializable]
	public struct SaveAbleDrop {
		public Vector3 position;
		public uint itemID;
		public ushort itemCount;

		public SaveAbleDrop(Vector3 position, uint itemID, ushort itemCount) {
			this.position = position;
			this.itemID = itemID;
			this.itemCount = itemCount;
		}
	}
	public static string WorldProfileParent => @$"{ProfileHandler.ProfileParent}\Worlds";
	public static readonly string chunkLocation = "chunks";
	public static string GetWorldDirFromName => $@"{WorldProfileParent}\{GameManager.WorldProfileNow?.name ?? throw new ArgumentNullException()}";
	public static string GetChunkLocationFromMainDir => $@"{GetWorldDirFromName}\{chunkLocation}";
	public static List<string> FindAllWorldDirectories() => new List<string>(Directory.GetDirectories(WorldProfileParent));
	
	#region ProfileHandling

	/// <summary>
	/// Checks if the Worlddirectory is valid
	/// </summary>
	/// <param name="mainDirName">Path to the Worlddirecotry (NOT general perent)</param>
	/// <returns>true if not null</returns>
	private static bool CheckWorldDirectory(string mainDirName) {
		ProfileHandler.CheckParent();
		if (string.IsNullOrEmpty(mainDirName.Trim()))
			return false;
		if (!Directory.Exists(mainDirName))
			Directory.CreateDirectory(mainDirName);
		if (!Directory.Exists(GetChunkLocationFromMainDir))
			Directory.CreateDirectory(GetChunkLocationFromMainDir);
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	public static void SaveComponentsInWorldProfile() {
		foreach (TerrainChunk tc in TerrainHandler.Chunks.Values) {
			if (tc == null)
				throw new ArgumentNullException();
			GameManager.WorldProfileNow.chunks.Add(tc);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static void LoadComponentsFromWorldProfile(WorldProfile worldProfile) {
		GameManager.WorldProfileNow = worldProfile;

		foreach (ChunkData chunkNow in worldProfile.chunks) {
			if (TerrainHandler.Chunks.ContainsKey(chunkNow.ChunkPositionInt)) {
				Debug.LogWarning($"Key already Contained: {chunkNow.chunkPosition}");
				continue;
			}
			TerrainHandler.Chunks.Add(chunkNow.ChunkPositionInt, chunkNow as TerrainChunk );
		}
	}

	#region WorldSave
	/// <summary>
	/// Saves the World<br></br>
	/// Used if new World created
	/// </summary>
	/// <param name="worldName">Worldname</param>
	/// <returns></returns>
	public static void SaveWorld(WorldProfile worldP) {
		CheckWorldDirectory(worldP.name);
		SaveWorldProfile(GetWorldDirFromName, worldP);
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
		for (int x = 0; x < WorldAssets.ChunkLength; x++) {
			for (int y = 0; y < WorldAssets.ChunkLength; y++) {
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
	public static string ConvertChunkDataToString(ChunkData cd) {
		string data = string.Empty;
		data += $"{cd.ChunkPositionInt.x}|{cd.ChunkPositionInt.y}\n";
		data += ConvertChunkArrToString(cd.blocks);
		data += "ChunksBG:\n";
		data += ConvertChunkArrToString(cd.bgBlocks);
		data += "Drops:\n";
		//Drops
		string tempDrops = string.Empty;
		foreach (Drop d in cd.drops)
			tempDrops += $"{d.ItemId},{d.Count},{d.Position};";
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
		foreach (ChunkData cd in worldToSave.chunks)
			SaveChunk(cd);
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
		byte[,] bytes = new byte[WorldAssets.ChunkLength, WorldAssets.ChunkLength];
		for (int x = 0; x < WorldAssets.ChunkLength; x++) {
			string[] smallStringArr = stringlines[x].Split(',');
			for (int y = 0; y < WorldAssets.ChunkLength; y++) {
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
	public static ChunkData GetChunkFromFile(string path) => ReadFromString(new List<string>(File.ReadAllLines(path)), ParseChunkPosition(path));

	public static ChunkData ReadFromString(List<string> dataLines, Vector2Int? posA)
	{
		string[] posStr = dataLines[0].Split('|');
		Vector2Int pos = posA ?? new Vector2Int(int.Parse(posStr[0]), int.Parse(posStr[1].TrimEnd()));
		
		//Static Number bc of heading in File
		byte[,] blockIDs = ConvertStringLinesToByteArr(dataLines.GetRange(1, WorldAssets.ChunkLength).ToArray());
		byte[,] blockIDBGs = ConvertStringLinesToByteArr(dataLines.GetRange(2 + WorldAssets.ChunkLength, WorldAssets.ChunkLength).ToArray());

		//Drops: 3 + GlobalVariables.WorldData.ChunkWidth
		List<Drop> drops = new List<Drop>();
		return new ChunkData(blockIDs, blockIDBGs, drops.ToArray(), pos);
	}

	public static Vector2Int ParseChunkPosition(string path) {
		string posString = path.Substring(path.LastIndexOf('\\') + "Chunk ".Length);
		string stringX = posString.Trim().Split(' ')[0], stringY = posString.Trim().Split(' ')[1];
		return new Vector2Int(int.Parse(stringX), int.Parse(stringY));
	}

	/// <summary>
	/// TODO: Dont Load all Chunks!!
	/// </summary>
	/// <param name="worldname"></param>
	/// <returns></returns>
	public static WorldProfile LoadWorldProfile(string worldname) {
		CheckWorldDirectory(worldname);
		string chunkPath = GetChunkLocationFromMainDir;

		List<ChunkData> chunks = new List<ChunkData>();

		foreach (string pathI in Directory.GetFiles(chunkPath)) {
			chunks.Add(GetChunkFromFile(pathI));
			if (DebugVariables.ShowLoadAndSave)
				Debug.Log($"Loaded Cunk: {pathI}");
		}

		return new WorldProfile(worldname, null) { chunks = chunks };
	}

	public static TerrainChunk LoadChunk(Vector2Int chunkCord) {
		CheckWorldDirectory(GetWorldDirFromName);
		string chunkPath = GetChunkLocationFromMainDir;
		foreach (string s in Directory.GetFiles(chunkPath))
			if (chunkCord == ParseChunkPosition(s))
				return GetChunkFromFile(s) as TerrainChunk;
		if(DebugVariables.ShowChunkHandle)
			Debug.Log($"No Chunk found: {chunkCord}; GENERATING!");
		return null;

	}



	public static void SaveChunk(ChunkData cd) {
		string chunkPathI = GetChunkLocationFromMainDir + @$"\Chunk {cd.ChunkPositionInt.x} {cd.ChunkPositionInt.y}";
		StreamWriter sw = new StreamWriter(File.Exists(chunkPathI) ? File.OpenWrite(chunkPathI) : File.Create(chunkPathI));
		string data = ConvertChunkDataToString(cd);
		sw.Write(data);
		sw.Close();
		if (DebugVariables.ShowLoadAndSave)
			Debug.Log($"Chunk saved: {cd.chunkPosition}");
	}
	#endregion
	#endregion

}