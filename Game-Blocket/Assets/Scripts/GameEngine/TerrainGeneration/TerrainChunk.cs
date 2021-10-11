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
    private byte[,] blockIDsBG;
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
    private List<Drop> drops;

    private GameObject DROPS;

    //----------------------------------------------- Properties ----------------------------------------------------------------------------

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
    public List<Drop> Drops { get => drops; set => drops = value; }
    public GameObject ChunkObject { get => chunkObject; set => chunkObject = value; }
    public Tilemap BackgroundTilemap { get => backgroundTilemap; set => backgroundTilemap = value; }
    public GameObject BackgroundObject { get => backgroundObject; set => backgroundObject = value; }
    public GameObject DROPS1 { get => DROPS; set => DROPS = value; }
    public byte[,] BlockIDsBG1 { get => blockIDsBG; set => blockIDsBG = value; }

    public TerrainChunk(int chunkID, World_Data world, GameObject chunkParent, GameObject chunkObject)
    {
        this.ChunkID = chunkID;
        this.World = world;
        this.BlockIDs = new byte[world.ChunkWidth, world.ChunkHeight];
        this.blockIDsBG = new byte[world.ChunkWidth, world.ChunkHeight];
        this.drops = new List<Drop>();
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
        BackgroundObject.AddComponent<TilemapRenderer>();

        CollisionObject = new GameObject($"Chunk {ChunkID} collision");
        CollisionObject.transform.SetParent(ChunkTileMap.transform);
        CollisionObject.transform.position = new Vector3(ChunkID * World.ChunkWidth, 0f, 0f);
        CollisionTileMap = CollisionObject.AddComponent<Tilemap>();
        ChunkTileMapCollider = CollisionObject.AddComponent<TilemapCollider2D>();
        CollisionTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);


        DROPS = new GameObject($"Chunk {ChunkID} drops");
        DROPS.transform.SetParent(ChunkTileMap.transform);
        InsertDrops();

        return chunkObject;
    }

    /// <summary>
    /// Add the ids of the blocks to the blockIDs array
    /// </summary>
    /// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
    /// <param name="biomindex">Index of the biom of the chunk</param>
    public void GenerateChunk(float[] noisemap, float[,] caveNoisepmap, int biomindex)
    {
        Biom = World.Biom[biomindex];
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            int positionHeight = Mathf.FloorToInt(World.Heightcurve.Evaluate(noisemap[x]) * World.HeightMultiplier) + World.ChunkGroundLevel;
            int heightvalue = 0;
            int blockIDpos = 0;
            int heightvalueBG = 0;
            int blockIDposBG = 0;
            for (int y = positionHeight - 1; y >= 0; y--)
            {
                //Debug.Log("Layerhaeight :"+ world.biom[biomindex].regions[blockIDpos].layerheight + " Heightvalue"+heightvalue);
                if (heightvalue == World.Biom[biomindex].Regions[blockIDpos].RegionRange)
                {
                    blockIDpos++;
                    heightvalue = 0;
                }
                else
                    heightvalue++;
                if (heightvalueBG == World.Biom[biomindex].BgRegions[blockIDposBG].RegionRange)
                {
                    blockIDposBG++;
                    heightvalueBG = 0;
                }
                else
                    heightvalueBG++;
                if (caveNoisepmap[x, y] > world.CaveSize)
                {
                    BlockIDs[x, y] = World.Biom[biomindex].Regions[blockIDpos].BlockID;
                    foreach (OreData oreData in Biom.Ores)
                    {
                        if (caveNoisepmap[x, y] > oreData.NoiseValueFrom && caveNoisepmap[x, y] < oreData.NoiseValueTo)
                        {
                            BlockIDs[x, y] = oreData.BlockID;
                        }
                    }
                    //PlaceTile(x, y, World.Blocks[BlockIDs[x, y]].Tile);
                }
                BlockIDsBG1[x, y] = World.Biom[biomindex].BgRegions[blockIDposBG].BlockID;
            }
        }
        PlaceTiles(biomindex, true);
    }

    /// <summary>
    /// places the tiles in the Tilemap according to the blockIDs array
    /// </summary>
    /// <param name="biomindex">Index of the biom of the chunk</param>b
    public void PlaceTiles(int biomindex, bool init)
    {
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            int heightvalue = 0;
            int blockIDpos = World.Biom[biomindex].Regions.Length - 1;
            for (int y = World.ChunkHeight - 1; y >= 0; y--)
            {
                if (BlockIDs[x, y] != 0)
                {
                    if (heightvalue == World.Biom[biomindex].Regions[blockIDpos].RegionRange)
                    {
                        blockIDpos--;
                        heightvalue = 0;
                    }
                    else
                        heightvalue++;
                    PlaceTile(x, y, World.Blocks[BlockIDs[x, y]].Tile);
                }
                if (BlockIDsBG1[x, y] != 0)
                {
                    if (init)
                        PlaceTileInBG(x, y, World.Blocks[BlockIDsBG1[x, y]].Tile);

                }
            }
        }
    }

    /// <summary>
    /// Lambda expression for shortening reasons
    /// </summary>
    private void PlaceTile(int x, int y, TileBase tile) => ChunkTileMap.SetTile(new Vector3Int(x, y, 0), tile);

    private void PlaceTileInBG(int x, int y, TileBase tile) => BackgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);

    public void BuildCollisions(bool init)
    {
        collisionTileMap.ClearAllTiles();
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
    /// <summary>
    /// Is used to load Chunks in and out
    /// </summary>
    /// <param name="value">represents the state the chunk switches to</param>
    public void SetChunkState(bool value)
    {
        BackgroundObject.SetActive(value);
        ChunkObject.SetActive(value);
    }

    /// <summary>
    /// Removes the block out of the tilemap
    /// </summary>
    public void DeleteBlock(Vector3Int coordinate)
    {
        if (BlockIDs[(coordinate.x - world.ChunkWidth * ChunkID), coordinate.y] == 0) return;

        InstantiateDrop(coordinate);
        ChunkTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID, coordinate.y, 0), null);
        BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID), coordinate.y] = 0;

    }
    /// <summary>
    /// Creating Drop + rigidbody and other Components
    /// </summary>
    public void InstantiateDrop(Vector3Int coordinate)
    {
        Drop d = new Drop();
        d.DropID = BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID), coordinate.y];
        d.DropName = world.Blocks[BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID), coordinate.y]].Name;
        d.DropObject = new GameObject($"Drops {d.DropID}");
        d.DropObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        d.DropObject.AddComponent<SpriteRenderer>();
        d.DropObject.GetComponent<SpriteRenderer>().sprite = world.Blocks[BlockIDs[(coordinate.x - world.ChunkWidth * ChunkID), coordinate.y]].Sprite;
        Vector3 c = coordinate;
        c.y = coordinate.y + 0.5f;
        c.x = coordinate.x + 0.5f;
        d.DropObject.transform.SetPositionAndRotation(c, new Quaternion());
        d.DropObject.AddComponent<Rigidbody2D>();
        d.DropObject.GetComponent<Rigidbody2D>().gravityScale = 20;
        d.DropObject.AddComponent<BoxCollider2D>();
        d.DropObject.layer = LayerMask.NameToLayer("Drops");

        Drops.Add(d);
        InsertDrops();
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
                Drops[x].DropObject.transform.SetParent(DROPS.transform);
            }
        }
    }
    /// <summary>
    /// Saves FPS while removing unessesary gameobjects
    /// </summary>
    public void CheckDropCollision(int x, int y)
    {
        float dropgrouprange = world.Groupdistance;
        if (world.Grid.WorldToCell(Drops[x].DropObject.transform.position).x + dropgrouprange > world.Grid.WorldToCell(Drops[y].DropObject.transform.position).x &&
            world.Grid.WorldToCell(Drops[x].DropObject.transform.position).x - dropgrouprange < world.Grid.WorldToCell(Drops[y].DropObject.transform.position).x &&
            world.Grid.WorldToCell(Drops[x].DropObject.transform.position).y + dropgrouprange > world.Grid.WorldToCell(Drops[y].DropObject.transform.position).y &&
            world.Grid.WorldToCell(Drops[x].DropObject.transform.position).y - dropgrouprange < world.Grid.WorldToCell(Drops[y].DropObject.transform.position).y &&
            Drops[x].DropObject.GetComponent<SpriteRenderer>().sprite.Equals(Drops[y].DropObject.GetComponent<SpriteRenderer>().sprite))
        {
            Drops[x].Anzahl++;
            removeDropfromView(Drops[y]);
            DROPS.SetActive(true);
        }
    }

    /// <summary>
    /// Removes the actual Drop from the scene
    /// </summary>
    /// <param name="removable"></param>
    public void removeDropfromView(Drop removable)
    {
        removable.DropObject.GetComponent<SpriteRenderer>().sprite = null;
        removable.DropObject.GetComponent<BoxCollider2D>().enabled = false;
        removable.DropObject.transform.parent = null;
        removable.DropObject = null;
        Drops.Remove(removable);
    }

}
