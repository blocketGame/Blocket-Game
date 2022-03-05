using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Netcode;

using UnityEngine;

public class ClientTerrainHandler : TerrainHandler {
	public static Queue<TerrainChunk> ChunkTileInitializationQueue { get; } = new Queue<TerrainChunk>();
	protected static List<TerrainChunk> ChunksLastUpdate { get; set; } = new List<TerrainChunk>();
	public TerrainChunk CurrentChunk => Chunks.ContainsKey(CurrentChunkCoord) == true ? Chunks[CurrentChunkCoord] : null;
	public bool CurrentChunkReady => !(CurrentChunk == null || !CurrentChunk.IsImported || !CurrentChunk.Visible);
	protected TerrainChunk LastChunk { get; set; }
	public Vector2Int CurrentChunkCoord => new Vector2Int((int)(PlPosNow.x / WD.ChunkWidth), (int)(PlPosNow.y / WD.ChunkHeight));
	public static Vector3 PlPosNow { get; private set; }
	private List<Vector2Int> RequestedChunks { get; } = new List<Vector2Int>();

	#region Client Side
	public void HandleChunkResponse(ulong clientId, FastBufferReader fbR) {
		Debug.Log("Got Response");
		fbR.ReadValueSafe(out string chunkData);
		HandleChunkResponse(chunkData);
	}

	public void HandleChunkResponse(string msg) => HandleChunkResponse(WorldProfile.ReadFromString(new List<string>(msg.Split('\n')), null));

	public void HandleChunkResponse(ChunkData cd){
		lock(Chunks)
			Chunks[cd.ChunkPositionInt] = cd as TerrainChunk;
		RequestedChunks.Remove(cd.ChunkPositionInt);
		Debug.Log("Chunk Responded");
	}

	public void RequestChunk(Vector2Int chunkCord) {
		if(RequestedChunks.Contains(chunkCord))
			return;
		Debug.Log("Send Request");
		RequestedChunks.Add(chunkCord);
		if(NetworkManager.Singleton.IsHost){
			GlobalVariables.ServerTerrainHandler.HandleChunkRequest(NetworkManager.Singleton.LocalClientId, chunkCord);
			return;
        }
		
		using FastBufferWriter writer = new FastBufferWriter(5120, Allocator.Persistent, 10000);
		
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("RequestChunk", NetworkManager.Singleton.ServerClientId, writer, NetworkDelivery.Reliable);
		writer.WriteValueSafe(chunkCord);
		
	}

	public void IterateChunksAroundPlayer() {
		bool noUpdates = true;

		for(int x = -WD.ChunkDistance; x <= WD.ChunkDistance; x++) {
			for(int y = -WD.ChunkDistance; y <= WD.ChunkDistance; y++) {
				bool needsWork = CheckChunk(new Vector2Int(CurrentChunkCoord.x + x, CurrentChunkCoord.y + y));
				//if (needsWork && DebugVariables.ShowMultipleTasksOrExecution)
				//Debug.Log($"{x}, {y}");
				if(needsWork && noUpdates) {
					noUpdates = false;
				}
			}
		}

		if(GameManager.State == GameState.LOADING && noUpdates) {
			if(TerrainGeneration.TerrainGenerationTaskNames.Count != 0) {
				///TODO: Try with locking
				string names = "Unfinished Tasks!: ";
				lock(TerrainGeneration.TerrainGenerationTaskNames)
					foreach(string s in TerrainGeneration.TerrainGenerationTaskNames)
						if(!string.IsNullOrEmpty(s))
							names += $";";
				if(DebugVariables.ShowMultipleTasksOrExecution)
					Debug.LogWarning(names);
				if(DebugVariables.ShowGameStateEvent)
					Debug.LogWarning("Switching to Playmode!");
			}
			GameManager.State = GameState.INGAME;
		}
	}

	
	public void UpdateLoaded() {
		for(int i = 0; i < WD.ChunkParent.transform.childCount; i++) {//Not clean
			Transform chunkIGO = WD.ChunkParent.transform.GetChild(i);
			if(Vector3.Distance(chunkIGO.position, PlPosNow) > WorldAssets.ChunkLength * GlobalVariables.WorldData.ChunkDistance * 2) {
				Vector2Int cPosI = new Vector2Int((int)chunkIGO.position.x / WorldAssets.ChunkLength, (int)chunkIGO.position.y / WorldAssets.ChunkLength);
				if(!Chunks.TryGetValue(cPosI, out TerrainChunk tcToSave)) {
					Debug.LogWarning($"Destroying unkown TerrainchunkGO: Name: {chunkIGO.name}, Pos: {chunkIGO.position}");
					Destroy(chunkIGO.gameObject);
					continue;
				}
				Chunks.Remove(cPosI);
				Destroy(chunkIGO.gameObject);
				if(NetworkManager.Singleton.IsServer)
					WorldProfile.SaveChunk(tcToSave);
				if(DebugVariables.ShowLoadAndSave)
					Debug.Log($"Removed Chunk: {tcToSave.ChunkPositionInt}");
			}
		}
	}

	public List<TerrainChunk> UpdateVisible() {
		List<TerrainChunk> chunksVisibleNow = new List<TerrainChunk>();
		for(int x = -WD.ChunkDistance; x <= WD.ChunkDistance; x++) {
			for(int y = -WD.ChunkDistance; y <= WD.ChunkDistance; y++) {
				Vector2Int cCC = new Vector2Int(CurrentChunkCoord.x + x, CurrentChunkCoord.y + y);
				Chunks.TryGetValue(cCC, out TerrainChunk chunkI);
				if(chunkI == null || chunkI.InImportQueue || !chunkI.IsImported)
					continue;
				if(!chunkI.Visible)
					chunkI.Visible = true;
				chunksVisibleNow.Add(chunkI);
			}
		}
		return chunksVisibleNow;
	}

	private void DisableChunks(List<TerrainChunk> chunksToDisable) {
		foreach(TerrainChunk tc in chunksToDisable)
			tc.Visible = false;
	}

	/// <summary>
	/// Checks if the Chunk is generated and or activated
	/// </summary>
	/// <returns>If there is work for the chunk</returns>
	private bool CheckChunk(Vector2Int chunkPosInt) {
		if(Chunks.TryGetValue(chunkPosInt, out TerrainChunk tc)) {
			if(tc.InImportQueue)
				return true;
			if(!tc.IsImported) {
				lock(ChunkTileInitializationQueue)
					ChunkTileInitializationQueue.Enqueue(tc);
				tc.InImportQueue = true;
				return true;
			}
		} else {
			if(!RequestedChunks.Contains(chunkPosInt))
				RequestChunk(chunkPosInt);
			return true;
		}
		return false;
	}

	#endregion

	#region Unity Scripts
	public void Awake() {
		GlobalVariables.ClientTerrainHandler = this;
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ChunkResponse", HandleChunkResponse);
		if(DebugVariables.WorldNetworking) {
			Debug.Log("Registered Chunk Response");
			Debug.Log($"Client ID: {NetworkManager.Singleton.LocalClientId} Server ID: {NetworkManager.Singleton.ServerClientId}");
		}
	}

	public void FixedUpdate() {
		//Always
		PlPosNow = GlobalVariables.LocalPlayerPos;
		lock(ChunkTileInitializationQueue) {
			for(int i = 0; i < ChunkTileInitializationQueue.Count && i < _updatePayload; i++) {
				TerrainChunk tc = ChunkTileInitializationQueue.Dequeue();
				if(!tc.IsImported)
					tc.ImportChunk(WD.ChunkParent);
				else
					if(DebugVariables.ShowMultipleTasksOrExecution)
					Debug.LogWarning($"Already Imported: {tc.chunkPosition}");
			}
		}

		//If game loading or Chunk swiched
		if(GameManager.State == GameState.LOADING || (LastChunk?.chunkPosition != CurrentChunk?.chunkPosition))
			IterateChunksAroundPlayer();



		if(GameManager.State == GameState.INGAME) {
			if(LastChunk?.chunkPosition != CurrentChunk?.chunkPosition) {
				List<TerrainChunk> chunksVisibleNow = UpdateVisible();
				foreach(TerrainChunk tc in chunksVisibleNow)
					if(ChunksLastUpdate.Contains(tc))
						ChunksLastUpdate.Remove(tc);
				DisableChunks(ChunksLastUpdate);
				ChunksLastUpdate = chunksVisibleNow;
				UpdateLoaded();
			}
			LastChunk = CurrentChunk;
		}
	}

    #endregion
}