using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
* @Author : Thomas Boigner / Cse19455
*/
public class TerrainChunk
{
    [SerializeField]
    private World_Data world;
    [SerializeField]
    private int chunkID;
    [SerializeField]
    private byte[,] blockIDs;
    [SerializeField]
    private GameObject chunkObject;
    [SerializeField]
    private Tilemap chunkTileMap;
    [SerializeField]
    private TilemapRenderer chunkTileMapRenderer;
    [SerializeField]
    private GameObject collisionObject;
    [SerializeField]
    private Tilemap collisionTileMap;
    [SerializeField]
    private TilemapCollider2D chunkTileMapCollider;
    [SerializeField]
    private Biom biom;
    [SerializeField]
    private int biomNr;
    [SerializeField]
    private Drop[] drops;

    public World_Data World { get => world; set => world = value; }
    public int ChunkID { get => chunkID; set => chunkID = value; }
    public byte[,] BlockIDs { get => blockIDs; set => blockIDs = value; }
    public Tilemap ChunkTileMap { get => chunkTileMap; set => chunkTileMap = value; }
    public TilemapRenderer ChunkTileMapRenderer { get => chunkTileMapRenderer; set => chunkTileMapRenderer = value; }
    public GameObject CollisionObject { get => collisionObject; set => collisionObject = value; }
    public Tilemap CollisionTileMap { get => collisionTileMap; set => collisionTileMap = value; }
    public TilemapCollider2D ChunkTileMapCollider { get => chunkTileMapCollider; set => chunkTileMapCollider = value; }
    public Biom Biom { get => biom; set => biom = value; }
    public int BiomNr { get => biomNr; set => biomNr = value; }
    public Drop[] Drops { get => drops; set => drops = value; }
    public GameObject ChunkObject { get => chunkObject; set => chunkObject = value; }

    public TerrainChunk(int chunkID, World_Data world, GameObject chunkParent, GameObject chunkObject)
    {
        this.ChunkID = chunkID;
        this.World = world;
        this.BlockIDs = new byte[world.ChunkWidth, world.ChunkHeight];

        chunkObject = new GameObject($"Chunk {chunkID}");
        chunkObject.transform.SetParent(chunkParent.transform);
        chunkObject.transform.position = new Vector3(chunkID * world.ChunkWidth, 0f, 0f);

        ChunkTileMap = chunkObject.AddComponent<Tilemap>();
        ChunkTileMapRenderer = chunkObject.AddComponent<TilemapRenderer>();
        ChunkTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        CollisionObject = new GameObject($"Chunk {chunkID} collision");
        CollisionObject.transform.SetParent(ChunkTileMap.transform);
        CollisionObject.transform.position = new Vector3(chunkID * world.ChunkWidth, 0f, 0f);

        CollisionTileMap = CollisionObject.AddComponent<Tilemap>();
        ChunkTileMapCollider = CollisionObject.AddComponent<TilemapCollider2D>();
        CollisionTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        this.ChunkObject = chunkObject;
    }

    /// <summary>
    /// Add the ids of the blocks to the blockIDs array
    /// </summary>
    /// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
    /// <param name="biomindex">Index of the biom of the chunk</param>
    public void GenerateChunk(float[] noisemap,int biomindex)
    {
        Biom = World.Biom[biomindex];
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            int positionHeight = Mathf.FloorToInt(World.Heightcurve.Evaluate(noisemap[x])*World.HeightMultiplier) + World.ChunkGroundLevel;
            int heightvalue = 0;
            int blockIDpos = 0;
            for (int y = positionHeight-1; y > 0; y--)
            {
                    //Debug.Log("Layerhaeight :"+ world.biom[biomindex].regions[blockIDpos].layerheight + " Heightvalue"+heightvalue);
                    if(heightvalue == World.Biom[biomindex].Regions[blockIDpos].RegionRange)
                    {
                        blockIDpos++;
                        heightvalue = 0;
                    }
                    else
                    heightvalue++;
                    BlockIDs[x, y] = World.Biom[biomindex].Regions[blockIDpos].BlockID;
                    PlaceTile(x, y, World.Blocks[BlockIDs[x, y]].Tile);
            }
        }
        PlaceTiles(biomindex);
    }

    /// <summary>
    /// places the tiles in the Tilemap according to the blockIDs array
    /// </summary>
    /// <param name="biomindex">Index of the biom of the chunk</param>
    public void PlaceTiles(int biomindex)
    {
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            int heightvalue = 0;
            int blockIDpos = World.Biom[biomindex].Regions.Length-1;
            for (int y = World.ChunkHeight-1; y > 0; y--)
            { 
                if (BlockIDs[x, y] != 0)
                {
                    if (heightvalue == World.Biom[biomindex].Regions[blockIDpos].RegionRange )
                    {
                        blockIDpos--;
                        heightvalue = 0;
                    }
                    else
                        heightvalue++;
                   PlaceTile(x, y, World.Blocks[BlockIDs[x,y]].Tile);
                }
            }
        }
    }

    /// <summary>
    /// Lambda expression for shortening reasons
    /// </summary>
    private void PlaceTile(int x, int y, TileBase tile) => ChunkTileMap.SetTile(new Vector3Int(x, y, 0), tile);

    public void BuildCollisions()
    {
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            for (int y = 0; y < World.ChunkHeight; y++)
            {
                int worldX = x + ChunkID * World.ChunkWidth;
                //Wird tatsächlich ausgeführt
                if (BlockIDs[x, y] != 0 &&
                    (World.getBlockFormCoordinate(worldX + 1, y) == 0 ||
                    World.getBlockFormCoordinate(worldX, y + 1) == 0 ||
                    World.getBlockFormCoordinate(worldX - 1, y) == 0 ||
                    World.getBlockFormCoordinate(worldX, y - 1) == 0))
                {
                    CollisionTileMap.SetTile(new Vector3Int(x, y, 0), World.getBlockbyId(1).Tile);
                }
            }
        }
    }

    /*
    //public variables
    public int width, height;
    public float maxsmoothness, seed;
    public bool seedrandomness;

    [Header("Tile Settings")]
    public TileBase groundTile, topTile;
    public Tilemap groundTilemap;

    //private variables
    private int[,] map;

    //delegates
    public delegate void loopContent(int x, int y);


    public void Start()
    {
        Generation();
    }

    public void Update()
    {
        //Generation();
    }

    private void Generation()
    {
        groundTilemap.ClearAllTiles();
        map = GenerateArray(width, height);
        map = TerrainGeneration(map);
        RenderMap(map);
    }

    private int[,] GenerateArray(int width, int height)
    {
        int[,] map = new int[width, height];
        loopXY((x, y) => { map[x, y] = 0; });
        return map;
    }

    private int[,] TerrainGeneration(int[,] map)
    {
        if (seedrandomness == true)
            seed = UnityEngine.Random.Range(0f, 10000f);

        for (int x = 0; x < width; x++)
        {
            int perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / maxsmoothness, seed) * height);
            for (int y = 0; y < perlinHeight; y++)
                map[x, y] = 1;
        }

        return map;
    }

    private int[,] RenderMap(int[,] map)
    {
        loopXY(
            (x, y) =>
            {
                if (map[x, y] == 1)
                    PlaceTile(x, y, groundTile);
                else if (y != 0 && map[x, y - 1] == 1)
                    PlaceTile(x, y, topTile);
            }
            );
        return map;
    }
    */
    /*
     * Automatition functions
     */
    /*
    private void loopXY(loopContent function)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                function(x, y);
    }


    private void PlaceTile(int x, int y, TileBase tile) => groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
    */
}
