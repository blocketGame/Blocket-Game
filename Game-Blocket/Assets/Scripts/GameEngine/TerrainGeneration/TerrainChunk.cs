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
    private Tilemap collisionTileMap;

    private Tilemap backgroundTilemap;

    private GameObject backgroundObject;
    [SerializeField]
    private TilemapRenderer chunkTileMapRenderer;
    [SerializeField]
    private GameObject collisionObject;
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
    public Tilemap BackgroundTilemap { get => backgroundTilemap; set => backgroundTilemap = value; }
    public GameObject BackgroundObject { get => backgroundObject; set => backgroundObject = value; }


    public TerrainChunk(int chunkID, World_Data world, GameObject chunkParent, GameObject chunkObject)
    {
        this.ChunkID = chunkID;
        this.World = world;
        this.BlockIDs = new byte[world.ChunkWidth, world.ChunkHeight];
        this.ChunkObject = BuildAllChunkLayers(chunkParent, chunkObject);
    }

    /// <summary>
    /// Creates Chunk - Bg / Collision / - tilemaps
    /// </summary>
    /// <returns></returns>
    private GameObject BuildAllChunkLayers(GameObject chunkParent, GameObject chunkObject)
    {
        chunkObject = new GameObject($"Chunk {ChunkID}");
        chunkObject.transform.SetParent(chunkParent.transform);
        chunkObject.transform.position = new Vector3(ChunkID * World.ChunkWidth, 0f, 0f);

        ChunkTileMap = chunkObject.AddComponent<Tilemap>();
        ChunkTileMapRenderer = chunkObject.AddComponent<TilemapRenderer>();
        ChunkTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        BackgroundObject = new GameObject($"Chunk {ChunkID} background");
        BackgroundObject.transform.SetParent(ChunkTileMap.transform);
        BackgroundObject.transform.position = new Vector3(ChunkID * World.ChunkWidth, 0f, 0f);
        BackgroundTilemap = BackgroundObject.AddComponent<Tilemap>();

        CollisionObject = new GameObject($"Chunk {ChunkID} collision");
        CollisionObject.transform.SetParent(ChunkTileMap.transform);
        CollisionObject.transform.position = new Vector3(ChunkID * World.ChunkWidth, 0f, 0f);
        CollisionTileMap = CollisionObject.AddComponent<Tilemap>();
        ChunkTileMapCollider = CollisionObject.AddComponent<TilemapCollider2D>();
        CollisionTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        return chunkObject;
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
                //Wird tats�chlich ausgef�hrt
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
    /// <summary>
    /// Is used to load Chunks in and out
    /// </summary>
    /// <param name="value">represents the state the chunk switches to</param>
    public void SetChunkState(bool value)
    {
        BackgroundObject.SetActive(value);
        ChunkObject.SetActive(value);
    }

}