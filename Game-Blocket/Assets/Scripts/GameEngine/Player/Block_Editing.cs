using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Block_Editing : MonoBehaviour
{
    public GameObject player;
    public Grid grid;
    public Camera mainCamera;
    public int selectedBlock;
    public WorldData world;
    public Vector3Int coordinate;
    public float count;
    public GameObject deleteSprite;
    public Sprite crackTile;
    

    public void Update()
    {
        //FABIAN PROBLEM WITH INV MOVE TILES NOT VALUES.
        TerrainChunk chunk = world.GetChunkFromCoordinate(coordinate.x, coordinate.y);
        Inventory inv = GameObject.Find("Player").GetComponent<Inventory>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        ItemAssets itemAssets = GameObject.Find("Assets").gameObject.GetComponent<ItemAssets>();

        if (GameObject.FindGameObjectWithTag("SlotOptions") != null)
            return;
        if (chunk == null)
            return;
        if (chunk.CollidewithDrop(grid.WorldToCell(player.transform.position).x, grid.WorldToCell(player.transform.position).y) != null)
        {
            Drop collissionDrop = chunk.CollidewithDrop(grid.WorldToCell(player.transform.position).x, grid.WorldToCell(player.transform.position).y);
            TakeDrops(inv,itemAssets.BlockItemsInGame[collissionDrop.DropID], collissionDrop.Anzahl);
            chunk.RemoveDropfromView(collissionDrop);
        }
        ChangeCoordinate(mouseWorldPos);


        if (world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).BlockID != 0)
            SetBlockOnFocus(mouseWorldPos);
        else { deleteSprite.SetActive(false); }

         // if (Input.mousePosition.x-959 < -200 || Input.mousePosition.x-959 > 200 ||Input.mousePosition.y - 429 < -150 || Input.mousePosition.y - 429 > 150 )
         //   return;

        if (Input.GetKey(GlobalVariables.leftClick))
        {

            //if (GameObject.FindGameObjectWithTag("LeftClick")!=null)
            //{
                //CHECK IF IT IS A BLOCK OR NOT
                try
                {
                    chunk = world.GetChunkFromCoordinate(coordinate.x, coordinate.y);
                    if ((coordinate.x.Equals(grid.WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(grid.WorldToCell(mouseWorldPos).y)))
                    {
                        StartCoroutine(Count(mouseWorldPos));
                        if (!(count > 0))
                            RemoveBlockAfterDuration();
                    }

                }
                catch (Exception e)
                {
                        Debug.Log(e.Message);
                }
            //}
        }

        if (Input.GetKey(GlobalVariables.rightClick) && 
            world.GetChunkFromCoordinate(coordinate.x, coordinate.y).BlockIDs[coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y] == 0 &&
            !(Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40))
            {
            ///[TODO]
                ItemAssets assets = GameObject.FindGameObjectWithTag("Assets").GetComponent<ItemAssets>();
            //for (int x = 0; x < assets.BlockItemsInGame.Count; x++)
            //    selectedBlock = (byte)assets.BlockItemsInGame[x].blockId;
                SetTile(chunk);
            }
    }

    IEnumerator Count(Vector3 mouseWorldPos)
    {
        yield return null;
        count -= Time.deltaTime * 5;
        if (!(count > 0))
            StopCoroutine(Count(mouseWorldPos));
    }
    void FixedUpdate()
    {
        world.IgnoreDropCollision();
        for (int x = 0; x < world.Terraingeneration.ChunksVisibleLastUpdate.Count; x++)
            world.Terraingeneration.ChunksVisibleLastUpdate[x].InsertDrops();
    }


    private void TakeDrops(Inventory inv,BlockItem blockitem,int anzahl)
    {
        //Player collides with Drop
        for(int x=0;x<anzahl;x++)
            inv.AddItem(blockitem);
        GameObject.FindGameObjectWithTag("Inventory").GetComponent<UIInventory>().SynchronizeToHotbar();
    }

    private void SetTile(TerrainChunk chunk)
    {
        if (selectedBlock == -1)
            return;
        
        chunk.ChunkTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y, 0), world.Blocks[selectedBlock].Tile);
        chunk.BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y] = world.Blocks[selectedBlock].BlockID;
        world.UpdateCollisionsAt(coordinate);
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
        world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
    }

    private void SetBlockOnFocus(Vector3 mouseWorldPos)
    {
        deleteSprite.SetActive(true);
        deleteSprite.transform.position = new Vector3(grid.WorldToCell(mouseWorldPos).x + 0.5f, grid.WorldToCell(mouseWorldPos).y + 0.5f, -2);
        deleteSprite.GetComponent<SpriteRenderer>().sprite = crackTile;
    }

    private void ChangeCoordinate(Vector3 mouseWorldPos)
    {
        if (!(coordinate.x.Equals(grid.WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(grid.WorldToCell(mouseWorldPos).y)))
        {
            coordinate = grid.WorldToCell(mouseWorldPos);
            coordinate.z = 0;
            count = world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
        }
    }

    private void RemoveBlockAfterDuration()
    {
        world.GetChunkFromCoordinate(coordinate.x, coordinate.y).DeleteBlock(coordinate);
        world.GetChunkFromCoordinate(coordinate.x, coordinate.y).BuildCollisions();
        count = world.getBlockbyId(world.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
    }
}
 