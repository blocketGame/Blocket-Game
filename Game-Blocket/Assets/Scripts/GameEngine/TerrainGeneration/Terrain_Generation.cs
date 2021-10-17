using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class Terrain_Generation: MonoBehaviour
{
    [SerializeField]
    private World_Data world;
    [SerializeField]
    private List<TerrainChunk> chunksVisibleLastUpdate;
    [SerializeField]
    private GameObject chunkParent;

    private Transform playerPosition;
    private Queue<TerrainChunk> chunkCollisionQueue = new Queue<TerrainChunk>();

    //------------------------------------------------------- Properties ------------------------------------------------------------------

    public World_Data World { get => world; set => world = value; }
    public List<TerrainChunk> ChunksVisibleLastUpdate { get => chunksVisibleLastUpdate; set => chunksVisibleLastUpdate = value; }
    public GameObject ChunkParent { get => chunkParent; set => chunkParent = value; }
    public Transform PlayerPosition { get => playerPosition; set => playerPosition = value; }
    public Queue<TerrainChunk> ChunkCollisionQueue { get => chunkCollisionQueue; set => chunkCollisionQueue = value; }

    public void Start()
    {
        ChunksVisibleLastUpdate = new List<TerrainChunk>();
        PlayerPosition = World.Player.transform;
        world.putBlocksIntoTxt();
        world.putBiomsIntoTxt();
    }

    public void Update()
    {
        UpdateChunks();
        foreach (TerrainChunk tc in ChunkCollisionQueue)
        {
            tc.BuildCollisions(false);
        }
        ChunkCollisionQueue.Clear();

    }

    public void UpdateChunks()
    {
        foreach(TerrainChunk t in ChunksVisibleLastUpdate)
        {
            t.SetChunkState(false);
        }
        ChunksVisibleLastUpdate.Clear();
        CheckChunksAroundPlayer();
    }

    /// <summary>
    /// Activates and deactivates Chunks
    /// </summary>
    public void CheckChunksAroundPlayer()
    {
        Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(PlayerPosition.position.x / World.ChunkWidth), Mathf.RoundToInt(PlayerPosition.position.y / World.ChunkHeight));

        for(int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++)
        {
            for (int yOffset = -World.ChunkDistance; yOffset<World.ChunkDistance; yOffset++)
            {
                Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
                if (!World.Chunks.ContainsKey(viewedChunkCoord))
                {
                    BiomsizeCheck(viewedChunkCoord);
                }
                else if (World.Chunks.ContainsKey(viewedChunkCoord))
                {
                    World.Chunks[viewedChunkCoord].SetChunkState(true);
                    ChunksVisibleLastUpdate.Add(World.Chunks[viewedChunkCoord]);
                }
            }
        }
    }

    /// <summary>
    /// Checks whether or not a Biom is complete 
    /// (Biomnr/Biomsize)
    /// </summary>
    private void BiomsizeCheck(Vector2Int viewedChunkCoord)
    {
        if (viewedChunkCoord.x == -1)
        {
            BuildChunk(viewedChunkCoord);
            return;
        }
        else if (World.Chunks.ContainsKey(new Vector2Int(viewedChunkCoord.x - 1, 0)) && World.Chunks[new Vector2Int(viewedChunkCoord.x - 1, 0)].BiomNr < World.Chunks[new Vector2Int(viewedChunkCoord.x - 1, 0)].Biom.Size)
        {
                BuildChunk(viewedChunkCoord, World.Chunks[new Vector2Int(viewedChunkCoord.x - 1, 0)].Biom.Index, World.Chunks[new Vector2Int(viewedChunkCoord.x - 1, 0)].BiomNr);
        }
        else if (World.Chunks.ContainsKey(new Vector2Int(viewedChunkCoord.x + 1, 0)) && World.Chunks[new Vector2Int(viewedChunkCoord.x + 1, 0)].BiomNr < World.Chunks[new Vector2Int(viewedChunkCoord.x + 1, 0)].Biom.Size)
        {
                BuildChunk(viewedChunkCoord, World.Chunks[new Vector2Int(viewedChunkCoord.x + 1, 0)].Biom.Index, World.Chunks[new Vector2Int(viewedChunkCoord.x + 1, 0)].BiomNr);
        }
        else
            BuildChunk(viewedChunkCoord);
    }
    
    /// <summary>
    ///     Generates Chunk From Noisemap without any extra consideration
    /// </summary>
    private void BuildChunk(Vector2Int position)
    { 
        TerrainChunk chunk = new TerrainChunk(position, World, ChunkParent,null);
        int Biom = new System.Random(World.Seed+World.ChunkWidth*position.x).Next(0,World.Biom.Length);
        chunk.GenerateChunk(
            NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth),
            NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
            Biom);
        chunk.BiomNr = 1;
        World.Chunks[position] = chunk;
        ChunksVisibleLastUpdate.Add(chunk);
        ChunkCollisionQueue.Enqueue(chunk);
    }
    /// <summary>
    ///     Generates Chunk From Noisemap with Biom (min) size in consideration
    /// </summary>
    /// <param name="Biomindex">index that specifice which Biom to use</param>
    /// <param name="Biomnr">iterative index for the new chunk</param>
    private void BuildChunk(Vector2Int position,int Biomindex,int Biomnr)
    {
        TerrainChunk chunk = new TerrainChunk(position, World, ChunkParent,null);
        chunk.GenerateChunk(
            NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth),
            NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
            Biomindex);
        chunk.BiomNr = Biomnr+1;
        World.Chunks[position] = chunk;
        ChunksVisibleLastUpdate.Add(chunk);
        ChunkCollisionQueue.Enqueue(chunk);
    }
}