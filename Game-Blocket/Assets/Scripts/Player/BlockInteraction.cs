using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// Used for Interacten per Mouse with Tilemap<br></br>
/// <b>TODO: Cleanup!!</b>
/// </summary>
public class BlockInteraction : MonoBehaviour {
	
	public GameObject deleteSprite;
	public Sprite crackTile;

	public Coroutine BreakCoroutine { get; set; }

	/// <summary>Used for the Mouse Offset<br></br>Due to not perfect input</summary>
	public readonly float mouseOfsetX = -0.5f, mouseOfsetY = -0.5f;

	#region UnityMethods
	public void Update() {
		if (GameManager.State != GameState.INGAME)
			return;

		///Gather Information
		Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
		Vector2Int blockHoverdAbsolute = new Vector2Int(Mathf.RoundToInt(temp.x + mouseOfsetX), Mathf.RoundToInt(temp.y + mouseOfsetY));
		Vector2Int blockInchunkCoord = new Vector2Int(blockHoverdAbsolute.x%ProfileHandler.chunkWidth, blockHoverdAbsolute.y % ProfileHandler.chunkHeight);
		if (blockInchunkCoord.x < 0)
			blockInchunkCoord.x = ProfileHandler.chunkWidth + blockInchunkCoord.x;
		if (blockInchunkCoord.y < 0)
			blockInchunkCoord.y = ProfileHandler.chunkWidth + blockInchunkCoord.y;

		TerrainChunk thisChunk = GlobalVariables.TerrainHandler.GetChunkFromCoordinate(blockHoverdAbsolute.x, blockHoverdAbsolute.y);
		SetFocusGO(blockHoverdAbsolute);
		byte targetBlockID = thisChunk.ChunkData.blocks[blockInchunkCoord.x, blockInchunkCoord.y];

		///Reset if not Pressed
		if (BreakCoroutine != null && !Input.GetKey(GameManager.SPNow.MainInteractionKey)) {
			if(DebugVariables.blockInteractionCR)
				Debug.Log("Stopped");
			StopCoroutine(BreakCoroutine);
			BreakCoroutine = null;
		}
			

		if (Input.GetKeyDown(GameManager.SPNow.MainInteractionKey)) {
			if(DebugVariables.blockInteractionInfo)
				Debug.Log(thisChunk.ChunkData.blocks[blockInchunkCoord.x, blockInchunkCoord.y]);

			if(BreakCoroutine == null) {
				byte targetRemoveDuration = GlobalVariables.WorldData.Blocks[targetBlockID].RemoveDuration;
				BreakCoroutine = StartCoroutine(nameof(BreakBlock), new Tuple<byte, byte, TerrainChunk, Vector2Int>(targetRemoveDuration, targetBlockID, thisChunk, blockInchunkCoord));
				if (DebugVariables.blockInteractionCR)
					Debug.Log("Started!");
			}
		}

		if (Input.GetKey(GameManager.SPNow.SideInteractionKey)) {
			if (DebugVariables.blockInteractionInfo)
				Debug.Log($"{blockHoverdAbsolute}, {blockInchunkCoord}, {thisChunk.ChunkData.ChunkPositionInt}");
			///UNDONE
		}

	}

	#endregion

	/// <summary>Set the Position of the FocusGO</summary>
	/// <param name="mouseWorldPos">Position of the Mouse</param>
	private void SetFocusGO(Vector2Int mouseWorldPos) => deleteSprite.transform.position = new Vector3(mouseWorldPos.x + 0.5f, mouseWorldPos.y + 0.5f, -10);

	/// <summary>Waits the amount of time. Then it will execute the statements after</summary>
	/// <param name="obj">Tuple of the blockID, the seconds and the position</param>
	/// <returns>WaitTimer as yield return</returns>
	public IEnumerator BreakBlock(object obj) {
		Tuple<byte, byte, TerrainChunk,  Vector2Int> values = obj as Tuple<byte, byte, TerrainChunk, Vector2Int> ?? throw new ArgumentException();
		yield return new WaitForSecondsRealtime(values.Item1);
		if (DebugVariables.blockInteractionCR)
			Debug.Log("Finished");
		StopCoroutine(BreakCoroutine);
		BlockBreaked(values.Item2, values.Item3, values.Item4);
	}

	private void BlockBreaked(byte blockID, TerrainChunk thisChunk, Vector2Int blockInChunk) {
		if (DebugVariables.blockInteractionCR)
			Debug.Log($"Block breaked: {blockID}");
		thisChunk.DeleteBlock(blockInChunk);
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
}
