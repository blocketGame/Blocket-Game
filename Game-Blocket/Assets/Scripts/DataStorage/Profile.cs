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

	public Profile(string name, int? profileHash)
    {
		this.name = name;
		this.profileHash = profileHash ?? new System.Random().Next();
	}
}



/// <summary>
/// UNDONE
/// </summary>
public class WorldProfile : Profile
{
	public List<SaveAbleChunk> chunks = new List<SaveAbleChunk>();

	public WorldProfile(string name, int? profileHash) : base(name, profileHash){}

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

	public static ChunkData CastSAChunkToChunk(SaveAbleChunk s) {
		return new ChunkData(s.blockIDs, s.blockIDsBG, null, s.chunkPosition);
	}

	public static SaveAbleChunk CastSAChunkToChunk(ChunkData s) {
		return new SaveAbleChunk(TerrainHandler.CastVector2ToInt(s.chunkPosition), s.blocks, s.bgBlocks, null);
	}
}

public class SettingsProfile : Profile{
	public KeyCode InventoryKey { get; set; } = KeyCode.E;
	public KeyCode MainInteractionKey { get; set; } = KeyCode.Mouse0;
	public KeyCode SideInteractionKey { get; set; } = KeyCode.Mouse1;
	public KeyCode JumpKey { get; set; } = KeyCode.Space;
	public KeyCode RollKey { get; set; } = KeyCode.LeftControl;
	public KeyCode CrawlKey { get; set; } = KeyCode.LeftShift;
	public SettingsProfile(string name, int? profileHash) : base(name, profileHash){}
}

/// <summary>
/// Storing player data
/// </summary>
public class PlayerProfile : Profile
{
	#region Inventory
	/// <summary>
	/// TODO capcity not hardcoded
	/// </summary>
	public List<SaveAbleItem> inventoryItems = new List<SaveAbleItem>(40);
	public List<SaveAbleItem> accessoiresItems = new List<SaveAbleItem>(7);
	public List<SaveAbleItem> armorItems = new List<SaveAbleItem>(3	);
	#endregion
	#region PlayerVariables
	public ushort health, armor, maxHealth, maxArmor;
	public uint healthGained, healthLost;
	#endregion

	public PlayerProfile(string name, int? profileHash) : base(name, profileHash)
    {

    }

	[Serializable]
	public struct SaveAbleItem
	{
		public uint itemId;
		public ushort count;

		public SaveAbleItem(uint itemId, ushort count)
		{
			this.itemId = itemId;
			this.count = count;
		}
	}
}
