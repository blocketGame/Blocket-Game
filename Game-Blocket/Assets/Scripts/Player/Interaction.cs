using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// Used for Interacten per Mouse with Tilemap<br></br>
/// </summary>
public class Interaction : MonoBehaviour {
	
	public GameObject deleteSprite;
	public Sprite crackTile;

	public Coroutine BreakCoroutine { get; set; }

	Vector3 MousePosInWorld => Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
	Vector2Int BlockHoverdAbsolute => new Vector2Int(Mathf.RoundToInt(MousePosInWorld.x + mouseOfsetX), Mathf.RoundToInt(MousePosInWorld.y + mouseOfsetY));

	public Vector2Int BlockInchunkCoord {
		get {
			Vector2Int blockInchunkCoord = new Vector2Int(BlockHoverdAbsolute.x % WorldAssets.ChunkLength, BlockHoverdAbsolute.y % WorldAssets.ChunkLength);
			if (blockInchunkCoord.x < 0)
				blockInchunkCoord.x = WorldAssets.ChunkLength + blockInchunkCoord.x;
			if (blockInchunkCoord.y < 0)
				blockInchunkCoord.y = WorldAssets.ChunkLength + blockInchunkCoord.y;
			return blockInchunkCoord;
		}
	}
	public TerrainChunk ThisChunk => GlobalVariables.TerrainHandler.GetChunkFromCoordinate(BlockHoverdAbsolute.x, BlockHoverdAbsolute.y);

	/// <summary>Used for the Mouse Offset<br></br>Due to not perfect input</summary>
	public readonly float mouseOfsetX = -0.5f, mouseOfsetY = -0.5f;

	#region UnityMethods
	public void Update() {
		if (GameManager.State != GameState.INGAME || (GlobalVariables.UIInventory?.InventoryOpened ?? false))
			return;
		//Blockinteraction
		if (BreakCoroutine != null && !Input.GetKey(GameManager.SPNow.Keys["MainInteractionKey"])) {
			if (DebugVariables.BlockInteractionCR)
				Debug.Log("Stopped");
			StopCoroutine(BreakCoroutine);
			BreakCoroutine = null;
		}
		if (GlobalVariables.Inventory.SelectedItemObj is ToolItem t)
			if (t.toolType == ToolItem.ToolType.MEELE)
				if (Input.GetKey(GameManager.SPNow.Keys["MainInteractionKey"]))
					Debug.Log("");

		//Default
		HandleBlockInteraction();
	}

	//TODO: Remove
    public void LateUpdate() {
		if (Input.GetKeyDown(KeyCode.Z))
			GlobalVariables.Inventory.AddItem(101, 1, out _);
		if (Input.GetKeyDown(KeyCode.K))
			Debug.Log(ThisChunk.bgBlocks.Length);
		if (Input.GetKeyDown(KeyCode.L))
			ThisChunk.BuildCollisions();
		if (Input.GetKeyDown(KeyCode.U))
			ThisChunk.PlaceAllTiles();
	}
    #endregion

    #region ToolInteraction

    //TODO:
    public void HandleWeaponInteraction(){
		
    }

    #endregion

    #region BlockInteraction
    public void HandleBlockInteraction(){
		if (!GlobalVariables.TerrainHandler.CurrentChunkReady)
			return;
		byte targetBlockID = ThisChunk?.blocks[BlockInchunkCoord.x, BlockInchunkCoord.y] ?? 0;
		SetFocusGO(BlockHoverdAbsolute, targetBlockID != 0);

		if (Input.GetKeyDown(GameManager.SPNow.Keys["MainInteractionKey"])) {
			if (DebugVariables.BlockInteractionInfo)
				Debug.Log(ThisChunk.blocks[BlockInchunkCoord.x, BlockInchunkCoord.y]);

			if (BreakCoroutine == null && targetBlockID != 0) {
				byte targetRemoveDuration = GlobalVariables.WorldData.Blocks[targetBlockID].removeDuration;
				BreakCoroutine = StartCoroutine(nameof(BreakBlock), new Tuple<byte, byte, TerrainChunk, Vector2Int>(targetRemoveDuration, targetBlockID, ThisChunk, BlockInchunkCoord));
				if (DebugVariables.BlockInteractionCR)
					Debug.Log("Started!");
			}
		}

		if (Input.GetKey(GameManager.SPNow.Keys["SideInteractionKey"])) {
			if (DebugVariables.BlockInteractionInfo)
				Debug.Log($"{BlockHoverdAbsolute}, {BlockInchunkCoord}, {ThisChunk.ChunkPositionInt}");
			///UNDONE
			Item selectedItem = GlobalVariables.ItemAssets.GetItemFromItemID(GlobalVariables.Inventory.SelectedItemId);
			if (selectedItem is BlockItem)
				ThisChunk.PlaceBlock(new Vector3Int(BlockInchunkCoord.x, BlockInchunkCoord.y, 0), GlobalVariables.Inventory.SelectedItemId);
		}
	}

	/// <summary>Set the Position of the FocusGO</summary>
	/// <param name="mouseWorldPos">Position of the Mouse</param>
	private void SetFocusGO(Vector2Int mouseWorldPos, bool activate) { 
		deleteSprite.transform.position = new Vector3(mouseWorldPos.x + 0.5f, mouseWorldPos.y + 0.5f, -10);
		deleteSprite.SetActive(activate);
	}

	/// <summary>Waits the amount of time. Then it will execute the statements after</summary>
	/// <param name="obj">Tuple of the blockID, the seconds and the position</param>
	/// <returns>WaitTimer as yield return</returns>
	public IEnumerator BreakBlock(object obj) {
		Tuple<byte, byte, TerrainChunk,  Vector2Int> values = obj as Tuple<byte, byte, TerrainChunk, Vector2Int> ?? throw new ArgumentException();
		yield return new WaitForSecondsRealtime(values.Item1);
		if (DebugVariables.BlockInteractionCR)
			Debug.Log("Finished");
		StopCoroutine(BreakCoroutine);
		BlockBreaked(values.Item2, values.Item3, values.Item4);
	}

	private void BlockBreaked(byte blockID, TerrainChunk thisChunk, Vector2Int blockInChunk) {
		if (DebugVariables.BlockInteractionCR)
			Debug.Log($"Block breaked: {blockID}");
		thisChunk.DeleteBlock(new Vector3Int(blockInChunk.x, blockInChunk.y, 0));
	}
    #endregion
}
