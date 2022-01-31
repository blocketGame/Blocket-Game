using MLAPI.Serialization;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Class that stores that stores the terraindata with unity-components
/// </summary>
[Serializable]
public sealed class TerrainChunk : ChunkData{
	public GameObject dropParent;

	public GameObject ParentGO { get; set; }
	public Tilemap TileMap { get; set; }
	public TilemapRenderer TileMapRenderer { get; set; }

	public GameObject CollisionGO { get; set; }
	public Tilemap CollisionTileMap { get; set; }
	public TilemapCollider2D TileMapCollider { get; set; }

	public GameObject BackgroundGO { get; set; }
	public Tilemap BackgroundTilemap { get; set; }

	#region Chunkbuilding Components
	/// <summary>Instantiates the Gameobbjects and Components</summary>
	public void ImportChunk(GameObject parent) {
		BuildGameObjects(parent);
		PlaceAllTiles();
		BuildCollisions();
		InImportQueue = false;
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
		ParentGO.transform.position = new Vector3(chunkPosition.x * GlobalVariables.WorldData.ChunkWidth, chunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0f);

		TileMap = ParentGO.AddComponent<Tilemap>();
		TileMapRenderer = ParentGO.AddComponent<TilemapRenderer>();
		TileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		///BGChunk GO
		BackgroundGO = new GameObject(ChunkName(chunkPosition, 1));
		BackgroundGO.transform.SetParent(TileMap.transform);
		BackgroundGO.transform.position = new Vector3(chunkPosition.x * GlobalVariables.WorldData.ChunkWidth, chunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0.001f);
		BackgroundTilemap = BackgroundGO.AddComponent<Tilemap>();
		BackgroundGO.AddComponent<TilemapRenderer>();

		///Collision GO
		CollisionGO = new GameObject(ChunkName(chunkPosition, 2)) {
			tag = "Terrain",
			layer = 6
		};
		CollisionGO.transform.SetParent(TileMap.transform);
		CollisionGO.transform.position = new Vector3(chunkPosition.x * GlobalVariables.WorldData.ChunkWidth, chunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0f);
		CollisionTileMap = CollisionGO.AddComponent<Tilemap>();
		TileMapCollider = CollisionGO.AddComponent<TilemapCollider2D>();
		CollisionTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		///Drop GO
		dropParent = new GameObject(ChunkName(chunkPosition, 3));
		dropParent.transform.SetParent(TileMap.transform);
		InsertDrops();
	}

	public void BuildCollisions() {
		if (CollisionTileMap == null)
			throw new NullReferenceException($"CollisionTileMap is null! {ChunkPositionInt}");
		CollisionTileMap.ClearAllTiles();
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++) {
				int worldX = x + ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth;
				int worldY = y + ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight;

				if (BlockIDs[x, y] != 0 &&
					(GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX + 1, worldY) == 0 ||
					GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX, worldY + 1) == 0 ||
					GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX - 1, worldY) == 0 ||
					GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX, worldY - 1) == 0)) {
					CollisionTileMap.SetTile(new Vector3Int(x, y, 0), GlobalVariables.WorldAssets.GetBlockbyId(1).tile);
				}
			}
		}
	}

	/// <summary></summary>
	public void PlaceAllTiles() {
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++) {
				if (BlockIDs[x, y] == 8)
					Debug.Log("a");
				PlaceTile(x, y, GlobalVariables.WorldData.Blocks[BlockIDs[x, y]].tile, false);
				PlaceTile(x, y, GlobalVariables.WorldData.Blocks[BlockIDsBG[x, y]].tile, true);
			}
		}
	}
	/// <summary>Lambda expression for shortening reasons</summary>
	private void PlaceTile(int x, int y, TileBase tile, bool background) {
		if (background)
			BackgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
		else
			TileMap.SetTile(new Vector3Int(x, y, 0), tile);
	}

	#endregion

	#region Contsructors

	/// <summary>Used from Terraingeneration</summary>
	public TerrainChunk(Vector2Int chunkPosition) : base(null, null, null, chunkPosition){
		
	}
	#endregion

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
			lock (TerrainHandler.ChunkTileInitializationQueue)
				TerrainHandler.ChunkTileInitializationQueue.Enqueue(this);
			InImportQueue = true;
			return true;
		}
		return false;
	}

	public bool InImportQueue { get; set; }

	public bool IsImported => ParentGO != null && BackgroundGO != null && CollisionGO != null;
	#endregion

	#region Shortcuts
	private byte[,] BlockIDs => blocks;
	private byte[,] BlockIDsBG => bgBlocks;
	public List<Drop> Drops { get; } = new List<Drop>();
	#endregion

	#region PlayerInteration
	public void PlaceBlock(Vector3Int coordinate, uint itemID) {
		if (itemID == 0) {
			Debug.LogWarning("ItemId is 0");
			return;
		}
		byte blockId = GlobalVariables.ItemAssets.GetBlockIdFromItemID(itemID);

		if (blockId == 0) {
			Debug.LogWarning($"No BlockId found for ItemId: {itemID}");
			return;
		}

		if (BlockIDs[coordinate.x, coordinate.y] != 0)
			return;
		//TODO: Make Inventory to ItemId not Item
		if (GlobalVariables.Inventory.RemoveItem(GlobalVariables.ItemAssets.GetItemFromItemID(itemID), 1)) {
			BlockIDs[coordinate.x, coordinate.y] = blockId;
			TileMap.SetTile(coordinate, GlobalVariables.WorldData.Blocks[blockId].tile);
		}

		BuildCollisions();
	}
	/// <summary>Removes the block out of the tilemap </summary>
	/// <param name="coordinate">Coordinate in the Chunk</param>
	public void DeleteBlock(Vector3Int coordinate) {
		//if (BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * ChunkPositionInt.x), (coordinate.y - GlobalVariables.WorldData.ChunkHeight * ChunkPositionInt.y)] == 0) return;
		byte blockId = BlockIDs[coordinate.x, coordinate.y];
		uint itemId = GlobalVariables.ItemAssets.GetItemIdFromBlockID(blockId);

		if (blockId == 0) {
			Debug.LogWarning("Destoryed Air!");
			return;
		}
		if (itemId == 0) {
			Debug.LogWarning($"No existing Item to blockID:{blockId}");
			return;
		}

		BlockIDs[coordinate.x, coordinate.y] = 0;
		TileMap.SetTile(coordinate, GlobalVariables.WorldData.Blocks[0].tile);

		BuildCollisions();
		InstantiateDrop(coordinate, 1, itemId);
	}
	#endregion

	#region Dropinteraction

	/// <summary>
	/// TODO: Move to BlockInteraction
	/// Creating Drop + rigidbody and other Components
	/// </summary>
	public void InstantiateDrop(Vector3Int coorBevore, byte count, uint itemID) {
		Vector3Int coordinate = new Vector3Int(
			coorBevore.x + (ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth),
			coorBevore.y + (ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight),
			coorBevore.z);

		///Drop Instance
		Drop drop = new Drop {
			ItemId = itemID,
			Name = "?",
			Count = count
		};

		///Drop GO
		drop.GameObject = new GameObject($"Drop {drop.ItemId}");
		drop.GameObject.transform.SetParent(dropParent.transform);
		drop.GameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		Vector3 posDrop = new Vector3(coordinate.x + 0.5f, coordinate.y + 0.5f, 0);
		drop.GameObject.transform.SetPositionAndRotation(posDrop, new Quaternion());
		drop.GameObject.layer = LayerMask.NameToLayer("Drops");

		///Drop-GO Components
		Rigidbody2D dropRB = drop.GameObject.AddComponent<Rigidbody2D>();
		dropRB.simulated = false;
		dropRB.gravityScale = 20;
		dropRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

		BoxCollider2D dropCollider = drop.GameObject.AddComponent<BoxCollider2D>();

		Physics2D.IgnoreCollision(TileMapCollider, dropCollider, false);
		Physics2D.IgnoreLayerCollision(GlobalVariables.LocalPlayer.layer, LayerMask.NameToLayer("Drops"), true);

		drop.GameObject.AddComponent<SpriteRenderer>().sprite = GlobalVariables.ItemAssets.GetSpriteFromItemID(itemID);

		lock (Drops)
			Drops.Add(drop);
		InsertDrops();
		dropRB.simulated = true;
	}

	public void PickedUpDrop(Drop drop) {
		GlobalVariables.Inventory.AddItem(GlobalVariables.ItemAssets.GetItemFromItemID(drop.ItemId), drop.Count, out ushort iCNA);
		drop.Count = iCNA;
		//TODO: Move lines after somewhere more modular
		if (drop.Count == 0) {
			Drops.Remove(drop);
			UnityEngine.Object.Destroy(drop.GameObject);
		}
	}


	/// <summary>
	/// Creates the Gameobject out of the Drops list
	/// </summary>
	public void InsertDrops() {
		for (int x = 0; x < Drops?.Count; x++) {
			for (int y = 0; y < Drops?.Count; y++) {
				if (Drops.Count > 1 && x != y)
					CheckDropCollision(x, y);
				//Drops[x].DropObject.transform.SetParent(DropObject.transform);
			}
		}
	}
	/// <summary>
	/// Saves FPS while removing unessesary gameobjects
	/// </summary>
	public void CheckDropCollision(int x, int y) {
		//float dropgrouprange = GlobalVariables.WorldData.Groupdistance;
		//if (GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).x + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).x &&
		//	GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).x - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).x &&
		//	GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).y + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).y &&
		//	GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).y - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).y &&
		//	Drops[x].GameObject.GetComponent<SpriteRenderer>().sprite.Equals(Drops[y].GameObject.GetComponent<SpriteRenderer>().sprite))
		//{
		//	Drops[x].Count++;
		//	RemoveDropfromView(Drops[y]);
		//	dropParent.SetActive(true);
		//}
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

	public Vector2Int ChunkPositionInt => TerrainHandler.CastVector2ToInt(chunkPosition);
	public Vector3 ChunkPostionWorldSpace => new Vector3(chunkPosition.x, chunkPosition.y);

	public ChunkData(byte[,] blocks, byte[,] bgBlocks, Drop[] drops, Vector2Int chunkPosition) {
		this.blocks = blocks ?? (new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight]);
		this.bgBlocks = bgBlocks ?? (new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight]);
		this.drops = drops ?? new Drop[0];
		if (chunkPosition == null)
			throw new ArgumentNullException("ChunkPosition is null!");
		this.chunkPosition = chunkPosition;
	}

	public override string ToString() {
		return $"Chunk: {chunkPosition}";
	}
}