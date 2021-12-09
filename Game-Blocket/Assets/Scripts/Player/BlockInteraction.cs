using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;

using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Used for Interacten per Mouse with Tilemap<br></br>
/// <b>TODO: Cleanup!!</b>
/// </summary>
public class BlockInteraction : MonoBehaviour{
	public Camera mainCamera;
	public int selectedBlock;
	public Vector3Int coordinate;
	public float count;
	public GameObject deleteSprite;
	public Sprite crackTile;

	private Vector3 PlayerPos { get => GlobalVariables.LocalPlayerPos; }

	#region UnityMethods
	public void Update()
	{
		//FABIAN PROBLEM WITH INV MOVE TILES NOT VALUES.

		//Was is?

		TerrainChunk chunk = GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y);
		Inventory inv = GlobalVariables.Inventory;
		Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		ItemAssets itemAssets = GameObject.Find("Assets").GetComponent<ItemAssets>();

		if (GameObject.FindGameObjectWithTag("SlotOptions") != null)
			return;
		if (chunk == null)
			return;
		if (chunk.CollidewithDrop(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(PlayerPos).x, GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(PlayerPos).y) != null)
		{
			Drop collissionDrop = chunk.CollidewithDrop(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(PlayerPos).x, GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(PlayerPos).y);
			TakeDrops(inv,itemAssets.BlockItemsInGame[collissionDrop.DropID], collissionDrop.Anzahl);
			chunk.RemoveDropfromView(collissionDrop);
		}
		ChangeCoordinate(mouseWorldPos);


		if (GlobalVariables.WorldData.GetBlockbyId(GlobalVariables.WorldData.GetBlockFormCoordinate(coordinate.x, coordinate.y)).BlockID != 0)
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
					chunk = GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y);
					if ((coordinate.x.Equals(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos).y)))
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
			GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).BlockIDs[coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x, coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y] == 0 
			//&& !(Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40))
			)
			{
			///[TODO]
				ItemAssets assets = GameObject.FindGameObjectWithTag("Assets").GetComponent<ItemAssets>();
			//for (int x = 0; x < assets.BlockItemsInGame.Count; x++)
			//    selectedBlock = (byte)assets.BlockItemsInGame[x].blockId;
				SetTile(chunk);
			}
	}

	public void FixedUpdate()
	{
		GlobalVariables.WorldData.IgnoreDropCollision();
		for (int x = 0; x < GlobalVariables.TerrainGeneration.ChunksVisibleLastUpdate.Count; x++)
			GlobalVariables.TerrainGeneration.ChunksVisibleLastUpdate[x].InsertDrops();
	}

	#endregion
	/// <summary>
	/// 
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	/// <returns></returns>
	public IEnumerator Count(Vector3 mouseWorldPos)
	{
		yield return null;
		count -= Time.deltaTime * 5;
		if (!(count > 0))
			StopCoroutine(Count(mouseWorldPos));
	}

	

	/// <summary>
	/// 
	/// </summary>
	/// <param name="inv"></param>
	/// <param name="blockitem"></param>
	/// <param name="anzahl"></param>
	private void TakeDrops(Inventory inv,BlockItem blockitem,int anzahl)
	{
		//Player collides with Drop
		for(int x=0;x<anzahl;x++)
			inv.AddItem(blockitem);
		GameObject.FindGameObjectWithTag("Inventory").GetComponent<UIInventory>().SynchronizeToHotbar();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="chunk"></param>
	private void SetTile(TerrainChunk chunk)
	{
		if (selectedBlock <= -1)
			return;
		
		chunk.ChunkTileMap.SetTile(new Vector3Int(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x, coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y, 0), GlobalVariables.WorldData.Blocks[selectedBlock].Tile);
		chunk.BlockIDs[(coordinate.x - GlobalVariables.WorldData.ChunkWidth * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.x), coordinate.y - GlobalVariables.WorldData.ChunkHeight * GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPosition.y] = GlobalVariables.WorldData.Blocks[selectedBlock].BlockID;
		GlobalVariables.WorldData.UpdateCollisionsAt(coordinate);
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
		GlobalVariables.WorldData.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	private void SetBlockOnFocus(Vector3 mouseWorldPos)
	{
		deleteSprite.SetActive(true);
		deleteSprite.transform.position = new Vector3(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos).x + 0.5f, GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos).y + 0.5f, -2);
		deleteSprite.GetComponent<SpriteRenderer>().sprite = crackTile;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	private void ChangeCoordinate(Vector3 mouseWorldPos)
	{
		if (!(coordinate.x.Equals(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos).y)))
		{
			coordinate = GlobalVariables.World.GetComponentInChildren<Grid>().WorldToCell(mouseWorldPos);
			coordinate.z = 0;
			count = GlobalVariables.WorldData.GetBlockbyId(GlobalVariables.WorldData.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void RemoveBlockAfterDuration()
	{
		GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).DeleteBlock(coordinate);
		GlobalVariables.WorldData.GetChunkFromCoordinate(coordinate.x, coordinate.y).BuildCollisions();
		count = GlobalVariables.WorldData.GetBlockbyId(GlobalVariables.WorldData.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
	}
}
 