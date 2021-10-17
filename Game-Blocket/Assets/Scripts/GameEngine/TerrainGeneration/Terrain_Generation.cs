using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class Terrain_Generation : MonoBehaviour
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

    public static System.Random prng;

    public void Start()
    {
        ChunksVisibleLastUpdate = new List<TerrainChunk>();
        PlayerPosition = World.Player.transform;
        world.putBlocksIntoTxt();
        world.putBiomsIntoTxt();
        prng = new System.Random(world.Seed);
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
        foreach (TerrainChunk t in ChunksVisibleLastUpdate)
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

        for (int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++)
        {
            for (int yOffset = -World.ChunkDistance; yOffset < World.ChunkDistance; yOffset++)
            {
                Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
                if (!World.Chunks.ContainsKey(viewedChunkCoord))
                {
                    BuildChunk(viewedChunkCoord);
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
        //NOCH NICHT GANZ KORREKT
        /*
        * BIOMNRX && BIOMNRY
        * BIOME SOLLEN SICH NICHT NUR ÜBER DIE X ODER DIE Y VERBREITEN SONDERN ÜBER BEIDES GLEICHZEITIG
        * BIOMNRX + BIOMNRY = SIZE 
        * DANN WIRD EIN NEUES BIOM REROLLED
        * Root Biom = 1,1
        */

        //Derzeit absolut random
        /* float[,] biomnoisemap = NoiseGenerator.generateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + viewedChunkCoord.x * World.ChunkWidth, world.OffsetY + viewedChunkCoord.y * World.ChunkHeight), new List<Biom>(world.Biom));
         foreach(float f in biomnoisemap)
         {
             Debug.Log(f);
         }
         Debug.Break();*/
        BuildChunk(viewedChunkCoord);
    }

    /// <summary>
    ///     Generates Chunk From Noisemap without any extra consideration
    /// </summary>
    private void BuildChunk(Vector2Int position)
    {
        TerrainChunk chunk = new TerrainChunk(position, World, ChunkParent, null);
        chunk.GenerateChunk(
            NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth),
            NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
            NoiseGenerator.generateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), new List<Biom>(world.Biom)));
        World.Chunks[position] = chunk;
        ChunksVisibleLastUpdate.Add(chunk);
        ChunkCollisionQueue.Enqueue(chunk);
    }

}