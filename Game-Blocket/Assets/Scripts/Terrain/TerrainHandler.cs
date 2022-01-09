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
	public GameObject ChunkParent;
	public static Dictionary<Vector2Int, TerrainChunk> Chunks { get; } = new Dictionary<Vector2Int, TerrainChunk>();
	public static Queue<TerrainChunk> ChunkCollisionQueue { get;} = new Queue<TerrainChunk>();
	public static Queue<TerrainChunk> ChunkVisibleQueue { get;} = new Queue<TerrainChunk>();
	public static Queue<TerrainChunk> ChunkTileInitializationQueue { get;} = new Queue<TerrainChunk>();
	public static Dictionary<ulong, Vector3> PlayerLastUpdate { get; } = new Dictionary<ulong, Vector3>();

	public WorldData WD => GlobalVariables.WorldData;

	private static readonly bool useItemer = false;
	private readonly uint _checkTime = 1000;
	private readonly byte _updatePayload = 2;
	public Timer TimerInstance { get; private set; }
	public static Task TerrainTask { get; private set; }
	public static Vector3 PlPosNow { get; private set; }
	public static Vector3 PlPosLast { get; private set; }
	public static Vector3? PlPosUpdate { get; private set; } = null;

	#region Unity Scripts
	public void Awake() => GlobalVariables.TerrainHandler = this;

	private void Start() {
		NetworkObject localPlayerNO = GlobalVariables.LocalPlayer.GetComponent<NetworkObject>();
		if(useItemer)
			TimerInstance = new Timer(new TimerCallback(UpdateChunksTask), null, 0, _checkTime);
	}

	private void OnApplicationQuit() {
		TimerInstance?.Change(Timeout.Infinite, Timeout.Infinite);
		TimerInstance = null;
		GC.Collect();
	}

	private void OnApplicationPause(bool pause) {
		Debug.Log($"Pause: {pause}");
	}

	private void FixedUpdate() {
		if (Input.GetKeyDown(KeyCode.L))
		Debug.Log($"{ChunkVisibleQueue.Count}, {ChunkCollisionQueue.Count}, {ChunkTileInitializationQueue.Count}");
		if (!useItemer)
			CheckChunksAroundPlayerStatic();
	}

	void Update() => UpdateChunks(null);

	public void LateUpdate() {
		PlPosLast = PlPosNow;
		PlPosNow = GlobalVariables.LocalPlayerPos;
		if(GameManager.State == GameState.INGAME)
			if (PlPosUpdate == null || Vector3.Distance(PlPosUpdate ?? new Vector3(), PlPosNow) > 5)
				UpdateVisible();
	}
	#endregion
	public void UpdateChunksTask(object _) {
		Debug.Log("Updated Chunks");
		if (TerrainTask == null || TerrainTask.IsCompleted) {
			TerrainTask = null;
			TerrainTask = new Task(CheckChunksAroundPlayerStatic);
			TerrainTask.Start();
		} else
			Debug.LogWarning($"Terraintask: {TerrainTask.Status}");
		
	}

	public void UpdateVisible() {
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(GlobalVariables.chunkTag))
			if (Vector3.Distance(go.transform.position, PlPosNow) > WD.ChunkDistance*WD.ChunkWidth * 2)
				Destroy(go);
			else if (Vector3.Distance(go.transform.position, PlPosNow) > WD.ChunkDistance * WD.ChunkWidth)
				DisableChunk(go.transform.position);
	}

	private void DisableChunk(Vector3 pos) {
		Vector2Int chunkcoord = new Vector2Int(Mathf.RoundToInt(pos.x / WD.ChunkWidth), Mathf.RoundToInt(pos.y / WD.ChunkHeight));
		if (Chunks.TryGetValue(chunkcoord, out TerrainChunk tc))
			tc.IsVisible = false;
		else
			Debug.LogWarning($"Chunk not found:{chunkcoord}");
	}

	public void UpdateChunks(NetworkObject playerNO) {
		int fUP = _updatePayload;
		///TileInit
		lock (ChunkTileInitializationQueue) {
			if (ChunkTileInitializationQueue.Count > 0)
			for (int i = 0; i < ChunkTileInitializationQueue.Count && i < fUP; i++) {
				TerrainChunk tc = ChunkTileInitializationQueue.Dequeue();
				if (!tc.IsImported)
					tc.ImportChunk(ChunkParent);
				else
					if (DebugVariables.showMultipleTasksOrExecution)
						Debug.LogWarning($"Already Imported: {tc.ChunkData.chunkPosition}");
			}
					
		}

		///Collision Init
		lock (ChunkCollisionQueue) {
			if (ChunkCollisionQueue.Count > 0)
				for (int i = 0; i < ChunkCollisionQueue.Count && i < fUP; i++) {
					TerrainChunk tc = ChunkCollisionQueue.Dequeue();
					if (!(tc?.BuildCollisions() ?? false))
						ChunkCollisionQueue.Enqueue(tc);
				}	
		}
		//PlayerLastUpdate[playerNO.NetworkInstanceId] = playerNO.gameObject.transform.position;
		//CheckChunksAroundPlayerNetworked(playerNO);
	}

	private void OrderChunk(Vector2Int chunkPosInt) {
		//2. Search in File
		///TODO...
		//3. Generate
		TerrainGeneration.BuildChunk(chunkPosInt, ChunkParent);
		//Debug.Log($"Chunk Ordered: {chunkPosInt}");
	}

	/// <summary>
	/// Checks if the Chunk is generated and or activated
	/// </summary>
	/// <returns>If there is work for the chunk</returns>
	private bool SearchChunk(Vector2Int chunkPosInt) {
		lock(Chunks)
			if (Chunks.TryGetValue(chunkPosInt, out TerrainChunk tc)) {
				if (!tc.IsImported) {
					lock (ChunkTileInitializationQueue)
						ChunkTileInitializationQueue.Enqueue(tc);
					return true;
				} else if (!tc.IsVisible) { 
					ChunkVisibleQueue.Enqueue(tc);
					return true;
				}
			} else { 
				OrderChunk(chunkPosInt);
				return true;
			}
		return false;
	}

	public void CheckChunksAroundPlayerStatic() {
		if (PlPosNow == PlPosLast && GameManager.State == GameState.INGAME)
			return;
		bool noUpdates = true;
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(PlPosNow.x / WD.ChunkWidth), Mathf.RoundToInt(PlPosNow.y / WD.ChunkHeight));

		for (int x = -WD.ChunkDistance; x <= WD.ChunkDistance; x++) {
			for (int y = -WD.ChunkDistance; y <= WD.ChunkDistance; y++) {
				bool needsWork = SearchChunk(new Vector2Int(currentChunkCoord.x + x, currentChunkCoord.y + y));
				if (needsWork && DebugVariables.showMultipleTasksOrExecution)
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
				Debug.LogWarning(names);
				Debug.LogWarning("Switching to Playmode!");
			}
			GameManager.State = GameState.INGAME;
		}
	}

	[ClientRpc]
	public void SendChunkClientRpc(ChunkList chunkList, ulong clientID) {
		Debug.Log(clientID);
		foreach (ChunkData chunk in chunkList) {
			//TerrainChunk terrainChunkToReturn = Chunk.TransferChunkToBlocks(chunk, ChunkParent);
			//WD.Chunks[CastVector2ToInt(chunk.ChunkPosition)] = terrainChunkToReturn;
			//terrainChunkToReturn.ChunkVisible = true;
		}
	}

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
		ChunkData chunk = GetChunkFromCoordinate(x, y)?.ChunkData;
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

		int chunkX = coordinate.x - chunk.ChunkData.ChunkPositionInt.x * WD.ChunkWidth;
		int chunkY = coordinate.y - chunk.ChunkData.ChunkPositionInt.y * WD.ChunkHeight;

		chunk.CollisionTileMap.SetTile(new Vector3Int(chunkX, chunkY, 0), null);

		if (GetBlockFormCoordinate(coordinate.x, coordinate.y) != 0 &&
			(GetBlockFormCoordinate(coordinate.x + 1, coordinate.y) == 0 ||
			GetBlockFormCoordinate(coordinate.x, coordinate.y + 1) == 0 ||
			GetBlockFormCoordinate(coordinate.x - 1, coordinate.y) == 0 ||
			GetBlockFormCoordinate(coordinate.x, coordinate.y - 1) == 0)) {
			chunk.CollisionTileMap.SetTile(new Vector3Int(chunkX, chunkY, 0), GetBlockbyId(1).Tile);
		}
	}

	/// <summary>
	/// returns the BlockData object of the index
	/// </summary>
	/// <param name="id">index of the block</param>
	/// <returns></returns>
	public BlockData GetBlockbyId(byte id) {
		foreach (BlockData bd in WD.Blocks) {
			if (bd.BlockID == id) {
				return bd;
			}
		}
		return WD.Blocks[0];
	}

	/// <summary>
	/// Method at wrong PLACE
	/// </summary>
	public void IgnoreDropCollision() {
		//foreach (TerrainChunk t in GlobalVariables.localGameVariables.terrainGeneration.ChunksVisibleLastUpdate)
		//    foreach (Drop d in t.Drops)
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Drops"), LayerMask.NameToLayer("Player"));
	}

	public static Vector2Int CastVector2ToInt(Vector2 vectorToCast) => new Vector2Int((int)vectorToCast.x, (int)vectorToCast.y);
}
