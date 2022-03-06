using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// Used for Interacten per Mouse with Tilemap<br></br>
/// </summary>
//Client
public class PlayerInteraction : MonoBehaviour {

	public GameObject deleteSprite;
	public Sprite crackTile;

    #region CursorSettings
    private CursorMode cursorMode = CursorMode.Auto;
	private Vector2 hotSpot = Vector2.zero;
    #endregion

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
	public TerrainChunk ThisChunk => GlobalVariables.ClientTerrainHandler.GetChunkFromCoordinate(BlockHoverdAbsolute.x, BlockHoverdAbsolute.y);

	public byte TargetBlockID => ThisChunk?.blocks[BlockInchunkCoord.x, BlockInchunkCoord.y] ?? 0;
	public byte TargetBlockIDBG => ThisChunk?.bgBlocks[BlockInchunkCoord.x, BlockInchunkCoord.y] ?? 0;

	/// <summary>Used for the Mouse Offset<br></br>Due to not perfect input</summary>
	public readonly float mouseOfsetX = -0.5f, mouseOfsetY = -0.5f;

	#region UnityMethods
	public void Awake() => GlobalVariables.Interaction = this;

	public void Update() {
		if (GameManager.State != GameState.INGAME || (GlobalVariables.UIInventory?.InventoryOpened ?? false))
        {
			Cursor.SetCursor(GlobalVariables.ItemAssets.InventoryCursor.texture,hotSpot, cursorMode);
			return;
		}
		HandleBlockInteraction();

		KeyCode main = GameManager.SettingsProfile.GetKeyCode("MainInteractionKey");
		KeyCode side = GameManager.SettingsProfile.GetKeyCode("SideInteractionKey");

		if (Input.GetKeyDown(KeyCode.F))
		{
			CraftingStation.HandleCraftingInterface(BlockHoverdAbsolute, GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId.Equals(255)));
		}

		if (Input.GetKeyDown(side) && GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId.Equals(TargetBlockID)) != null)
		{
			//Open Menu
			//Crafting System
			CraftingStation.HandleCraftingInterface(new Vector2Int((int)GlobalVariables.LocalPlayerPos.x, (int)GlobalVariables.LocalPlayerPos.y), GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId.Equals(TargetBlockID)));
		}

		if (GlobalVariables.Inventory.SelectedItemObj is BlockItem bI) { 
			if (Input.GetKey(side))
				bI.OnSideInteractionKey();
			if (Input.GetKey(main))
				bI.OnMainInteractionKey();
		}
		if (GlobalVariables.Inventory.SelectedItemObj is ToolItem tI)
        {
			if (Input.GetKey(side))
				tI.OnSideInteractionKey();
			if (Input.GetKey(side))
				tI.OnMainInteractionKey();
		}
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

    #endregion

    #region BlockInteraction
	public void BlockPlace(){
		if (!GlobalVariables.ClientTerrainHandler.CurrentChunkReady)
			return;
		
		if (DebugVariables.BlockInteractionInfo)
			Debug.Log($"{BlockHoverdAbsolute}, {BlockInchunkCoord}, {ThisChunk.ChunkPositionInt}");
		///UNDONE
		Item selectedItem = GlobalVariables.ItemAssets.GetItemFromItemID(GlobalVariables.Inventory.SelectedItemId);
		if (selectedItem is BlockItem)
			ThisChunk.PlaceBlock(new Vector3Int(BlockInchunkCoord.x, BlockInchunkCoord.y, 0), GlobalVariables.Inventory.SelectedItemId);
		
	}

    public void HandleBlockInteraction(){
		SetFocusGO(BlockHoverdAbsolute, TargetBlockID != 0);
		//Blockinteraction

		if (BreakCoroutine != null && !Input.GetKey(GameManager.SettingsProfile.GetKeyCode("MainInteractionKey")))
		{
			if (DebugVariables.BlockInteractionCR)
				Debug.Log("Stopped");
			StopCoroutine(BreakCoroutine);
			BreakCoroutine = null;
		}
		if (DebugVariables.BlockInteractionInfo)
			Debug.Log(ThisChunk.blocks[BlockInchunkCoord.x, BlockInchunkCoord.y]);

		if (BreakCoroutine == null && TargetBlockID != 0){

			byte targetRemoveDuration = GlobalVariables.WorldData.Blocks[TargetBlockID].removeDuration;
			BreakCoroutine = StartCoroutine(nameof(BreakBlock), new Tuple<byte, byte, TerrainChunk, Vector2Int>(targetRemoveDuration, TargetBlockID, ThisChunk, BlockInchunkCoord));
			if (DebugVariables.BlockInteractionCR)
				Debug.Log("Started!");
		}else if (BreakCoroutine == null && TargetBlockIDBG != 0)
		{

			byte targetRemoveDuration = GlobalVariables.WorldData.Blocks[TargetBlockIDBG].removeDuration;
			BreakCoroutine = StartCoroutine(nameof(BreakBlockBG), new Tuple<byte, byte, TerrainChunk, Vector2Int>(targetRemoveDuration, TargetBlockIDBG, ThisChunk, BlockInchunkCoord));
			if (DebugVariables.BlockInteractionCR)
				Debug.Log("Started!");
		}
	}

	/// <summary>Set the Position of the FocusGO</summary>
	/// <param name="mouseWorldPos">Position of the Mouse</param>
	private void SetFocusGO(Vector2Int mouseWorldPos, bool activate) {
		if(activate)
			Cursor.SetCursor(GlobalVariables.ItemAssets.MiningCursor.texture, hotSpot, cursorMode);
		else
			Cursor.SetCursor(GlobalVariables.ItemAssets.AttackingCursor.texture, hotSpot, cursorMode);
		deleteSprite.transform.position = new Vector3(mouseWorldPos.x + 0.5f, mouseWorldPos.y + 0.5f, deleteSprite.transform.position.z);
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

	public IEnumerator BreakBlockBG(object obj)
	{
		Tuple<byte, byte, TerrainChunk, Vector2Int> values = obj as Tuple<byte, byte, TerrainChunk, Vector2Int> ?? throw new ArgumentException();
		yield return new WaitForSecondsRealtime(values.Item1);
		if (DebugVariables.BlockInteractionCR)
			Debug.Log("Finished");
		StopCoroutine(BreakCoroutine);
		BlockBreakedBG(values.Item2, values.Item3, values.Item4);
	}

	private void BlockBreaked(byte blockID, TerrainChunk thisChunk, Vector2Int blockInChunk) {
		if (DebugVariables.BlockInteractionCR)
			Debug.Log($"Block breaked: {blockID}");
		thisChunk.DeleteBlock(new Vector3Int(blockInChunk.x, blockInChunk.y, 0));
	}

	private void BlockBreakedBG(byte blockID, TerrainChunk thisChunk, Vector2Int blockInChunk)
	{
		if (DebugVariables.BlockInteractionCR)
			Debug.Log($"Block breaked: {blockID}");
		thisChunk.DeleteBlockBG(new Vector3Int(blockInChunk.x, blockInChunk.y, 0));
	}
	#endregion
}
