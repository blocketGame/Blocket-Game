using MLAPI;
using MLAPI.Messaging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using static TerrainChunk;

/// <summary>
/// Author: Philipp Cserich, Thomas Boigner, Domas Bogner
/// </summary>
public  class TerrainHandler : MonoBehaviour
{
	private static readonly bool useItemer = false;
	private readonly uint _checkTime = 1000;
	private readonly byte _updatePayload = 2;
	private readonly byte _pickUpDist = 2;

	public GameObject ChunkParent;

	public static Dictionary<Vector2Int, TerrainChunk> Chunks { get; } = new Dictionary<Vector2Int, TerrainChunk>();
	public static Queue<TerrainChunk> ChunkCollisionQueue { get;} = new Queue<TerrainChunk>();
	public static Queue<TerrainChunk> ChunkTileInitializationQueue { get;} = new Queue<TerrainChunk>();
	public static Dictionary<ulong, Vector3> PlayerLastUpdate { get; } = new Dictionary<ulong, Vector3>();

	public WorldData WD => GlobalVariables.WorldData;

	//Not multiplayer save
	public Vector2Int CurrentChunkCoord => new Vector2Int(Mathf.RoundToInt(PlPosNow.x / WD.ChunkWidth), Mathf.RoundToInt(PlPosNow.y / WD.ChunkHeight));

	public TerrainChunk CurrentChunk => Chunks[CurrentChunkCoord];
	public bool CurrentChunkReady => (CurrentChunk == null || !CurrentChunk.IsImported || !CurrentChunk.Visible);

	public static Vector3 PlPosNow { get; private set; }
	public static Vector3 PlPosLastUpdateV { get; private set; } = new Vector3();
	public static Vector3 PlPosLastUpdateL { get; private set; } = new Vector3();

	#region Unity Scripts
	public void Awake() => GlobalVariables.TerrainHandler = this;

	public void FixedUpdate() {
		IterateChunksAroundPlayerStatic();
		if (GameManager.State != GameState.INGAME)
			return;
		CheckDrops();
		
	}

	/// <summary>
	/// TODO: 
	/// </summary>
	private void CheckDrops()
	{
		TerrainChunk tc = GetChunkFromCoordinate(GlobalVariables.LocalPlayerPos.x, GlobalVariables.LocalPlayerPos.y) ?? throw new NullReferenceException($"Chunk not found!");;

		foreach(Drop drop in tc.Drops){
			if (Vector3.Distance(drop.GameObject.transform.position, GlobalVariables.LocalPlayerPos) < _pickUpDist)
			{
				tc.PickedUpDrop(drop);
				break;
			}
		}
	}

	public void Update(){
		//Init Queue
		lock (ChunkTileInitializationQueue) {
			if (ChunkTileInitializationQueue.Count > 0)
				for (int i = 0; i < ChunkTileInitializationQueue.Count && i < _updatePayload; i++) {
					TerrainChunk tc = ChunkTileInitializationQueue.Dequeue();
					if (!tc.IsImported)
						tc.ImportChunk(ChunkParent);
					else
						if (DebugVariables.ShowMultipleTasksOrExecution)
						Debug.LogWarning($"Already Imported: {tc.chunkPosition}");
				}
		}
	}

	public void LateUpdate() {
		//Visible
		PlPosNow = GlobalVariables.LocalPlayerPos;
		if (GameManager.State != GameState.INGAME)
			return;
		if (Vector3.Distance(PlPosLastUpdateV, PlPosNow) > 5) {
			List<TerrainChunk> chunksVisibleNow = UpdateVisible();
			foreach (TerrainChunk tc in chunksVisibleNow)
				if (ChunksLastUpdate.Contains(tc))
					ChunksLastUpdate.Remove(tc);
			DisableChunk(ChunksLastUpdate);
			ChunksLastUpdate = chunksVisibleNow;
			PlPosLastUpdateV = PlPosNow;
		}
		if (Vector3.Distance(PlPosLastUpdateL, PlPosNow) > WorldAssets.ChunkLength * GlobalVariables.WorldData.ChunkDistance * 1.5) {
			Debug.Log("Update Loaded");
			UpdateLoaded();
			PlPosLastUpdateL = PlPosNow;
		}
	}
	private List<TerrainChunk> ChunksLastUpdate { get; set; } = new List<TerrainChunk>();

	#endregion

	public void UpdateLoaded(){
		for (int i = 0; i < GlobalVariables.TerrainHandler.ChunkParent.transform.childCount; i++) {
			Transform chunkIGO = GlobalVariables.TerrainHandler.ChunkParent.transform.GetChild(i);
			if (Vector3.Distance(chunkIGO.position, PlPosNow) > WorldAssets.ChunkLength * GlobalVariables.WorldData.ChunkDistance * 2) {
				Vector2Int cPosI = new Vector2Int((int)chunkIGO.position.x / WorldAssets.ChunkLength, (int)chunkIGO.position.y / WorldAssets.ChunkLength);
				if (!Chunks.TryGetValue(cPosI, out TerrainChunk tcToSave)){
					Debug.LogWarning($"Destroying unkown TerrainchunkGO: Name: {chunkIGO.name}, Pos: {chunkIGO.position}");
					Destroy(chunkIGO.gameObject);
					continue;
				}
				Chunks.Remove(cPosI);
				Destroy(chunkIGO.gameObject);
				WorldProfile.SaveChunk(tcToSave);
				Debug.Log($"Removed Chunk: {tcToSave.ChunkPositionInt}");
			}
		}
	}

	public List<TerrainChunk> UpdateVisible() {
		List<TerrainChunk> chunksVisibleNow = new List<TerrainChunk>();
		for (int x = -WD.ChunkDistance; x <= WD.ChunkDistance; x++) {
			for (int y = -WD.ChunkDistance; y <= WD.ChunkDistance; y++) {
				Vector2Int cCC = new Vector2Int(CurrentChunkCoord.x + x, CurrentChunkCoord.y + y);
				Chunks.TryGetValue(cCC, out TerrainChunk chunkI);
				if (chunkI == null || chunkI.InImportQueue || !chunkI.IsImported)
					continue;
				if (!chunkI.Visible)
					chunkI.Visible = true;
				chunksVisibleNow.Add(chunkI);
			}
		}
		return chunksVisibleNow;
	}

	private void DisableChunk(List<TerrainChunk> chunksToDisable) {
		foreach(TerrainChunk tc  in chunksToDisable)
			tc.Visible = false;
	}

	private void OrderChunk(Vector2Int chunkPosInt) {
		TerrainChunk tc = WorldProfile.LoadChunk(chunkPosInt);
		if (tc == null)
			TerrainGeneration.BuildChunk(chunkPosInt, ChunkParent);
		else
			lock(Chunks)
				Chunks.Add(chunkPosInt, tc);
	}

	/// <summary>
	/// Checks if the Chunk is generated and or activated
	/// </summary>
	/// <returns>If there is work for the chunk</returns>
	private bool CheckChunk(Vector2Int chunkPosInt) {
		lock(Chunks)
			if (Chunks.TryGetValue(chunkPosInt, out TerrainChunk tc)) {
				if(tc.InImportQueue)
					return true;
				if (!tc.IsImported) {
					lock (ChunkTileInitializationQueue)
						ChunkTileInitializationQueue.Enqueue(tc);
						tc.InImportQueue = true;
					return true;
				}
			} else { 
				OrderChunk(chunkPosInt);
				return true;
			}
		return false;
	}

	public void IterateChunksAroundPlayerStatic() {
		if (PlPosNow == GlobalVariables.LocalPlayerPos && GameManager.State == GameState.INGAME)
			return;
		bool noUpdates = true;
		
		for (int x = -WD.ChunkDistance; x <= WD.ChunkDistance; x++) {
			for (int y = -WD.ChunkDistance; y <= WD.ChunkDistance; y++) {
				bool needsWork = CheckChunk(new Vector2Int(CurrentChunkCoord.x + x, CurrentChunkCoord.y + y));
				if (needsWork && DebugVariables.ShowMultipleTasksOrExecution)
					Debug.Log($"{x}, {y}");
				if (needsWork && noUpdates)
					noUpdates = false;
			}
		}

		if (GameManager.State == GameState.LOADING && noUpdates) { 
			if (TerrainGeneration.TerrainGenerationTaskNames.Count != 0) {
				///TODO: Try with locking
				string names = "Unfinished Tasks!: ";
				lock(TerrainGeneration.TerrainGenerationTaskNames)
					foreach (string s in TerrainGeneration.TerrainGenerationTaskNames)
						if(!string.IsNullOrEmpty(s))
							names += $";";
				if (DebugVariables.ShowMultipleTasksOrExecution)
					Debug.LogWarning(names);
				if (DebugVariables.ShowGameStateEvent)
					Debug.LogWarning("Switching to Playmode!");
			}
			GameManager.State = GameState.INGAME;
		}
	}

	//[ClientRpc]
	//public void SendChunkClientRpc(ChunkList chunkList, ulong clientID) {
	//	Debug.Log(clientID);
	//	foreach (ChunkData chunk in chunkList) {
	//		//TerrainChunk terrainChunkToReturn = Chunk.TransferChunkToBlocks(chunk, ChunkParent);
	//		//WD.Chunks[CastVector2ToInt(chunk.ChunkPosition)] = terrainChunkToReturn;
	//		//terrainChunkToReturn.ChunkVisible = true;
	//	}
	//}

	/// <summary>Returns the chunk the given coordinate is in</summary>
	/// <param name="x">coordinate in a chunk</param>
	/// <returns></returns>
	public TerrainChunk GetChunkFromCoordinate(float x, float y) {
		Vector2Int chunkPosition = new Vector2Int(Mathf.FloorToInt(x / GlobalVariables.WorldData.ChunkWidth), Mathf.FloorToInt(y / GlobalVariables.WorldData.ChunkHeight));

		return Chunks.TryGetValue(chunkPosition, out TerrainChunk chunk) ? chunk : null;
	}

	/// <summary>Returns the block on any coordinate</summary>
	/// <param name="x">x coordinate</param>
	/// <param name="y">y coordinate</param>
	/// <returns></returns>
	public byte GetBlockFormCoordinate(int x, int y) {
		ChunkData chunk = GetChunkFromCoordinate(x, y);
		if (chunk != null) {
			int chunkX = x - WD.ChunkWidth * chunk.ChunkPositionInt.x;
			int chunkY = y - WD.ChunkHeight * chunk.ChunkPositionInt.y;
			if (chunkX < WD.ChunkWidth && chunkY < WD.ChunkHeight) {
				return chunk.blocks[chunkX, chunkY];
			}
		}
		return 1;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="coordinate"></param>
	public void UpdateCollisionsAt(Vector2Int coordinate) {
		TerrainChunk chunk = GetChunkFromCoordinate(coordinate.x, coordinate.y);

		int chunkX = coordinate.x - chunk.ChunkPositionInt.x * WD.ChunkWidth;
		int chunkY = coordinate.y - chunk.ChunkPositionInt.y * WD.ChunkHeight;

		chunk.CollisionTileMap.SetTile(new Vector3Int(chunkX, chunkY, 0), null);

		if (GetBlockFormCoordinate(coordinate.x, coordinate.y) != 0 &&
			(GetBlockFormCoordinate(coordinate.x + 1, coordinate.y) == 0 ||
			GetBlockFormCoordinate(coordinate.x, coordinate.y + 1) == 0 ||
			GetBlockFormCoordinate(coordinate.x - 1, coordinate.y) == 0 ||
			GetBlockFormCoordinate(coordinate.x, coordinate.y - 1) == 0)) {
			chunk.CollisionTileMap.SetTile(new Vector3Int(chunkX, chunkY, 0), GlobalVariables.WorldAssets.GetBlockbyId(1).tile);
		}
	}

	public static Vector2Int CastVector2ToInt(Vector2 vectorToCast) => new Vector2Int((int)vectorToCast.x, (int)vectorToCast.y);
}
