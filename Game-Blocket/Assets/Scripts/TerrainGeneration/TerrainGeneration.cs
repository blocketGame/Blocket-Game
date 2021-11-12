using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using System;
using System.Threading;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class TerrainGeneration : MonoBehaviour
{
    [SerializeField]
    private WorldData world;
    [SerializeField]
    private List<TerrainChunk> chunksVisibleLastUpdate;
    [SerializeField]
    private GameObject chunkParent;

    private Transform playerPosition;
    private Queue<TerrainChunk> chunkCollisionQueue = new Queue<TerrainChunk>();
    private Queue<TerrainChunk> chunkTileInitializationQueue = new Queue<TerrainChunk>();
    private Vector3 playerChunk;

    //------------------------------------------------------- Properties ------------------------------------------------------------------

    public WorldData World { get => world; set => world = value; }
    public List<TerrainChunk> ChunksVisibleLastUpdate { get => chunksVisibleLastUpdate; set => chunksVisibleLastUpdate = value; }
    public GameObject ChunkParent { get => chunkParent; set => chunkParent = value; }
    public Transform PlayerPosition { get => playerPosition; set => playerPosition = value; }
    public Queue<TerrainChunk> ChunkCollisionQueue { get => chunkCollisionQueue; set => chunkCollisionQueue = value; }

    public static System.Random prng;

    public void Start()
    {
        ChunksVisibleLastUpdate = new List<TerrainChunk>();
        PlayerPosition = world.Player.transform;
        world.PutBlocksIntoTxt();
        world.putBiomsIntoTxt();
        prng = new System.Random(world.Seed);
        UpdateChunks();
    }

    public void Update()
    {
        Vector3 distance = playerPosition.position - playerChunk;
        if (distance.x <= 0 || distance.x >= world.ChunkWidth || distance.y <= 0 || distance.y >= world.ChunkHeight)
            UpdateChunks();
        if (ChunkCollisionQueue.Count > 0)
        {
            lock (ChunkCollisionQueue)
            {
                foreach (TerrainChunk tc in ChunkCollisionQueue)
                {
                    tc.BuildCollisions();
                }
                ChunkCollisionQueue.Clear();
            }
        }

        if (chunkTileInitializationQueue.Count > 0)
        {
            lock (chunkTileInitializationQueue)
            {
                foreach(TerrainChunk terrainChunk in chunkTileInitializationQueue)
                {
                    terrainChunk.PlaceAllTiles();
                }
                chunkTileInitializationQueue.Clear();
            }
        }

    }

    public void UpdateChunks()
    {
        TerrainChunk currentChunk = world.GetChunkFromCoordinate(playerPosition.position.x, playerPosition.position.y);
        if (currentChunk != null)
        {
            playerChunk = currentChunk.ChunkPositionWorldSpace;
        }

        foreach (TerrainChunk t in ChunksVisibleLastUpdate)
        {
            t?.SetChunkState(false);
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

    /// <summary>
    ///     Generates Chunk From Noisemap without any extra consideration
    /// </summary>
    private void BuildChunk(Vector2Int position)
    {
        TerrainChunk chunk = new TerrainChunk(position, World, ChunkParent, null);
        List<Biom> bioms;
        if (position.y > -20)
            bioms = world.getBiomsByType(Biomtype.OVERWORLD);
        else
            bioms = world.getBiomsByType(Biomtype.UNDERGROUND);

        float[] noisemap;
        if (world.Noisemaps.ContainsKey(position.x))
        {
            noisemap = world.Noisemaps[position.x];
        }
        else
        {
            noisemap = NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth);
            world.Noisemaps.Add(position.x, noisemap);
        }
        ThreadStart threadStart = delegate
        {
            chunk.GenerateChunk(
                  noisemap,
                  NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
                  NoiseGenerator.GenerateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), bioms));
            World.Chunks[position] = chunk;
            lock (chunksVisibleLastUpdate)
            {
                chunksVisibleLastUpdate.Add(chunk);
            }
            lock (chunkCollisionQueue)
            {
                chunkCollisionQueue.Enqueue(chunk);
            }
            lock (chunkTileInitializationQueue)
            { 
                chunkTileInitializationQueue.Enqueue(chunk);
            }
           
        };
        new Thread(threadStart).Start();
    }
    /*
    public void RequestBiomNoisemapData(Vector3 position, List<Biom> bioms, Action<float[,]> callback)
    {
        ThreadStart threadStart = delegate
        {
            float[,] biomNoiseMap = NoiseGenerator.GenerateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), bioms);
            lock (biomNoiseMapThreadInfoQueue)
            {
                biomNoiseMapThreadInfoQueue.Enqueue(new MapThreadInfo<float[,]>(callback, biomNoiseMap));
            }
        };
        new Thread(threadStart).Start();
    }

    public void OnBiomNoiseMapReceived(float[,] biomNoiseMap)
    {
        float[] noisemap;
        if (world.Noisemaps.ContainsKey(position.x))
        {
            noisemap = world.Noisemaps[position.x];
        }
        else
        {
            noisemap = NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth);
            world.Noisemaps.Add(position.x, noisemap);
        }

        chunk.GenerateChunk(
              noisemap,
              NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
              NoiseGenerator.GenerateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, world.OffsetY + position.y * World.ChunkHeight), bioms));
        World.Chunks[position] = chunk;
        ChunksVisibleLastUpdate.Add(chunk);
        ChunkCollisionQueue.Enqueue(chunk);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }*/
}