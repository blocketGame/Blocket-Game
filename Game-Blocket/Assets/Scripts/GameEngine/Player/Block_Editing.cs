using System;
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
    public Vector3Int coordinate;
    public float count;
    public GameObject deleteSprite;
    public Sprite crackTile;

    // Start is called before the first frame update
    public void Start()
    {
    }
    

    // Update is called once per frame
    public void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        bool changed;
        //Debug.Log(grid.WorldToCell(mouseWorldPos));
        //Debug.Log(coordinate);
        if (!(coordinate.x.Equals(grid.WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(grid.WorldToCell(mouseWorldPos).y)))
        {
            changed = true;
            coordinate = grid.WorldToCell(mouseWorldPos);
            coordinate.z = 0;
            count = world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
        }
        else
            changed = false;


        if (world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).BlockID != 0)
        {
            deleteSprite.SetActive(true);
            deleteSprite.transform.position = new Vector3(grid.WorldToCell(mouseWorldPos).x + 0.5f, grid.WorldToCell(mouseWorldPos).y + 0.5f, -2);
            deleteSprite.GetComponent<SpriteRenderer>().sprite = crackTile;
        }else
            deleteSprite.SetActive(false);
        //Debug.Log("x =" + (Input.mousePosition.x - 959));
        if (Input.mousePosition.x-959 < -200 || Input.mousePosition.x-959 > 200 ||Input.mousePosition.y - 429 < -150 || Input.mousePosition.y - 429 > 150 )//|| (Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40)) //50 -5
            return;
            if (Input.GetKey(delete))
            {
                try
                {
                    TerrainChunk chunk = world.GetChunkFromCoordinate(coordinate.x, coordinate.y);
                    if ((coordinate.x.Equals(grid.WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(grid.WorldToCell(mouseWorldPos).y)))
                    {
                        StartCoroutine(Count(mouseWorldPos));
                        if (!(count > world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration / 2))
                        {
                            /*deleteSprite.SetActive(true);
                            deleteSprite.transform.position = new Vector3(grid.WorldToCell(mouseWorldPos).x +0.5f,grid.WorldToCell(mouseWorldPos).y+0.5f,-2);
                            deleteSprite.GetComponent<SpriteRenderer>().sprite = crackTile;*/
                        }
                        if (!(count > 0))
                        {
                            world.GetChunkFromCoordinate(coordinate.x, coordinate.y).DeleteBlock(coordinate);
                            world.GetChunkFromCoordinate(coordinate.x, coordinate.y).BuildCollisions();
                            //deleteSprite.SetActive(false);
                            count = world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }


            if (Input.GetKey(create) && world.GetChunkFromCoordinate(coordinate.x, coordinate.y).BlockIDs[coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y] == 0 && !(Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40))
            {
                TerrainChunk chunk = world.GetChunkFromCoordinate(coordinate.x, coordinate.y);
                chunk.ChunkTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y, 0), world.Blocks[selectedBlock].Tile);
                chunk.BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y] = world.Blocks[selectedBlock].BlockID;
                world.UpdateCollisionsAt(coordinate);
                world.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
                world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
                world.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
                world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
            }
        }

        IEnumerator Count(Vector3 mouseWorldPos)
        {
            //es geht grad absolut garned

            yield return null;
            count -= Time.deltaTime * 5;
            if (!(count > 0))
                StopCoroutine(Count(mouseWorldPos));
        }
         void FixedUpdate()
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
 