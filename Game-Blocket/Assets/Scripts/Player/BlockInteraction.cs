using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// Used for Interacten per Mouse with Tilemap<br></br>
/// <b>TODO: Cleanup!!</b>
/// </summary>
public class BlockInteraction : MonoBehaviour {
	public Grid grid;
	public Camera mainCamera;
	public int selectedBlock;
	public WorldData world => GlobalVariables.WorldData;
	public Vector3Int coordinate;
	public float count;
	public GameObject deleteSprite;
	public Sprite crackTile;

	private TerrainHandler TH => GlobalVariables.TerrainHandler;

	private Vector3 PlayerPos { get => GlobalVariables.LocalPlayerPos; }

	#region UnityMethods
	public void Update() {
		RaycastHit2D r = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.down);
		if (r) {
			if (DebugVariables.showRayCastTargets)
				Debug.Log(r.collider.gameObject.name);
			GameObject objHited = r.collider.gameObject;
			if (objHited.tag == GlobalVariables.chunkTag) {
				///TODO: Get Block and so on...
			}
		}


		if (GameManager.State != GameState.NEVER)
			return;
		//FABIAN PROBLEM WITH INV MOVE TILES NOT VALUES.

		//Was is?

		TerrainChunk chunk = TH.GetChunkFromCoordinate(coordinate.x, coordinate.y);
		Inventory inv = GameObject.Find("Player").GetComponent<Inventory>();
		Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		ItemAssets itemAssets = GameObject.Find("Assets").GetComponent<ItemAssets>();

		if (GameObject.FindGameObjectWithTag("SlotOptions") != null)
			return;
		if (chunk == null)
			return;
		if (chunk.CollidewithDrop(grid.WorldToCell(PlayerPos).x, grid.WorldToCell(PlayerPos).y) != null) {
			Drop collissionDrop = chunk.CollidewithDrop(grid.WorldToCell(PlayerPos).x, grid.WorldToCell(PlayerPos).y);
			TakeDrops(inv, itemAssets.BlockItemsInGame[collissionDrop.DropID], collissionDrop.Count);
			chunk.RemoveDropfromView(collissionDrop);
		}
		ChangeCoordinate(mouseWorldPos);


		if (TH.GetBlockbyId(TH.GetBlockFormCoordinate(coordinate.x, coordinate.y)).BlockID != 0)
			SetBlockOnFocus(mouseWorldPos);
		else { deleteSprite.SetActive(false); }

		// if (Input.mousePosition.x-959 < -200 || Input.mousePosition.x-959 > 200 ||Input.mousePosition.y - 429 < -150 || Input.mousePosition.y - 429 > 150 )
		//   return;

		if (Input.GetKey(GlobalVariables.leftClick)) {

			//if (GameObject.FindGameObjectWithTag("LeftClick")!=null)
			//{
			//CHECK IF IT IS A BLOCK OR NOT
			try {
				chunk = TH.GetChunkFromCoordinate(coordinate.x, coordinate.y);
				if ((coordinate.x.Equals(grid.WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(grid.WorldToCell(mouseWorldPos).y))) {
					StartCoroutine(Count(mouseWorldPos));
					if (!(count > 0))
						RemoveBlockAfterDuration();
				}

			} catch (Exception e) {
				Debug.Log(e.Message);
			}
			//}
		}

		//if (Input.GetKey(GlobalVariables.rightClick) && 
		//	world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkData.blocks[coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkData.chunkPosition.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkData.chunkPosition.y] == 0 &&
		//	!(Input.mousePosition.y - 429 < 55 && Input.mousePosition.y - 429 > -5 && Input.mousePosition.x - 959 > -40 && Input.mousePosition.x - 959 < 40))
		//	{
		//	///[TODO]
		//		ItemAssets assets = GameObject.FindGameObjectWithTag("Assets").GetComponent<ItemAssets>();
		//	//for (int x = 0; x < assets.BlockItemsInGame.Count; x++)
		//	//    selectedBlock = (byte)assets.BlockItemsInGame[x].blockId;
		//		SetTile(chunk);
		//	}
	}

	/// <summary>
	/// UNDONE
	/// </summary>
	public void FixedUpdate() {
		//world.IgnoreDropCollision();
		//for (int x = 0; x < GlobalVariables.TerrainGeneration.ChunksVisibleLastUpdate.Count; x++)
		//	GlobalVariables.TerrainHandler.ChunksVisibleLastUpdate[x].InsertDrops();
	}
	#endregion

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	/// <returns></returns>
	public IEnumerator Count(Vector3 mouseWorldPos) {
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
	private void TakeDrops(Inventory inv, BlockItem blockitem, int anzahl) {
		//Player collides with Drop
		for (int x = 0; x < anzahl; x++)
			inv.AddItem(blockitem);
		GameObject.FindGameObjectWithTag("Inventory").GetComponent<UIInventory>().SynchronizeToHotbar();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="chunk"></param>
	private void SetTile(TerrainChunk chunk) {
		if (selectedBlock <= -1)
			return;

		//chunk.ChunkTileMap.SetTile(new Vector3Int(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionWorldSpace.x, coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionWorldSpace.y, 0), world.Blocks[selectedBlock].Tile);
		//chunk.BlockIDs[(coordinate.x - world.ChunkWidth * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionWorldSpace.x), coordinate.y - world.ChunkHeight * world.GetChunkFromCoordinate(coordinate.x, coordinate.y).ChunkPositionWorldSpace.y] = world.Blocks[selectedBlock].BlockID;
		//world.UpdateCollisionsAt(coordinate);
		//world.UpdateCollisionsAt(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
		//world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
		//world.UpdateCollisionsAt(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
		//world.UpdateCollisionsAt(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	private void SetBlockOnFocus(Vector3 mouseWorldPos) {
		deleteSprite.SetActive(true);
		deleteSprite.transform.position = new Vector3(grid.WorldToCell(mouseWorldPos).x + 0.5f, grid.WorldToCell(mouseWorldPos).y + 0.5f, -2);
		deleteSprite.GetComponent<SpriteRenderer>().sprite = crackTile;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	private void ChangeCoordinate(Vector3 mouseWorldPos) {
		if (!(coordinate.x.Equals(grid.WorldToCell(mouseWorldPos).x) && coordinate.y.Equals(grid.WorldToCell(mouseWorldPos).y))) {
			coordinate = grid.WorldToCell(mouseWorldPos);
			coordinate.z = 0;
			count = TH.GetBlockbyId(TH.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void RemoveBlockAfterDuration() {
		TH.GetChunkFromCoordinate(coordinate.x, coordinate.y).DeleteBlock(coordinate);
		TH.GetChunkFromCoordinate(coordinate.x, coordinate.y).BuildCollisions();
		count = TH.GetBlockbyId(TH.GetBlockFormCoordinate(coordinate.x, coordinate.y)).RemoveDuration;
	}
}
