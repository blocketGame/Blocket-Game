using MLAPI.Serialization;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
/*
* @Author : Thomas Boigner / Cse19455
*/
[Serializable]
public sealed class TerrainChunk
{
	public ChunkData ChunkData { get; private set; }

	public bool IsVisible {
		get {
			if (ChunkObject.activeSelf != BackgroundObject.activeSelf)
				Debug.LogWarning("ChunkObject and BG not same!");
			return ChunkObject.activeSelf;
		}
		set {
			if (!IsImported)
				Debug.LogWarning($"Not Imported: {ChunkData.ChunkPositionInt}");
			BackgroundObject.SetActive(value);
			ChunkObject.SetActive(value);
		}
	}

	public bool IsImported { get {
			return ChunkObject != null && BackgroundObject != null;
		}
	}
	public bool InQueueForImport { get; set; } = false;

	#region Shortcuts
	private Vector2 ChunkPosition => ChunkData.chunkPosition;
	private Vector2Int ChunkPositionInt => TerrainHandler.CastVector2ToInt(ChunkData.chunkPosition);
	private Vector3 ChunkPositionWorldSpace => new Vector3(ChunkPosition.x, ChunkPosition.y);
	private byte[,] BlockIDs => ChunkData.blocks;
	private byte[,] BlockIDsBG => ChunkData.bgBlocks;
	private List<Drop> Drops => new List<Drop>(ChunkData.drops);
	#endregion

	#region WorldSpecifications
	public GameObject dropParent;
	[SerializeField]
	private GameObject thisGO;
	[SerializeField]
	private GameObject collisionGO;
	
	public GameObject ChunkObject { get => thisGO; set => thisGO = value; }
	public GameObject CollisionObject { get => collisionGO; set => collisionGO = value; }

	public Tilemap BackgroundTilemap { get; set; }
	public GameObject BackgroundObject { get; set; }

	[SerializeField]
	private Tilemap chunkTileMap;
	[SerializeField]
	private Tilemap collisionTileMap;
	[SerializeField]
	private TilemapRenderer chunkTileMapRenderer;
	[SerializeField]
	private TilemapCollider2D chunkTileMapCollider;

	public Tilemap ChunkTileMap { get => chunkTileMap; set => chunkTileMap = value; }
	public TilemapRenderer ChunkTileMapRenderer { get => chunkTileMapRenderer; set => chunkTileMapRenderer = value; }
	public Tilemap CollisionTileMap { get => collisionTileMap; set => collisionTileMap = value; }
	public TilemapCollider2D ChunkTileMapCollider { get => chunkTileMapCollider; set => chunkTileMapCollider = value; }
	#endregion

	#region Contsructors
	public TerrainChunk(Vector2 chunkPosition, byte[,] blockIDs, byte[,] blockIDsBG, List<Drop> drops) {
		ChunkData = new ChunkData(blockIDs, blockIDsBG, drops.ToArray(), TerrainHandler.CastVector2ToInt(chunkPosition));
	}

	public TerrainChunk(Vector2Int chunkPosition, GameObject chunkParent)
	{
		ChunkData = new ChunkData(null, null, null, chunkPosition);
	}

	public TerrainChunk(Vector2 chunkPosition, GameObject chunkParent)
	{
		ChunkData = new ChunkData(null, null, null, TerrainHandler.CastVector2ToInt(chunkPosition));
	}
	#endregion

	/// <summary>
	/// makes the GameObject and COmponents...
	/// </summary>
	/// <param name="parent"></param>
	public void ImportChunk(GameObject parent) {
		BuildAllChunkLayers(parent);
		PlaceAllTiles();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="kind"></param>
	/// <returns></returns>
	private static string ChunkName(Vector2 pos, byte kind) {
		return kind switch {
			0 => $"Chunk {pos.x} {pos.y}",
			1 => $"Chunk {pos.x} {pos.y} background",
			2 => $"Chunk {pos.x} {pos.y} collision",
			3 => $"Chunk {pos.x} {pos.y} drops",
			_ => throw new ArgumentException($"{kind}"),
		};
	}

	/// <summary>
	/// Creates Chunk - Bg / Collision / - tilemaps
	/// </summary>
	/// <returns></returns>
	private void BuildAllChunkLayers(GameObject chunkParent)
	{
		if (ChunkObject != null) { 
			Debug.LogWarning($"ChunkGO existing!: {ChunkPositionInt}");
			return;
		}
		///Chunk GO
		ChunkObject = new GameObject(ChunkName(ChunkPosition, 0)) {
			tag = "Chunk"
		};
		ChunkObject.transform.SetParent(chunkParent.transform);
		ChunkObject.transform.position = new Vector3(ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth, ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0f);

		ChunkTileMap = ChunkObject.AddComponent<Tilemap>();
		ChunkTileMapRenderer = ChunkObject.AddComponent<TilemapRenderer>();
		ChunkTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		///BGChunk GO
		BackgroundObject = new GameObject(ChunkName(ChunkPosition, 1));
		BackgroundObject.transform.SetParent(ChunkTileMap.transform);
		BackgroundObject.transform.position = new Vector3(ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth, ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0.001f);
		BackgroundTilemap = BackgroundObject.AddComponent<Tilemap>();
		BackgroundObject.AddComponent<TilemapRenderer>();

		///Collision GO
		CollisionObject = new GameObject(ChunkName(ChunkPosition, 2)) {
			tag = "Terrain"
		};
		CollisionObject.transform.SetParent(ChunkTileMap.transform);
		CollisionObject.transform.position = new Vector3(ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth, ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0f);
		CollisionTileMap = CollisionObject.AddComponent<Tilemap>();
		ChunkTileMapCollider = CollisionObject.AddComponent<TilemapCollider2D>();
		CollisionTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		///Drop GO
		dropParent = new GameObject(ChunkName(ChunkPosition, 3));
		dropParent.transform.SetParent(ChunkTileMap.transform);
		InsertDrops();
	}

	/// <summary>
	/// Add the ids of the blocks to the blockIDs array
	/// </summary>
	/// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
	/// <param name="biomindex">Index of the biom of the chunk</param>
	public void GenerateChunk(float[] noisemap, float[,] caveNoisepmap, byte[,] oreNoiseMap, int[,] biomNoiseMap) {
		float caveSize = GlobalVariables.WorldData.InitCaveSize;
		if (ChunkPosition.y < 0) {
			caveSize = GlobalVariables.WorldData.InitCaveSize - ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight * 0.001f;
		} else
		if (ChunkPosition.y > 0) {
			caveSize = GlobalVariables.WorldData.InitCaveSize + ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight * 0.001f;
		}

		if (caveSize > 0) {
			caveSize = 0;
		}

		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			AnimationCurve heightCurve = new AnimationCurve(GlobalVariables.WorldData.Heightcurve.keys);
			int positionHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[x]) * GlobalVariables.WorldData.HeightMultiplier) + 1;

			for (int y = GlobalVariables.WorldData.ChunkHeight - 1; y >= 0; y--) {
				Biom biom = GlobalVariables.WorldData.Biom[biomNoiseMap[x, y]];
				if (y + ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight < positionHeight) {
					if (caveNoisepmap[x, y] > caveSize) {
						if (caveNoisepmap[x, y] < caveSize + GlobalVariables.WorldData.StoneSize) {
							BlockIDs[x, y] = biom.StoneBlockId;
						} else {
							foreach (RegionData region in biom.Regions) {
								if (region.RegionRange <= positionHeight - (y + ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight)) {
									BlockIDs[x, y] = region.BlockID;
								}
							}

							foreach (OreData oreData in biom.Ores) {
								if (oreData.BlockID == oreNoiseMap[x, y]) {
									BlockIDs[x, y] = oreNoiseMap[x, y];
								}
							}
						}
					}

					foreach (RegionData regionBG in biom.BgRegions) {
						if (regionBG.RegionRange <= positionHeight - (y + ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight)) {
							BlockIDsBG[x, y] = regionBG.BlockID;
						}
					}
				}
				//Place Trees.
				if (x % 5 == 0 && ChunkPosition.y == 0)
				{
					//	//try to spawn a Tree
					//GenerateTrees(x, positionHeight, biom.Index);
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void PlaceAllTiles() {
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++) {
				PlaceTile(x,y, GlobalVariables.WorldData.Blocks[BlockIDs[x, y]].Tile, false);
				PlaceTile(x,y, GlobalVariables.WorldData.Blocks[BlockIDsBG[x, y]].Tile, true);
			}
		}
	}

	/// <summary>
	/// places the tiles in the Tilemap according to the blockIDs array
	/// </summary>
	/// <param name="biomindex">Index of the biom of the chunk</param>b
	public void PlaceTiles(float[,] biomNoiseMap, bool init)
	{
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++)
		{
			int heightvalue = 0;
			int blockIDpos = GlobalVariables.WorldData.Biom[(int)biomNoiseMap[x, 0]].Regions.Length - 1;
			for (int y = GlobalVariables.WorldData.ChunkHeight - 1; y >= 0; y--)
			{
				if (BlockIDs[x, y] != 0)
				{
					if (heightvalue == GlobalVariables.WorldData.Biom[(int)biomNoiseMap[x, y]].Regions[blockIDpos].RegionRange)
					{
						blockIDpos--;
						heightvalue = 0;
					}
					else
						heightvalue++;
					PlaceTile(x,y, GlobalVariables.WorldData.Blocks[BlockIDs[x, y]].Tile, false);
					if (init)
						PlaceTile(x,y, GlobalVariables.WorldData.Blocks[BlockIDsBG[x, y]].Tile, true);
				}
			}
		}
	}

	/// <summary>
	/// Lambda expression for shortening reasons
	/// </summary>
	private void PlaceTile(int x, int y, TileBase tile, bool background) {
		if(background)
			BackgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
		else
			ChunkTileMap.SetTile(new Vector3Int(x, y, 0), tile);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>True if colliosion is sucessfully build</returns>
	public bool BuildCollisions()
	{
		if (CollisionTileMap == null)
			return false;
		CollisionTileMap.ClearAllTiles();
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++) {
				int worldX = x + ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth;
				int worldY = y + ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight;

				try {
					_ = BlockIDs[x, y];
				} catch (Exception) {
					Debug.LogWarning($"Not existing in Chunk {ChunkData.chunkPosition}: {x}, {y}");
				}

				if (BlockIDs[x, y] != 0 &&
					(GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX + 1, worldY) == 0 ||
					GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX, worldY + 1) == 0 ||
					GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX - 1, worldY) == 0 ||
					GlobalVariables.TerrainHandler.GetBlockFormCoordinate(worldX, worldY - 1) == 0)) {
					CollisionTileMap.SetTile(new Vector3Int(x, y, 0), GlobalVariables.TerrainHandler.GetBlockbyId(1).Tile);
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Removes the block out of the tilemap
	/// </summary>
	public void DeleteBlock(Vector3Int coordinate)
	{
		if (BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * ChunkPositionInt.x), (coordinate.y - GlobalVariables.WorldData.ChunkHeight * ChunkPositionInt.y)] == 0) return;

		InstantiateDrop(coordinate);
		ChunkTileMap.SetTile(new Vector3Int(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.x, coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.y, 0), null);
		BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.y] = 0;
		GlobalVariables.TerrainHandler.UpdateCollisionsAt(coordinate);
		GlobalVariables.TerrainHandler.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
		GlobalVariables.TerrainHandler.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
		GlobalVariables.TerrainHandler.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
		GlobalVariables.TerrainHandler.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
	}

	/// <summary>
	/// TODO: Move to BlockInteraction
	/// Creating Drop + rigidbody and other Components
	/// </summary>
	public void InstantiateDrop(Vector3Int coordinate)
	{
		Drop d = new Drop();
		d.DropID = GlobalVariables.WorldData.Blocks[BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.y]].Item1;
		d.Name = GlobalVariables.WorldData.Blocks[BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.y]].Name;
		d.GameObject = new GameObject($"Drop {d.DropID}");
		d.GameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		d.GameObject.AddComponent<SpriteRenderer>();
		d.GameObject.GetComponent<SpriteRenderer>().sprite = GlobalVariables.WorldData.Blocks[BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * ChunkPositionInt.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.TerrainHandler.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionInt.y]].Sprite;
		Vector3 c = coordinate;
		c.y = coordinate.y + 0.5f;
		c.x = coordinate.x + 0.5f;
		d.GameObject.transform.SetPositionAndRotation(c, new Quaternion());
		d.GameObject.AddComponent<Rigidbody2D>();
		d.GameObject.GetComponent<Rigidbody2D>().gravityScale = 20;
		d.GameObject.AddComponent<BoxCollider2D>();
		d.GameObject.layer = LayerMask.NameToLayer("Drops");
		d.Count = 1;
		Drops.Add(d);
		InsertDrops();
		d.GameObject.transform.SetParent(dropParent.transform);
	}
	/// <summary>
	/// Creates the Gameobject out of the Drops list
	/// </summary>
	public void InsertDrops()
	{
		for (int x = 0; x < Drops?.Count; x++)
		{
			for (int y = 0; y < Drops?.Count; y++)
			{
				if (Drops.Count > 1 && x != y)
					CheckDropCollision(x, y);
				//Drops[x].DropObject.transform.SetParent(DropObject.transform);
			}
		}
	}
	/// <summary>
	/// Saves FPS while removing unessesary gameobjects
	/// </summary>
	public void CheckDropCollision(int x, int y)
	{
		float dropgrouprange = GlobalVariables.WorldData.Groupdistance;
		if (GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).x + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).x &&
			GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).x - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).x &&
			GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).y + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).y &&
			GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].GameObject.transform.position).y - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].GameObject.transform.position).y &&
			Drops[x].GameObject.GetComponent<SpriteRenderer>().sprite.Equals(Drops[y].GameObject.GetComponent<SpriteRenderer>().sprite))
		{
			Drops[x].Count++;
			RemoveDropfromView(Drops[y]);
			dropParent.SetActive(true);
		}
	}

	/// <summary>
	/// returns the dropid of the drop it collides with
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public Drop CollidewithDrop(int x, int y)
	{
		//Einsammeldistanz
		float dropgrouprange = GlobalVariables.WorldData.PickUpDistance;
		for (int i = 0; i < Drops.Count; i++)
		{
			if (x + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].GameObject.transform.position).x &&
				x - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].GameObject.transform.position).x &&
				y + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].GameObject.transform.position).y &&
				y - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].GameObject.transform.position).y)
			{
				return Drops[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Removes the actual Drop from the scene
	/// </summary>
	/// <param name="removable"></param>
	public void RemoveDropfromView(Drop removable)
	{
		/*
		removable.DropObject.GetComponent<SpriteRenderer>().sprite = null;
		removable.DropObject.GetComponent<BoxCollider2D>().enabled = false;
		removable.DropObject.transform.parent = null;
		removable.DropObject = null;
		*/
		Drops.Remove(removable);
		GameObject.Destroy(removable.GameObject);
	}

	/// <summary>
	/// Chunks are euqal if they have the same <see cref="ChunkPosition"/> Vector
	/// </summary>
	/// <param name="obj">The other Object</param>
	/// <returns>True if the otherChunk is the same</returns>
	public new bool Equals(object obj)
	{
		return obj is TerrainChunk other && ChunkPositionInt.Equals(other.ChunkPositionInt);
	}
	//public void GenerateTrees(int x, int y, int biom) {
	//	//Chunk = 32 in der width.
	//	//Trees ben�tigen 5 Bl�cke in der width bis der n�chste BAum spawnen kann
	//	//[Funktioniert, aber ned sch�n]

	//	if (new System.Random(ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth + x).Next(1, 5) == 4 && x > GlobalVariables.WorldData.Strukturen[0].blocks.GetLength(0) && x < (32 - GlobalVariables.WorldData.Strukturen[0].blocks.GetLength(0))) {
	//		int rando = new System.Random(ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth + x).Next(5, 10);
	//		//for (int i = 0;i<rando;i++)
	//		//BlockIDsBG[x, y+i] = world.Strukturen[0].blocks[2,5];

	//		for (int z = 0; z < GlobalVariables.ItemAssets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0); z++) {
	//			for (int q = 0; q < GlobalVariables.ItemAssets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(1); q++) {
 //                   try { 
	//					if (BlockIDsBG[x + z - GlobalVariables.ItemAssets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0) / 2, y + q] == 0)
	//						BlockIDsBG[x + z - GlobalVariables.ItemAssets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0) / 2, y + q] = GlobalVariables.ItemAssets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks[z, q];
	//				}catch { }
	//			}
	//		}
	//		//int breite=0;
	//		//for(int b= rando+2; b > 4; b--)
	//		//{
	//		//    for(int o = -breite; o <= breite; o++)
	//		//    {
	//		//        if(BlockIDsBG[x + o, y + rando]==0)
	//		//        BlockIDsBG[x+o, y+b] = 17;
	//		//    }
	//		//    breite++;
	//		//}
	//	}
	//}



	public class ChunkList : List<ChunkData>, INetworkSerializable
	{
		public void NetworkSerialize(NetworkSerializer serializer)
		{
			for (int i = 0; i < this.Count; i++)
			{
				ChunkData chunk = this[i];
				serializer.Serialize(ref chunk.chunkPosition);
				#region Blocks
				// Length
				if (serializer.IsReading)
					chunk.blocks = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];
				for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++)
					for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++)
						serializer.Serialize(ref chunk.blocks[x, y]);
				#endregion
				#region BGBlocks
				// Length
				if (serializer.IsReading)
					chunk.bgBlocks = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];
				for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++)
					for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++)
						serializer.Serialize(ref chunk.bgBlocks[x, y]);
				#endregion
				#region Drops
				/**
				int dropArraylength = 0;
				if (!serializer.IsReading)
					dropArraylength = drops.Length;
				serializer.Serialize(ref dropArraylength);

				if (serializer.IsReading)
					bgBlocks = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];

				for (int i = 0; i < GlobalVariables.WorldData.ChunkWidth; i++)
						serializer.Serialize(ref drops[i]);
				*/
				#endregion
			}
		}
		}
	}

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