
using System;
using System.Collections.Generic;

using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;
using UnityEngine.Tilemaps;

using Random = System.Random;

/// <summary>
/// Class that stores that stores the terraindata with unity-components
/// </summary>
[Serializable]
public sealed class TerrainChunk : ChunkData{
	public static readonly float bgTileMapTransparency = 0.5f;


	public GameObject dropParent;
	public GameObject ParentGO { get; set; }
	public Tilemap TileMap { get; set; }
	public TilemapRenderer TileMapRenderer { get; set; }

	public GameObject CollisionGO { get; set; }
	public Tilemap CollisionTM { get; set; }
	public TilemapCollider2D TileMapCollider { get; set; }

	public GameObject BackgroundGO { get; set; }
	public Tilemap BGTileMap { get; set; }


	//Client
	#region Chunkbuilding Components
	/// <summary>Instantiates the Gameobbjects and Components</summary>
	public void ImportChunk(GameObject parent) {
		BuildGameObjects(parent);
		PlaceAllTiles();
		BuildCollisions();
		InImportQueue = false;
		//Import neighbourgh chaunks
		if(TerrainHandler.Chunks.TryGetValue(new Vector2Int(ChunkPositionInt.x + 1, ChunkPositionInt.y), out TerrainChunk tc1))
			tc1.BuildCollisions(false, false);
		if(TerrainHandler.Chunks.TryGetValue(new Vector2Int(ChunkPositionInt.x -1, ChunkPositionInt.y), out TerrainChunk tc2))
			tc2.BuildCollisions(false, false);
		if(TerrainHandler.Chunks.TryGetValue(new Vector2Int(ChunkPositionInt.x, ChunkPositionInt.y + 1), out TerrainChunk tc3))
			tc3.BuildCollisions(false, false);
		if(TerrainHandler.Chunks.TryGetValue(new Vector2Int(ChunkPositionInt.x, ChunkPositionInt.y - 1), out TerrainChunk tc4))
			tc4.BuildCollisions(false, false);
	}

	private void BuildGameObjects(GameObject chunkParent) {
		if (ParentGO != null) {
			Debug.LogWarning($"ChunkGO existing!: {ChunkPositionInt}");
			return;
		}
		///Chunk GO
		ParentGO = new GameObject(ChunkName(ChunkPositionInt, 0)) {
			tag = GlobalVariables.chunkTag
		};
		ParentGO.transform.SetParent(chunkParent.transform);
		ParentGO.transform.position = new Vector3(chunkPosition.x * WorldAssets.ChunkLength, chunkPosition.y * WorldAssets.ChunkHeight, 0f);

		TileMap = ParentGO.AddComponent<Tilemap>();
		TileMapRenderer = ParentGO.AddComponent<TilemapRenderer>();
		TileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		///BGChunk GO
		BackgroundGO = new GameObject(ChunkName(chunkPosition, 1));
		BackgroundGO.transform.SetParent(TileMap.transform);
		BackgroundGO.transform.position = new Vector3(chunkPosition.x * WorldAssets.ChunkLength, chunkPosition.y * WorldAssets.ChunkHeight, 0.001f);
		BGTileMap = BackgroundGO.AddComponent<Tilemap>();
		BackgroundGO.AddComponent<TilemapRenderer>();

		BGTileMap.color = new Color(bgTileMapTransparency, bgTileMapTransparency, bgTileMapTransparency);

		///Collision GO
		CollisionGO = new GameObject(ChunkName(chunkPosition, 2)) {
			tag = "Terrain",
			layer = 6
		};
		CollisionGO.transform.SetParent(TileMap.transform);
		CollisionGO.transform.position = new Vector3(chunkPosition.x * WorldAssets.ChunkLength, chunkPosition.y * WorldAssets.ChunkHeight, 0f);
		CollisionTM = CollisionGO.AddComponent<Tilemap>();
		TileMapCollider = CollisionGO.AddComponent<TilemapCollider2D>();
		CollisionTM.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		///Drop GO
		dropParent = new GameObject(ChunkName(chunkPosition, 3));
		dropParent.transform.SetParent(TileMap.transform);
	}

	/// <summary>
	/// Bulds Collsions in Collison-TILEMAP
	/// </summary>
	/// <param name="borderOnly">If the </param>
	/// <param name="overwrite"></param>
	/// <exception cref="NullReferenceException"></exception>
	public void BuildCollisions(bool borderOnly = false, bool overwrite = true) {
		if(CollisionTM == null)
			return;
		if(overwrite)
			CollisionTM.ClearAllTiles();
		if(borderOnly) {
			for(int i = 0; i < WorldAssets.ChunkLength; i++)
				BuildCollision(new Vector2Int(0, i), Vector2Int.left);
			for(int i = 0; i < WorldAssets.ChunkHeight; i++)
				BuildCollision(new Vector2Int(i, 0), Vector2Int.down);
			for(int i = 0; i < WorldAssets.ChunkLength; i++)
				BuildCollision(new Vector2Int(31, i), Vector2Int.right);
			for(int i = 0; i < WorldAssets.ChunkHeight; i++)
				BuildCollision(new Vector2Int(i, 31), Vector2Int.up);
		} else
			for(int x = 0; x < WorldAssets.ChunkLength; x++)
				for(int y = 0; y < WorldAssets.ChunkHeight; y++)
					BuildCollision(new Vector2Int(x, y));
	}

	/// <summary></summary>
	/// <param name="inChunk"></param>
	/// <param name="pivot">The direction where to check collsision</param>
	/// <returns>true if the block has to be collsions</returns>
	/// <exception cref="Exception">General Excepteion if c# is not working correctly</exception>
	/// <exception cref="ArgumentException">If Pivot is not a predefined</exception>
	public void BuildCollision(Vector2Int inChunk, Vector2Int? pivot = null){
		if(!BlockHasCollsision(blocks[inChunk.x, inChunk.y]))
			return;
		
		int worldX = inChunk.x + ChunkPositionInt.x * WorldAssets.ChunkLength;
		int worldY = inChunk.y + ChunkPositionInt.y * WorldAssets.ChunkHeight;

		bool build = false;

		if(pivot == null)
			build = TerrainHandler.Singleton.GetBlockFormCoordinate(worldX + 1, worldY) == 0 ||
				TerrainHandler.Singleton.GetBlockFormCoordinate(worldX, worldY + 1) == 0 ||
				TerrainHandler.Singleton.GetBlockFormCoordinate(worldX - 1, worldY) == 0 ||
				TerrainHandler.Singleton.GetBlockFormCoordinate(worldX, worldY - 1) == 0;
		else{ 
			//Pivot NotNull
			Vector2Int pivotNN = pivot ?? throw new Exception("C# Mistake");
			if(pivotNN == Vector2Int.left && TerrainHandler.Chunks.ContainsKey(new Vector2Int(ChunkPositionInt.x-1, ChunkPositionInt.y)))
				build = BlockHasCollsision(TerrainHandler.Singleton.GetBlockFormCoordinate(worldX - 1, worldY));
			if(pivotNN == Vector2Int.down && TerrainHandler.Chunks.ContainsKey(new Vector2Int(ChunkPositionInt.x, ChunkPositionInt.y -1)))
				build = BlockHasCollsision(TerrainHandler.Singleton.GetBlockFormCoordinate(worldX, worldY - 1));
			if(pivotNN == Vector2Int.up && TerrainHandler.Chunks.ContainsKey(new Vector2Int(ChunkPositionInt.x, ChunkPositionInt.y +1)))
				build = BlockHasCollsision(TerrainHandler.Singleton.GetBlockFormCoordinate(worldX, worldY + 1));
			if(pivotNN == Vector2Int.right && TerrainHandler.Chunks.ContainsKey(new Vector2Int(ChunkPositionInt.x + 1, ChunkPositionInt.y)))
				build = BlockHasCollsision(TerrainHandler.Singleton.GetBlockFormCoordinate(worldX + 1, worldY));
		}
		//SetTile
		if(build)
			CollisionTM.SetTile(new Vector3Int(inChunk.x, inChunk.y, 0), WorldAssets.Singleton.GetBlockbyId(1).tile);
		else
			CollisionTM.SetTile(new Vector3Int(inChunk.x, inChunk.y, 0), null);
	}

	public bool BlockHasCollsision(byte blockId){
		//TODO: If other Blocks has no Collision
		if(blockId == 0)
			return false;
		return true;
    }

	/// <summary></summary>
	public void PlaceAllTiles() {
		for (int x = 0; x < WorldAssets.ChunkLength; x++) {
			for (int y = 0; y < WorldAssets.ChunkHeight; y++) {
				PlaceTile(x, y, WorldAssets.Singleton.blocks[BlockIDs[x, y]].tile, false);
				PlaceTile(x, y, WorldAssets.Singleton.blocks[BlockIDsBG[x, y]].tile, true);
			}
		}
	}
	/// <summary>Lambda expression for shortening reasons</summary>
	private void PlaceTile(int x, int y, TileBase tile, bool background) {
		if (background)
			BGTileMap.SetTile(new Vector3Int(x, y, 0), tile);
		else
			TileMap.SetTile(new Vector3Int(x, y, 0), tile);
	}

	#endregion

	//Both
	#region Contsructors

	/// <summary>Used from Terraingeneration</summary>
	public TerrainChunk(Vector2Int chunkPosition) : base(null, null, null, chunkPosition){
		
	}
	#endregion

	//Both
	#region Utilmethods
	private static string ChunkName(Vector2 pos, byte kind) {
		return kind switch {
			0 => $"Chunk {pos.x} {pos.y}",
			1 => $"Chunk {pos.x} {pos.y} background",
			2 => $"Chunk {pos.x} {pos.y} collision",
			3 => $"Chunk {pos.x} {pos.y} drops",
			_ => throw new ArgumentException($"{kind}"),
		};
	}
	public bool Visible {
		get {
			if (ParentGO.activeSelf != BackgroundGO.activeSelf)
				Debug.LogWarning("ChunkObject and BG not same!");

			if (InImportQueue || NeedsInport())
				return false;
			return ParentGO?.activeSelf ?? throw new NullReferenceException("Not Imported");
		}
		set {
			if (NeedsInport())
				return;
			BackgroundGO?.SetActive(value);
			ParentGO?.SetActive(value);
		}
	}

	public bool NeedsInport() {
		//False if is in ImportQueue
		if (InImportQueue)
			return true;
		//False if is not Importet => Import it and log warning
		if (!IsImported) {
			Debug.LogWarning($"Hard Importing: {ChunkPositionInt}");
			lock (ClientTerrainHandler.ChunkTileInitializationQueue)
				ClientTerrainHandler.ChunkTileInitializationQueue.Enqueue(this);
			InImportQueue = true;
			return true;
		}
		return false;
	}

	public bool InImportQueue { get; set; }

	public bool IsImported => ParentGO != null && BackgroundGO != null && CollisionGO != null;
	#endregion

	//Both
	#region Shortcuts
	private byte[,] BlockIDs => blocks;
	private byte[,] BlockIDsBG => bgBlocks;
	public List<Drop> Drops { get; } = new List<Drop>();
	#endregion

	//Client
	#region PlayerInteration
	public void PlaceBlock(Vector3Int coordinate, uint itemID) {
		if (itemID == 0) {
			Debug.LogWarning("ItemId is 0");
			return;
		}
		byte blockId = ItemAssets.Singleton.GetBlockIdFromItemID(itemID);

		if (blockId == 0) {
			Debug.LogWarning($"No BlockId found for ItemId: {itemID}");
			return;
		}

		if (BlockIDs[coordinate.x, coordinate.y] != 0)
			return;
		//TODO: Make Inventory to ItemId not Item
		if (Inventory.Singleton.RemoveItem(ItemAssets.Singleton.GetItemFromItemID(itemID), 1)) {
			BlockIDs[coordinate.x, coordinate.y] = blockId;
			TileMap.SetTile(coordinate, WorldAssets.Singleton.blocks[blockId].tile);
		}

		///Neighb... Collision
		//Left
		if(coordinate.x == 0)
			TerrainHandler.Chunks[new Vector2Int(ChunkPositionInt.x - 1, ChunkPositionInt.y)]?.BuildCollisions();
		if(coordinate.x == 31)
			TerrainHandler.Chunks[new Vector2Int(ChunkPositionInt.x + 1, ChunkPositionInt.y)]?.BuildCollisions();
		if(coordinate.y == 0)
			TerrainHandler.Chunks[new Vector2Int(ChunkPositionInt.x, ChunkPositionInt.y + 1)]?.BuildCollisions();
		if(coordinate.y == 32)
			TerrainHandler.Chunks[new Vector2Int(ChunkPositionInt.x, ChunkPositionInt.y - 1)]?.BuildCollisions();
		BuildCollisions();
	}

	/// <summary>Removes the block out of the tilemap </summary>
	/// <param name="cord">Coordinate in the Chunk</param>
	public void DeleteBlock(Vector3Int cord, bool foreGround) {
		//if (BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * ChunkPositionInt.x), (coordinate.y - GlobalVariables.WorldData.ChunkHeight * ChunkPositionInt.y)] == 0) return;
		byte blockId = foreGround ? BlockIDs[cord.x, cord.y] : BlockIDsBG[cord.x, cord.y];
		if (blockId == 0) {
			Debug.LogWarning("Destoryed Air!");
			return;
		}

		if(foreGround){
			BlockIDs[cord.x, cord.y] = 0;
			TileMap.SetTile(cord, WorldAssets.Singleton.blocks[0].tile);
			BuildCollisions();
		} else{
			BlockIDsBG[cord.x, cord.y] = 0;
			BGTileMap.SetTile(cord, WorldAssets.Singleton.blocks[0].tile);
		}
		
		foreach(uint itemId in GetDroppedIDs(blockId))
			InstantiateDrop(cord, 1, itemId);
	}

	/// <summary></summary>
	/// <param name="blockId"></param>
	/// <returns></returns>
	public List<uint> GetDroppedIDs(byte blockId){
		List<uint> ids = new List<uint>();
		Random rand = new Random();

		foreach (BlockData.BlockDropAble blockDropAble in WorldAssets.Singleton.GetBlockbyId(blockId).blockDrops)
			if(Inventory.Singleton.SelectedItemObj is ToolItem tool && tool.toolType == blockDropAble.toolItemType || (blockDropAble.toolItemType == ToolItem.ToolType.DEFAULT))
				if ((rand.NextDouble() * 100) + 1 < blockDropAble.dropchance)
					ids.Add(blockDropAble.itemID);
		return ids;
	}
	#endregion

	//(Server) not sure
	#region Dropinteraction

	/// <summary>
	/// TODO: Move to BlockInteraction
	/// Creating Drop + rigidbody and other Components
	/// </summary>
	public void InstantiateDrop(Vector3Int coorBevore, byte count, uint itemID) {
		Vector3Int coordinate = new Vector3Int(
			coorBevore.x + (ChunkPositionInt.x * WorldAssets.ChunkLength),
			coorBevore.y + (ChunkPositionInt.y * WorldAssets.ChunkHeight),
			coorBevore.z);

		GameObject dropGO = new GameObject($"Drop ItemID: {itemID}");
		dropGO.transform.SetParent(dropParent.transform);

		dropGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		dropGO.transform.position = new Vector3(coordinate.x + 0.5f, coordinate.y + 0.5f, 0);
		dropGO.layer = LayerMask.NameToLayer("Drops");

		Drop drop = dropGO.AddComponent<Drop>();
		drop.ItemId = itemID;
		drop.Count = count;
		lock (Drops)
			Drops.Add(drop);
	}

	#endregion

	/// <summary> Chunks are euqal if they have the same <see cref="ChunkPosition"/> Vector</summary>
	/// <param name="obj">The other Object</param>
	/// <returns>True if the otherChunk is the same</returns>
	public new bool Equals(object obj) {
		return obj is TerrainChunk other && ChunkPositionInt.Equals(other.ChunkPositionInt);
	}
}

/// <summary>
/// Stores the importand Data of the Chunk
/// </summary>
public class ChunkData {
	public byte[,] blocks, bgBlocks;
	public Drop[] drops;
	public Vector2 chunkPosition;
	public Dictionary<byte, List<Vector2Int>> structureCoordinates = new Dictionary<byte, List<Vector2Int>>();

	public Vector2Int ChunkPositionInt {
		get{
			if(chunkPositionInt == null)
				chunkPositionInt = TerrainHandler.CastVector2ToInt(chunkPosition);
			return chunkPositionInt ?? throw new NullReferenceException();
		} 
	}
	private Vector2Int? chunkPositionInt;
	public Vector3 ChunkPostionWorldSpace => new Vector3(chunkPosition.x, chunkPosition.y);

	public ChunkData(byte[,] blocks, byte[,] bgBlocks, Drop[] drops, Vector2Int chunkPosition) {
		this.blocks = blocks ?? (new byte[WorldAssets.ChunkLength, WorldData.Singleton.ChunkHeight]);
		this.bgBlocks = bgBlocks ?? (new byte[WorldData.Singleton.ChunkWidth, WorldData.Singleton.ChunkHeight]);
		this.drops = drops ?? new Drop[0];
		if (chunkPosition == null)
			throw new ArgumentNullException("ChunkPosition is null!");
		this.chunkPosition = chunkPosition;
	}

	public override string ToString() {
		return $"Chunk: {chunkPosition}";
	}
}