using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class World_Data : MonoBehaviour
{


    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Terrain_Generation terraingeneration;
    [SerializeField]
    private Dictionary<int, TerrainChunk> chunks = new Dictionary<int, TerrainChunk>();
    [SerializeField]
    private Biom[] biom;
    [SerializeField]
    private BlockData[] blocks;
    [SerializeField]
    private int chunkWidth;
    [SerializeField]
    private int chunkHeight;
    [SerializeField]
    private int chunkGroundLevel;
    [SerializeField]
    private int chunkDistance;
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
    private int heightMultiplier;
    [SerializeField]
    private AnimationCurve heightcurve;


//----------------------------------------------- Properties ----------------------------------------------------------------------------

    public float Persistance { get => persistance; set => persistance = value; }
    public float Lacurinarity { get => lacurinarity; set => lacurinarity = value; }
    public float OffsetX { get => offsetX; set => offsetX = value; }
    public int HeightMultiplier { get => heightMultiplier; set => heightMultiplier = value; }
    public AnimationCurve Heightcurve { get => heightcurve; set => heightcurve = value; }
    public int Octives { get => octives; set => octives = value; }
    public float Scale { get => scale; set => scale = value; }
    public int Seed { get => seed; set => seed = value; }
    public int ChunkDistance { get => chunkDistance; set => chunkDistance = value; }
    public int ChunkGroundLevel { get => chunkGroundLevel; set => chunkGroundLevel = value; }
    public int ChunkHeight { get => chunkHeight; set => chunkHeight = value; }
    public int ChunkWidth { get => chunkWidth; set => chunkWidth = value; }
    public BlockData[] Blocks { get => blocks; set => blocks = value; }
    public Biom[] Biom { get => biom; set => biom = value; }
    public Dictionary<int, TerrainChunk> Chunks { get => chunks; set => chunks = value; }
    public Terrain_Generation Terraingeneration { get => terraingeneration; set => terraingeneration = value; }
    public GameObject Player { get => player; set => player = value; }



    /// <summary>
    /// Returns the chunk the given coordinate is in
    /// </summary>
    /// <param name="x">coordinate in a chunk</param>
    /// <returns></returns>
    public TerrainChunk GetChunkFromCoordinate(float x)
    {
        int chunkIndex = Mathf.FloorToInt(x / ChunkWidth);
        if (Chunks.ContainsKey(chunkIndex))
        {
            return Chunks[chunkIndex];
        }
        return null;
    }

    /// <summary>
    /// Returns the block on any coordinate
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns></returns>
    public byte getBlockFormCoordinate(int x, int y)
    {
        TerrainChunk chunk = GetChunkFromCoordinate(x);
       
        if (chunk != null)
        {
            int chunkX = x - ChunkWidth * chunk.ChunkID;
            return chunk.BlockIDs[chunkX, y];   
        }
        return 0;
    }

    /// <summary>
    /// returns the BlockData object of the index
    /// </summary>
    /// <param name="id">index of the block</param>
    /// <returns></returns>
    public BlockData getBlockbyId(byte id)
    {
        foreach(BlockData bd in Blocks)
        {
            if(bd.BlockID == id)
            {
                return bd;
            }
        }
        return Blocks[0];
    }


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

    /// <summary>
    /// Creates Blocks.txt file as documentation for the blocks array
    /// </summary>
    public void putBlocksIntoTxt()
    {
        string writeContent="# This File is considered as documentation tool for the Blocks and their Ids \n";
        for(int x =0; x < blocks.Length; x++)
        {
            writeContent += "\n" +
                " ID :" + blocks[x].BlockID + "\n" +
                " Name : " + blocks[x].Name +"\n";
        }

        writeContent += "\nChanged : false";

        File.WriteAllText("Docs/Blocks.txt", writeContent);
    }
}

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
    [SerializeField]
    private string _name;
    [SerializeField]
    private byte _blockID;
    [SerializeField]
    private TileBase _tile;
    [SerializeField]
    private Drop[] _drops;

    public string Name { get =>_name; set => _name = value; }
    public byte BlockID { get => _blockID; set => _blockID = value; }
    public TileBase Tile { get => _tile; set => _tile = value; }
    public Drop[] Drops { get => _drops; set => _drops = value; }
}


