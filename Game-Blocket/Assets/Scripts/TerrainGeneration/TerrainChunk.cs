using MLAPI.Serialization;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

using static TerrainChunk.ChunkList;
/*
* @Author : Thomas Boigner / Cse19455
*/
[Serializable]
public class TerrainChunk
{
	#region WorldSpecifications
	[SerializeField]
	private Vector2Int chunkPosition;
	[SerializeField]
	private Vector3 chunkPositionWorldSpace;
	[SerializeField]
	private byte[,] blockIDs;
	[SerializeField]
	private GameObject chunkObject;
	[SerializeField]
	private GameObject collisionObject;

	public bool ChunkVisible { 
		get {
			if (ChunkObject.activeSelf != BackgroundObject.activeSelf)
				Debug.LogWarning("ChunkObject and BG not same!");
			return ChunkObject.activeSelf;
		}
		set{
			BackgroundObject.SetActive(value);
			ChunkObject.SetActive(value);
		}
	}

	public Vector2Int ChunkPosition { get => chunkPosition; set => chunkPosition = value; }
	public Vector3 ChunkPositionWorldSpace { get => chunkPositionWorldSpace; set => chunkPositionWorldSpace = value; }
	public byte[,] BlockIDs { get => blockIDs; set => blockIDs = value; }
	public GameObject ChunkObject { get => chunkObject; set => chunkObject = value; }
	public GameObject CollisionObject { get => collisionObject; set => collisionObject = value; }
	#endregion

	#region BackgroundsAndTilemaps
	[SerializeField]
	private byte[,] blockIDsBG;
	private Tilemap backgroundTilemap;
	private GameObject backgroundObject;

	public Tilemap BackgroundTilemap { get => backgroundTilemap; set => backgroundTilemap = value; }
	public GameObject BackgroundObject { get => backgroundObject; set => backgroundObject = value; }
	public byte[,] BlockIDsBG { get => blockIDsBG; set => blockIDsBG = value; }
	#endregion

	#region Tilemaps

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

	#region Drops
	[SerializeField]
	private List<Drop> drops;
	private GameObject dropObject;
	private Vector2 vector2;
	private GameObject chunkParent;

	public GameObject DropObject { get => dropObject; set => dropObject = value; }
	public List<Drop> Drops { get => drops; set => drops = value; }
	#endregion

	public TerrainChunk(Vector2Int chunkPosition, GameObject chunkParent)
	{
		this.ChunkPosition = chunkPosition;
		this.ChunkPositionWorldSpace = new Vector3(chunkPosition.x * GlobalVariables.WorldData.ChunkWidth, chunkPosition.y * GlobalVariables.WorldData.ChunkHeight);
		this.BlockIDs = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];
		this.blockIDsBG = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];
		this.drops = new List<Drop>();
		this.ChunkObject = BuildAllChunkLayers(chunkParent);
	}

	public TerrainChunk(Vector2 vector2, GameObject chunkParent)
	{
		this.ChunkPosition = TerrainGeneration.CastVector2ToInt(vector2);
		this.BlockIDs = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];
		this.blockIDsBG = new byte[GlobalVariables.WorldData.ChunkWidth, GlobalVariables.WorldData.ChunkHeight];
		this.drops = new List<Drop>();
		this.ChunkObject = BuildAllChunkLayers(chunkParent);
	}

	/// <summary>
	/// Creates Chunk - Bg / Collision / - tilemaps
	/// </summary>
	/// <returns></returns>
	private GameObject BuildAllChunkLayers(GameObject chunkParent)
	{
		GameObject chunkObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y}");
		chunkObject.tag = "Chunk";
		chunkObject.transform.SetParent(chunkParent.transform);
		chunkObject.transform.position = new Vector3(ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth, ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0f);

		ChunkTileMap = chunkObject.AddComponent<Tilemap>();
		ChunkTileMapRenderer = chunkObject.AddComponent<TilemapRenderer>();
		ChunkTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

		BackgroundObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y} background");
		BackgroundObject.transform.SetParent(ChunkTileMap.transform);
		BackgroundObject.transform.position = new Vector3(ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth, ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0.001f);
		BackgroundTilemap = BackgroundObject.AddComponent<Tilemap>();
		BackgroundObject.AddComponent<TilemapRenderer>();

		CollisionObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y} collision");
		CollisionObject.tag = "Terrain";
		CollisionObject.transform.SetParent(ChunkTileMap.transform);
		CollisionObject.transform.position = new Vector3(ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth, ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight, 0f);
		CollisionTileMap = CollisionObject.AddComponent<Tilemap>();
		ChunkTileMapCollider = CollisionObject.AddComponent<TilemapCollider2D>();
		CollisionTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);


		DropObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y} drops");
		DropObject.transform.SetParent(ChunkTileMap.transform);
		InsertDrops();

		return chunkObject;
	}

	/// <summary>
	/// Add the ids of the blocks to the blockIDs array
	/// </summary>
	/// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
	/// <param name="biomindex">Index of the biom of the chunk</param>
	public void GenerateChunk(float[] noisemap, float[,] caveNoisepmap, byte[,] oreNoiseMap, int[,] biomNoiseMap) {
		float caveSize = GlobalVariables.WorldData.InitCaveSize;
		if (chunkPosition.y < 0) {
			caveSize = GlobalVariables.WorldData.InitCaveSize - chunkPosition.y * GlobalVariables.WorldData.ChunkHeight * 0.001f;
		} 
		else
		if (chunkPosition.y > 0) {
			caveSize = GlobalVariables.WorldData.InitCaveSize + chunkPosition.y * GlobalVariables.WorldData.ChunkHeight * 0.001f;
		}

		if (caveSize > 0) {
			caveSize = 0;
		}

		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			AnimationCurve heightCurve = new AnimationCurve(GlobalVariables.WorldData.Heightcurve.keys);
			int positionHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[x]) * GlobalVariables.WorldData.HeightMultiplier) + 1;

			for (int y = GlobalVariables.WorldData.ChunkHeight - 1; y >= 0; y--) {
				Biom biom = GlobalVariables.WorldData.Biom[biomNoiseMap[x, y]];
				if (y + chunkPosition.y * GlobalVariables.WorldData.ChunkHeight < positionHeight) {
					if (caveNoisepmap[x, y] > caveSize) {
						if (caveNoisepmap[x, y] < caveSize + GlobalVariables.WorldData.StoneSize) {
							blockIDs[x, y] = biom.StoneBlockId;
						} else {
							foreach (RegionData region in biom.Regions) {
								if (region.RegionRange <= positionHeight - (y + chunkPosition.y * GlobalVariables.WorldData.ChunkHeight)) {
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
						if (regionBG.RegionRange <= positionHeight - (y + chunkPosition.y * GlobalVariables.WorldData.ChunkHeight)) {
							BlockIDsBG[x, y] = regionBG.BlockID;
						}
					}
				}
				//Place Trees.
				if (x % 5 == 0 && chunkPosition.y == 0)
				{
					//	//try to spawn a Tree
					GenerateTrees(x, positionHeight, biom.Index);
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
				PlaceTile(x, y, GlobalVariables.WorldData.Blocks[BlockIDs[x, y]].Tile);
				PlaceTileInBG(x, y, GlobalVariables.WorldData.Blocks[BlockIDsBG[x, y]].Tile);
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
					PlaceTile(x, y, GlobalVariables.WorldData.Blocks[BlockIDs[x, y]].Tile);
					if (init)
						PlaceTileInBG(x, y, GlobalVariables.WorldData.Blocks[BlockIDsBG[x, y]].Tile);
				}
			}
		}
	}

	/// <summary>
	/// Lambda expression for shortening reasons
	/// </summary>
	private void PlaceTile(int x, int y, TileBase tile) => ChunkTileMap.SetTile(new Vector3Int(x, y, 0), tile);

	private void PlaceTileInBG(int x, int y, TileBase tile) => BackgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);

	public void BuildCollisions()
	{
		collisionTileMap.ClearAllTiles();
		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++)
		{
			for (int y = 0; y < GlobalVariables.WorldData.ChunkHeight; y++)
			{
				int worldX = x + ChunkPosition.x * GlobalVariables.WorldData.ChunkWidth;
				int worldY = y + ChunkPosition.y * GlobalVariables.WorldData.ChunkHeight;
				if (BlockIDs[x, y] != 0 &&
					(GlobalVariables.WorldData.GetBlockFormCoordinate(worldX + 1, worldY) == 0 ||
					GlobalVariables.WorldData.GetBlockFormCoordinate(worldX, worldY + 1) == 0 ||
					GlobalVariables.WorldData.GetBlockFormCoordinate(worldX - 1, worldY) == 0 ||
					GlobalVariables.WorldData.GetBlockFormCoordinate(worldX, worldY - 1) == 0))
				{
					CollisionTileMap.SetTile(new Vector3Int(x, y, 0), GlobalVariables.WorldData.GetBlockbyId(1).Tile);
				}
			}
		}
	}

	/// <summary>
	/// Removes the block out of the tilemap
	/// </summary>
	public void DeleteBlock(Vector3Int coordinate)
	{
		if (BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * chunkPosition.x), (coordinate.y - GlobalVariables.WorldData.ChunkHeight * chunkPosition.y)] == 0) return;

		InstantiateDrop(coordinate);
		ChunkTileMap.SetTile(new Vector3Int(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x, coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y, 0), null);
		BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y] = 0;
		GlobalVariables.WorldData.UpdateCollisionsAt(coordinate);
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
	}

	/// <summary>
	/// TODO: Move to BlockInteraction
	/// Creating Drop + rigidbody and other Components
	/// </summary>
	public void InstantiateDrop(Vector3Int coordinate)
	{
		Drop d = new Drop();
		d.DropID = GlobalVariables.WorldData.Blocks[BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y]].Item1;
		d.DropName = GlobalVariables.WorldData.Blocks[BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y]].Name;
		d.DropObject = new GameObject($"Drop {d.DropID}");
		d.DropObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		d.DropObject.AddComponent<SpriteRenderer>();
		d.DropObject.GetComponent<SpriteRenderer>().sprite = GlobalVariables.WorldData.Blocks[BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * chunkPosition.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y]].Sprite;
		Vector3 c = coordinate;
		c.y = coordinate.y + 0.5f;
		c.x = coordinate.x + 0.5f;
		d.DropObject.transform.SetPositionAndRotation(c, new Quaternion());
		d.DropObject.AddComponent<Rigidbody2D>();
		d.DropObject.GetComponent<Rigidbody2D>().gravityScale = 20;
		d.DropObject.AddComponent<BoxCollider2D>();
		d.DropObject.layer = LayerMask.NameToLayer("Drops");
		d.Anzahl = 1;
		Drops.Add(d);
		InsertDrops();
		d.DropObject.transform.SetParent(DropObject.transform);
	}
	/// <summary>
	/// Creates the Gameobject out of the Drops list
	/// </summary>
	public void InsertDrops()
	{
		for (int x = 0; x < drops?.Count; x++)
		{
			for (int y = 0; y < drops?.Count; y++)
			{
				if (drops.Count > 1 && x != y)
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
		if (GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].DropObject.transform.position).x + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].DropObject.transform.position).x &&
			GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].DropObject.transform.position).x - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].DropObject.transform.position).x &&
			GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].DropObject.transform.position).y + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].DropObject.transform.position).y &&
			GlobalVariables.WorldData.Grid.WorldToCell(Drops[x].DropObject.transform.position).y - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[y].DropObject.transform.position).y &&
			Drops[x].DropObject.GetComponent<SpriteRenderer>().sprite.Equals(Drops[y].DropObject.GetComponent<SpriteRenderer>().sprite))
		{
			Drops[x].Anzahl++;
			RemoveDropfromView(Drops[y]);
			DropObject.SetActive(true);
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
			if (x + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].DropObject.transform.position).x &&
				x - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].DropObject.transform.position).x &&
				y + dropgrouprange > GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].DropObject.transform.position).y &&
				y - dropgrouprange < GlobalVariables.WorldData.Grid.WorldToCell(Drops[i].DropObject.transform.position).y)
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
		GameObject.Destroy(removable.DropObject);
	}

	/// <summary>
	/// Chunks are euqal if they have the same <see cref="chunkPosition"/> Vector
	/// </summary>
	/// <param name="obj">The other Object</param>
	/// <returns>True if the otherChunk is the same</returns>
	public new bool Equals(object obj)
	{
		return obj is TerrainChunk other && chunkPosition.Equals(other.chunkPosition);
	}

	public void GenerateTrees(int x, int y, int biom) {
		//Chunk = 32 in der width.
		//Trees ben�tigen 5 Bl�cke in der width bis der n�chste BAum spawnen kann
		//[Funktioniert, aber ned sch�n]

		if (new System.Random(chunkPosition.x * GlobalVariables.WorldData.ChunkWidth + x).Next(1, 5) == 4 && x > GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0) && x < (32 - GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0))) {
			//int rando = new System.Random(chunkPosition.x * GlobalVariables.WorldData.ChunkWidth + x).Next(5, 10);
			//for (int i = 0;i<rando;i++)
			//BlockIDsBG[x, y+i] = world.Strukturen[0].blocks[2,5];


			for (int z = 0; z < GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0); z++) {
				for (int q = 0; q < GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(1); q++) {
                    try { 
						if (BlockIDsBG[x + z - GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0) / 2, y + q] == 0)
							BlockIDsBG[x + z - GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks.GetLength(0) / 2, y + q] = GlobalVariables.Assets.Structures[GlobalVariables.WorldData.Biom[biom].Structures[0]].blocks[z, q];
					}catch { }
				}
			}
			//int breite=0;
			//for(int b= rando+2; b > 4; b--)
			//{
			//    for(int o = -breite; o <= breite; o++)
			//    {
			//        if(BlockIDsBG[x + o, y + rando]==0)
			//        BlockIDsBG[x+o, y+b] = 17;
			//    }
			//    breite++;
			//}
		}
	}



	public class ChunkList : List<Chunk>, INetworkSerializable
	{
		public void NetworkSerialize(NetworkSerializer serializer)
		{
			for (int i = 0; i < this.Count; i++)
			{
				Chunk chunk = this[i];
				serializer.Serialize(ref chunk.chunkPositionWorldSpace);
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

		public struct Chunk
		{
			public byte[,] blocks;
			public byte[,] bgBlocks;
			public Drop[] drops;
			public Vector2 chunkPositionWorldSpace;

			/// <summary>
			/// Transfers TerrainChunk to Chunk struct for Multiplayer worldsharing
			/// </summary>
			/// <param name="chunk"></param>
			/// <returns></returns>
			public static Chunk TransferBlocksToChunk(TerrainChunk chunk)
			{
				return new Chunk()
				{
					blocks = chunk.BlockIDs,
					bgBlocks = chunk.BlockIDsBG,
					drops = chunk.Drops.ToArray(),
					chunkPositionWorldSpace = new Vector2(chunk.ChunkPosition.x, chunk.ChunkPosition.y)
				};
			}
			/// <summary>
			/// Transfers Chunk struct to TerrainChunk Object for Multiplayer Worldsharing<br></br>
			/// TODO: Drops
			/// </summary>
			/// <param name="chunk"></param>
			/// <param name="GlobalVariables.WorldData"></param>
			/// <param name="chunkParent"></param>
			/// <returns></returns>
			public static TerrainChunk TransferChunkToBlocks(Chunk chunk, GameObject chunkParent)
			{
				return new TerrainChunk(new Vector2(chunk.chunkPositionWorldSpace.x, chunk.chunkPositionWorldSpace.y), chunkParent);
			}

			
		}
	}
}