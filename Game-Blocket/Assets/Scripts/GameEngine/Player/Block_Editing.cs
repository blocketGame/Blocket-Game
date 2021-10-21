using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Block_Editing : MonoBehaviour
{
    /// <summary>
    ///  NOT FUNCTIONAL AT THIS POINT
    ///  CONTACT @CSE19455 FOR FURTHER INFORMATION
    /// </summary>

    public GameObject player;
    public KeyCode delete;
    public KeyCode create;
    public Grid grid;
    public Camera mainCamera;
    public int selectedBlock;
    public World_Data world;
    public bool kcdown;

    // Start is called before the first frame update
    public void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
        if(!GlobalVariables.gameRunning)
            return;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int coordinate = grid.WorldToCell(mouseWorldPos);
        coordinate.z = 0;
        //Debug.Log("x =" + (Input.mousePosition.x - 959));
        if (Input.mousePosition.x - 959 < -200 || Input.mousePosition.x - 959 > 200 || Input.mousePosition.y - 429 < -150 || Input.mousePosition.y - 429 > 150)//|| (Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40)) //50 -5
            return;
        if (Input.GetKeyDown(delete))
        {
            kcdown = true;
        }
        if(Input.GetKeyUp(delete))
        {
            kcdown = false;
        }
        if (kcdown)
        {
            try
            {
                //world.GetChunkFromCoordinate(coordinate.x).CollisionTileMap.SetTile(new Vector3Int(coordinate.x-world.ChunkWidth* world.GetChunkFromCoordinate(coordinate.x).ChunkID,coordinate.y,0), null);
                world.GetChunkFromCoordinate(coordinate.x).DeleteBlock(coordinate);
                world.GetChunkFromCoordinate(coordinate.x).BuildCollisions(false);
                world.GetChunkFromCoordinate(coordinate.x + world.ChunkWidth).BuildCollisions(false);
                world.GetChunkFromCoordinate(coordinate.x - world.ChunkWidth).BuildCollisions(false);
            }
            catch { }
        }
        if (Input.GetKeyDown(create) && world.GetChunkFromCoordinate(coordinate.x).BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID), coordinate.y] == 0 && !(Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40))
        {
            //world.GetChunkFromCoordinate(coordinate.x).CollisionTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID, coordinate.y, 0), world.Blocks[selectedBlock].Tile);
            world.GetChunkFromCoordinate(coordinate.x).ChunkTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID, coordinate.y, 0), world.Blocks[selectedBlock].Tile);
            world.GetChunkFromCoordinate(coordinate.x).BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x).ChunkID), coordinate.y] = world.Blocks[selectedBlock].BlockID;
            world.GetChunkFromCoordinate(coordinate.x).BuildCollisions(false);
            world.GetChunkFromCoordinate(coordinate.x + world.ChunkWidth).BuildCollisions(false);
            world.GetChunkFromCoordinate(coordinate.x - world.ChunkWidth).BuildCollisions(false);
        }
    }

    private void FixedUpdate()
    {
        world.IgnoreDropCollision();
        for (int x = 0; x < world.Terraingeneration.ChunksVisibleLastUpdate.Count; x++)
        {
            world.Terraingeneration.ChunksVisibleLastUpdate[x].InsertDrops();
        }
    }

    private void RemoveTile(Tilemap tileMap, Vector3Int position)
    {
        tileMap.SetTile(position, null);
    }

    private void CreateTile(Tilemap tileMap, Vector3Int position, TileBase block)
    {
        tileMap.SetTile(position, block);
    }
}