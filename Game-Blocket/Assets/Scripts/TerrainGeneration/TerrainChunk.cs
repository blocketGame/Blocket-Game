using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
* @Author : Thomas Boigner / Cse19455
*/
public class TerrainChunk
{
    #region WorldSpecifications
    [SerializeField]
    private WorldData world;
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

    public WorldData World { get => world; set => world = value; }
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

    public GameObject DropObject { get => dropObject; set => dropObject = value; }
    public List<Drop> Drops { get => drops; set => drops = value; }
    #endregion

    public TerrainChunk(Vector2Int chunkPosition, WorldData world, GameObject chunkParent, GameObject chunkObject)
    {
        this.ChunkPosition = chunkPosition;
        this.World = world;
        this.chunkPositionWorldSpace = new Vector3(chunkPosition.x * world.ChunkWidth, chunkPosition.y * world.ChunkHeight);
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
        chunkObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y}");
        chunkObject.transform.SetParent(chunkParent.transform);
        chunkObject.transform.position = new Vector3(ChunkPosition.x * World.ChunkWidth, ChunkPosition.y * World.ChunkHeight, 0f);

        ChunkTileMap = chunkObject.AddComponent<Tilemap>();
        ChunkTileMapRenderer = chunkObject.AddComponent<TilemapRenderer>();
        ChunkTileMap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        BackgroundObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y} background");
        BackgroundObject.transform.SetParent(ChunkTileMap.transform);
        BackgroundObject.transform.position = new Vector3(ChunkPosition.x * World.ChunkWidth, ChunkPosition.y * World.ChunkHeight, 0f);
        BackgroundTilemap = BackgroundObject.AddComponent<Tilemap>();
        BackgroundObject.AddComponent<TilemapRenderer>();

        CollisionObject = new GameObject($"Chunk {ChunkPosition.x} {ChunkPosition.y} collision");
        CollisionObject.transform.SetParent(ChunkTileMap.transform);
        CollisionObject.transform.position = new Vector3(ChunkPosition.x * World.ChunkWidth, ChunkPosition.y * World.ChunkHeight, 0f);
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

    public void GenerateChunk(float[] noisemap, float[,] caveNoisepmap, float[,] biomNoiseMap)
    {
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            AnimationCurve heightCurve = new AnimationCurve(world.Heightcurve.keys);
            int positionHeight;
            lock (noisemap)
            {
                positionHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[x]) * World.HeightMultiplier) + 1;
            }
            
            for (int y = world.ChunkHeight - 1; y >= 0; y--)
            {
                if (y + chunkPosition.y * world.ChunkHeight < positionHeight)
                {
                    lock (caveNoisepmap)
                    {
                        if (caveNoisepmap[x, y] > world.CaveSize)
                        {
                            lock (biomNoiseMap)
                            {
                                foreach (RegionData region in world.Biom[(int)biomNoiseMap[x, y]].Regions)
                                {
                                    if (region.RegionRange <= positionHeight - (y + ChunkPosition.y * world.ChunkHeight))
                                    {
                                        BlockIDs[x, y] = region.BlockID;
                                    }
                                }

                                foreach (OreData oreData in world.Biom[(int)biomNoiseMap[x, y]].Ores)
                                {
                                    if (caveNoisepmap[x, y] > oreData.NoiseValueFrom && caveNoisepmap[x, y] < oreData.NoiseValueTo)
                                    {
                                        BlockIDs[x, y] = oreData.BlockID;
                                    }
                                }
                            }
 
                        }
                    }
                    
                    foreach (RegionData regionBG in world.Biom[(int)biomNoiseMap[x, y]].BgRegions)
                    {
                        if (regionBG.RegionRange <= positionHeight - (y + ChunkPosition.y * world.ChunkHeight))
                        {
                            BlockIDsBG[x, y] = regionBG.BlockID;
                        }
                    }
                }
            }
        }
    }

    public void PlaceAllTiles()
    {
        for (int x = 0; x < world.ChunkWidth; x++)
        {
            for (int y = 0; y < world.ChunkHeight; y++)
            {
                PlaceTile(x, y, World.Blocks[BlockIDs[x, y]].Tile);
                PlaceTileInBG(x, y, World.Blocks[BlockIDsBG[x, y]].Tile);
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
        for (int x = 0; x < World.ChunkWidth; x++)
        {
            for (int y = 0; y < World.ChunkHeight; y++)
            {
                int worldX = x + ChunkPosition.x * World.ChunkWidth;
                int worldY = y + ChunkPosition.y * World.ChunkHeight;
                if (BlockIDs[x, y] != 0 &&
                    (World.GetBlockFormCoordinate(worldX + 1, worldY) == 0 ||
                    World.GetBlockFormCoordinate(worldX, worldY + 1) == 0 ||
                    World.GetBlockFormCoordinate(worldX - 1, worldY) == 0 ||
                    World.GetBlockFormCoordinate(worldX, worldY - 1) == 0))
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
        if (BlockIDs[(coordinate.x - world.ChunkWidth * chunkPosition.x), (coordinate.y - world.ChunkHeight * chunkPosition.y)] == 0) return;

        InstantiateDrop(coordinate);
        ChunkTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y, 0), null);
        BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y] = 0;
        world.UpdateCollisionsAt(coordinate);
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
    }
    /// <summary>
    /// Creating Drop + rigidbody and other Components
    /// </summary>
    public void InstantiateDrop(Vector3Int coordinate)
    {
        Drop d = new Drop();
        d.DropID =world.Blocks[BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y]].Item1;
        d.DropName = world.Blocks[BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y]].Name;
        d.DropObject = new GameObject($"Drop {d.DropID}");
        d.DropObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        d.DropObject.AddComponent<SpriteRenderer>();
        d.DropObject.GetComponent<SpriteRenderer>().sprite = world.Blocks[BlockIDs[(coordinate.x - world.ChunkWidth * chunkPosition.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).chunkPosition.y]].Sprite;
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
        float dropgrouprange = world.Groupdistance;
        if (world.Grid.WorldToCell(Drops[x].DropObject.transform.position).x + dropgrouprange > world.Grid.WorldToCell(Drops[y].DropObject.transform.position).x &&
            world.Grid.WorldToCell(Drops[x].DropObject.transform.position).x - dropgrouprange < world.Grid.WorldToCell(Drops[y].DropObject.transform.position).x &&
            world.Grid.WorldToCell(Drops[x].DropObject.transform.position).y + dropgrouprange > world.Grid.WorldToCell(Drops[y].DropObject.transform.position).y &&
            world.Grid.WorldToCell(Drops[x].DropObject.transform.position).y - dropgrouprange < world.Grid.WorldToCell(Drops[y].DropObject.transform.position).y &&
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
    public Drop CollidewithDrop(int x,int y)
    {
        //Einsammeldistanz
        float dropgrouprange = world.PickUpDistance;
        for (int i = 0; i<Drops.Count;i++)
        {
            if( x+dropgrouprange > world.Grid.WorldToCell(Drops[i].DropObject.transform.position).x &&
                x-dropgrouprange< world.Grid.WorldToCell(Drops[i].DropObject.transform.position).x &&
                y+dropgrouprange> world.Grid.WorldToCell(Drops[i].DropObject.transform.position).y &&
                y-dropgrouprange < world.Grid.WorldToCell(Drops[i].DropObject.transform.position).y)
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
}