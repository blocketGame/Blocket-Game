using MLAPI;
using MLAPI.Messaging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using static System.Net.WebRequestMethods;
using static TerrainChunk;

/// <summary>
/// Author: Philipp Cserich, Thomas Boigner, Domas Bogner
/// </summary>
public  class TerrainHandler : MonoBehaviour
{
	public GameObject ChunkParent;
	public static Dictionary<Vector2Int, TerrainChunk> Chunks { get; } = new Dictionary<Vector2Int, TerrainChunk>();

	/// <summary>
	/// Chunk was ONLY Visible Last Update: Stores Chunks that has to get turned of
	/// </summary>
	public static List<TerrainChunk> ChunksVisibleOLU { get; set; } = new List<TerrainChunk>();
	/// <summary>
	/// Chunk Visible this Update
	/// </summary>
	public static List<TerrainChunk> ChunksVisibleTU { get; set; } = new List<TerrainChunk>();
	public static Queue<TerrainChunk> ChunkCollisionQueue { get;} = new Queue<TerrainChunk>();
	public static Queue<TerrainChunk> ChunkTileInitializationQueue { get;} = new Queue<TerrainChunk>();
	public static Dictionary<ulong, Vector3> PlayerLastUpdate { get; } = new Dictionary<ulong, Vector3>();

	public WorldData WD => GlobalVariables.WorldData;

	private static readonly bool useItemer = false;
	private readonly uint _checkTime = 1000;
	private readonly byte _updatePayload = 2;
	public Timer TimerInstance { get; private set; }
	public static Task TerrainTask { get; private set; }
	public static Vector3 PlayerPos { get; private set; }
	public static Vector3 LastPlayerPos { get; private set; }

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
		Debug.Log($"{ChunksVisibleTU.Count}, {ChunksVisibleOLU.Count}, {ChunkCollisionQueue.Count}, {ChunkTileInitializationQueue.Count}");
		if (!useItemer)
			CheckChunksAroundPlayerStatic();
	}

	void Update() => UpdateChunks(null);

	public void LateUpdate() {
		PlayerPos = GlobalVariables.LocalPlayerPos;
		//DisableChunksOutOfRange();
	}
	#endregion

	private void DisableChunksOutOfRange() {
		foreach (GameObject chunkGo in GameObject.FindGameObjectsWithTag("Chunk")) {
			float f = Vector2.Distance(chunkGo.transform.position, GlobalVariables.LocalPlayerPos);
			if (f >= GlobalVariables.WorldData.ChunkDistance * GlobalVariables.WorldData.ChunkWidth) {
				if (chunkGo.activeSelf) {
					chunkGo.SetActive(false);
				}
			}
		}
	}


	public void UpdateChunksTask(object _) {
		Debug.Log("Updated Chunks");
		if (TerrainTask == null || TerrainTask.IsCompleted) {
			TerrainTask = new Task(CheckChunksAroundPlayerStatic);
			TerrainTask.Start();
		} else
			Debug.LogWarning($"Terraintask: {TerrainTask.Status}");
		
	}

	public void UpdateChunks(NetworkObject playerNO) {
		int fUP = _updatePayload;
		///Disable Chunkss
		if (ChunksVisibleOLU.Count > 0) {
			lock (ChunksVisibleOLU) {
				foreach (TerrainChunk tc in ChunksVisibleOLU)
					tc.IsVisible = false;
			}
			ChunksVisibleOLU.Clear();
		}

		//EnableChunks
		if (ChunksVisibleTU.Count > 0) {
			lock (ChunksVisibleTU) {
				foreach (TerrainChunk tc in ChunksVisibleTU)
					tc.IsVisible = true;
			}
			ChunksVisibleTU.Clear();
		}

		///TileInit
		if (ChunkTileInitializationQueue.Count > 0)
			lock (ChunkTileInitializationQueue) {
				for (int i = 0; i < ChunkTileInitializationQueue.Count && i < fUP; i++)
					ChunkTileInitializationQueue.Dequeue().ImportChunk(ChunkParent);
			}

		///Collision Init
		if (ChunkCollisionQueue.Count > 0)
			lock (ChunkCollisionQueue) {
				for (int i = 0; i < ChunkCollisionQueue.Count && i < fUP; i++) {
					TerrainChunk tc = ChunkCollisionQueue.Dequeue();
					try{
						tc.BuildCollisions();
					}catch(Exception e) {
						Debug.LogWarning($"Thread: {Thread.CurrentThread.Name} \n {e.Message}");
						ChunkCollisionQueue.Enqueue(tc);
					}
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
		Debug.Log($"Chunk Ordered: {chunkPosInt}");
	}

	/// <summary>
	/// Checks if the Chunk is generated and or activated
	/// </summary>
	private void SearchChunk(Vector2Int chunkPosInt) {
		if (chunkPosInt.x == 4)
			Debug.Log(4);
		lock(Chunks)
			if (Chunks.TryGetValue(chunkPosInt, out TerrainChunk tc)) {
				if (tc.IsImported) {
					lock(ChunksVisibleOLU)
						lock (ChunksVisibleTU) {
							if (tc.IsVisible)
								ChunksVisibleOLU.Remove(tc);
							else
								ChunksVisibleTU.Add(tc);
						}
				} else
					lock(ChunkTileInitializationQueue)
						ChunkTileInitializationQueue.Enqueue(tc);
			} else
				OrderChunk(chunkPosInt);
	}

	public void CheckChunksAroundPlayerStatic() {
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(PlayerPos.x / WD.ChunkWidth), Mathf.RoundToInt(PlayerPos.y / WD.ChunkHeight));

		for (int x = -WD.ChunkDistance; x <= WD.ChunkDistance; x++) {
			for (int y = -WD.ChunkDistance; y <= WD.ChunkDistance; y++) {
				Debug.Log($"Searched Chunk: {x}, {y}");
				SearchChunk(new Vector2Int(currentChunkCoord.x + x, currentChunkCoord.y + y));
			}
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
	public void UpdateCollisionsAt(Vector3Int coordinate) {
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
