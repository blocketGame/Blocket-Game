using System.Collections.Generic;
using System.IO;

using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;

using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// <b>Author : Cse19455 / Thomas Boigner</b>
/// </summary>
public class WorldData : NetworkBehaviour
{
	#region Fields
	private NetworkDictionary<Vector2Int, TerrainChunk> chunks = new NetworkDictionary<Vector2Int, TerrainChunk>(new NetworkVariableSettings
	{ WritePermission = NetworkVariablePermission.OwnerOnly, ReadPermission = NetworkVariablePermission.Everyone });
	[SerializeField]
	private Biom[] biom;
	[SerializeField]
	private BlockData[] blocks;
	[SerializeField]
	private int chunkWidth;
	[SerializeField]
	private int chunkHeight;
	[SerializeField]
	private int chunkDistance;
	[SerializeField]
	private float caveSize;
	[SerializeField]
	private int seed;
	[SerializeField]
	private float scale;
	[SerializeField]
	private int octives;
	[SerializeField]
	[Range(0, 1)]
	private float persistance;
	[SerializeField]
	private float lacurinarity;
	[SerializeField]
	private float offsetX;
	[SerializeField]
	private float offsetY;
	[SerializeField]
	private int heightMultiplier;
	[SerializeField]
	private AnimationCurve heightcurve;
	[SerializeField]
	private Grid grid;
	[SerializeField]
	private float groupdistance;
	[SerializeField]
	private float pickUpDistance;
	#endregion

	#region Properties
	public float Persistance { get => persistance; set => persistance = value; }
	public float Lacurinarity { get => lacurinarity; set => lacurinarity = value; }
	public float OffsetX { get => offsetX; set => offsetX = value; }
	public float OffsetY { get => offsetX; set => offsetX = value; }
	public int HeightMultiplier { get => heightMultiplier; set => heightMultiplier = value; }
	public AnimationCurve Heightcurve { get => heightcurve; set => heightcurve = value; }
	public int Octives { get => octives; set => octives = value; }
	public float Scale { get => scale; set => scale = value; }
	public float CaveSize { get => caveSize; set => caveSize = value; }
	public int Seed { get => seed; set => seed = value; }
	public int ChunkDistance { get => chunkDistance; set => chunkDistance = value; }
	public int ChunkHeight { get => chunkHeight; set => chunkHeight = value; }
	public int ChunkWidth { get => chunkWidth; set => chunkWidth = value; }
	public BlockData[] Blocks { get => blocks; set => blocks = value; }
	public Biom[] Biom
	{
		get => biom; set => biom = value;
	}
	public NetworkDictionary<Vector2Int, TerrainChunk> Chunks { get => chunks; set => chunks = value; }

	public Grid Grid { get => grid; set => grid = value; }
	public float Groupdistance { get => groupdistance; set => groupdistance = value; }
	public float PickUpDistance { get => pickUpDistance; set => pickUpDistance = value; }

    #endregion

    public void Update()
    {
		Debug.Log(chunks.Count);
    }

    /// <summary>
    /// Stores this class to <see cref="GlobalVariables"/>
    /// </summary>
    public void Awake() {
		GlobalVariables.WorldData = this;
	}

	/// <summary>
	/// Returns the chunk the given coordinate is in
	/// </summary>
	/// <param name="x">coordinate in a chunk</param>
	/// <returns></returns>
	public TerrainChunk GetChunkFromCoordinate(float x, float y)
	{
		Vector2Int chunkPosition = new Vector2Int(Mathf.FloorToInt(x / ChunkWidth), Mathf.FloorToInt(y / ChunkHeight));
		if (Chunks.ContainsKey(chunkPosition))
		{
			return Chunks[chunkPosition];
		}
		return null;
	}

	/// <summary>
	/// Returns the block on any coordinate
	/// </summary>
	/// <param name="x">x coordinate</param>
	/// <param name="y">y coordinate</param>
	/// <returns></returns>
	public byte GetBlockFormCoordinate(int x, int y)
	{
		TerrainChunk chunk = GetChunkFromCoordinate(x, y);
		if (chunk != null)
		{
			int chunkX = x - ChunkWidth * chunk.ChunkPosition.x;
			int chunkY = y - ChunkHeight * chunk.ChunkPosition.y;
			if (chunkX < ChunkWidth && chunkY < ChunkHeight)
			{
				return chunk.BlockIDs[chunkX, chunkY];
			}
		}
		return 1;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="coordinate"></param>
	public void UpdateCollisionsAt(Vector3Int coordinate)
	{
		TerrainChunk chunk = GetChunkFromCoordinate(coordinate.x, coordinate.y);

		int chunkX = coordinate.x - chunk.ChunkPosition.x * ChunkWidth;
		int chunkY = coordinate.y - chunk.ChunkPosition.y * ChunkHeight;

		chunk.CollisionTileMap.SetTile(new Vector3Int(chunkX, chunkY, 0), null);

		if (GetBlockFormCoordinate(coordinate.x, coordinate.y) != 0 &&
			(GetBlockFormCoordinate(coordinate.x + 1, coordinate.y) == 0 ||
			GetBlockFormCoordinate(coordinate.x, coordinate.y + 1) == 0 ||
			GetBlockFormCoordinate(coordinate.x - 1, coordinate.y) == 0 ||
			GetBlockFormCoordinate(coordinate.x, coordinate.y - 1) == 0))
		{
			chunk.CollisionTileMap.SetTile(new Vector3Int(chunkX, chunkY, 0), GetBlockbyId(1).Tile);
		}
	}

	/// <summary>
	/// returns the BlockData object of the index
	/// </summary>
	/// <param name="id">index of the block</param>
	/// <returns></returns>
	public BlockData GetBlockbyId(byte id)
	{
		foreach (BlockData bd in Blocks)
		{
			if (bd.BlockID == id)
			{
				return bd;
			}
		}
		return Blocks[0];
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public List<Biom> GetBiomsByType(Biomtype type) {
		List<Biom> biomlist = new List<Biom>();
		foreach(Biom b in biom) {
			if(b.Biomtype.Contains(type))
				biomlist.Add(b);
		}
		return biomlist;
	}

	/**
	public bool getBlocksFromTxt()
	{
		string[] lines = System.IO.File.ReadAllLines(@"Docs/Blocks.txt");
		if(lines[lines.Length-1].Equals("Changed : true"))
		{
			Debug.Log("HI I AM HERE");
			//Auslesen der Daten im Txt file In den Block array
			return true;
		}
		return false;
	}
	**/

	/// <summary>
	/// Method at wrong PLACE
	/// </summary>
	public void IgnoreDropCollision()
	{
		//foreach (TerrainChunk t in GlobalVariables.localGameVariables.terrainGeneration.ChunksVisibleLastUpdate)
		//    foreach (Drop d in t.Drops)
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Drops"), LayerMask.NameToLayer("Player"));
	}

	#region Filehandling

	/// <summary>
	/// Creates Blocks.txt file as documentation for the blocks array
	/// </summary>

	public void PutBlocksIntoTxt()
	{
		string writeContent = "# This File is considered as documentation tool for the Blocks and their Ids \n";
		for (int x = 0; x < blocks.Length; x++)
		{
			writeContent += "\n" +
				" ID : " + blocks[x].BlockID + "\n" +
				" Name : " + blocks[x].Name + "\n";
		}

		File.WriteAllText("Docs/Blocks.txt", writeContent);
	}

	public void PutBiomsIntoTxt()
	{
		string writeContent = "# This File is considered as documentation tool for the Bioms and their Indizes \n";
		for (int x = 0; x < biom.Length; x++)
		{
			writeContent += "\n" +
				" ID : " + biom[x].Index + "\n" +
				" BiomName : " + biom[x].BiomName + "\n";
			for (int y = 0; y < biom[x].Regions.Length; y++)
			{
				writeContent += "\n" +
				"\t ID : " + biom[x].Regions[y].BlockID + "\n" +
				"\t Range : " + biom[x].Regions[y].RegionRange + "\n";
			}

		}

		writeContent += "\n ---------------------------- Rules ---------------------------------- \n Range : -1 => Infinity";

		File.WriteAllText("Docs/Bioms.txt", writeContent);
	}

	#endregion
}

#region WorldDataAssets

[System.Serializable]
public struct OreData
{
	[SerializeField]
	private string name;
	[SerializeField]
	private float noiseValueFrom;
	[SerializeField]
	private float noiseValueTo;
	[SerializeField]
	private byte blockID;

	public string Name { get => name; set => name = value; }
	public float NoiseValueFrom { get => noiseValueFrom; set => noiseValueFrom = value; }
	public float NoiseValueTo { get => noiseValueTo; set => noiseValueTo = value; }
	public byte BlockID { get => blockID; set => blockID = value; }
}

[System.Serializable]
public struct RegionData
{
	[SerializeField]
	private int regionRange; //-1 => infinite range
	[SerializeField]
	private byte blockID;

	public int RegionRange { get => regionRange; set => regionRange = value; }
	public byte BlockID { get => blockID; set => blockID = value; }
}

[System.Serializable]
public struct BlockData
{
	#region Specification
	[SerializeField]
	private string _name;
	[SerializeField]
	private byte _blockID;

	public string Name { get => _name; set => _name = value; }
	public byte BlockID { get => _blockID; set => _blockID = value; }
	#endregion

	#region Graphics
	[SerializeField]
	private TileBase _tile;
	[SerializeField]
	private Sprite sprite;

	public TileBase Tile { get => _tile; set => _tile = value; }
	public Sprite Sprite { get => sprite; set => sprite = value; }
	#endregion

	#region Settings
	[SerializeField]
	private byte removeDuration;
	[SerializeField]
	private byte item;

	public byte RemoveDuration { get => removeDuration; set => removeDuration = value; }
	public byte Item1 { get => item; set => item = value; }
	#endregion
}
#endregion