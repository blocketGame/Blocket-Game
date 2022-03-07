using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Netcode;

using UnityEngine;

/// <summary>Handles the <b>clientside</b> for the Worldhandling</summary>
public class ClientTerrainHandler : TerrainHandler {

	/// <summary>Static due to <see cref="TerrainGeneration"/></summary>
	public static Queue<TerrainChunk> ChunkTileInitializationQueue { get; } = new Queue<TerrainChunk>();

	/// <summary>Queue which stores objects for destory</summary>
	private  Queue<Transform> DestroyQueue { get; } = new Queue<Transform>();

	/// <summary>Chunks that are Requested from Server</summary>
	private List<Vector2Int> RequestedChunks { get; } = new List<Vector2Int>();

    #region Visibility
    /// <summary>Chunks the were visible the last Update frame</summary>
    private List<TerrainChunk> ChunksLastUpdate { get; set; } = new List<TerrainChunk>();
	/// <summary>Chunk that was the lastChunk the player were in</summary>
	private TerrainChunk LastChunk { get; set; }

	/// <summary>Chunk where the player stands</summary>
	public TerrainChunk CurrentChunk => Chunks.ContainsKey(CurrentChunkCoord) == true ? Chunks[CurrentChunkCoord] : null;
	
	/// <summary>True if the Chunk is loaded, importet and visible</summary>
	public bool CurrentChunkReady => !(CurrentChunk == null || !CurrentChunk.IsImported || !CurrentChunk.Visible);
    #endregion

    #region Util-Methods
    /// <summary>Calculates the current Chunkcorrdinate (<seealso cref="Vector2Int"/>) where the player stands in</summary>
    public Vector2Int CurrentChunkCoord => new Vector2Int((int)(PlPosNow.x / WD.ChunkWidth), (int)(PlPosNow.y / WD.ChunkHeight));

	/// <summary>Own variable due to unity: "No plsss noo other thread on gameobject >:("</summary>
	public static Vector3 PlPosNow { get; private set; }
    #endregion

    #region Client Side

	/// <summary> Message-Handler that will be called if the Server has sent the chunkdata-string</summary>
	/// <param name="clientId">Server id</param>
	/// <param name="fbR">networking stream</param>
    public void HandleChunkResponse(ulong clientId, FastBufferReader fbR) {
		if(DebugVariables.WorldNetworking)
			Debug.Log("Got Response");
		fbR.ReadValueSafe(out string chunkData);
		HandleChunkResponse(chunkData);
	}

	/// <summary>Handles a single hardmade string</summary>
	/// <param name="msg"></param>
	public void HandleChunkResponse(string msg) => HandleChunkResponse(WorldProfile.ReadFromString(new List<string>(msg.Split('\n')), null));

	/// <summary>Imports the Chunk Response</summary>
	/// <param name="cd">Chunkdata struct</param>
	public void HandleChunkResponse(ChunkData cd){
		lock(Chunks)
			Chunks[cd.ChunkPositionInt] = cd as TerrainChunk;
		RequestedChunks.Remove(cd.ChunkPositionInt);
		Debug.Log("Chunk Responded");
	}

	/// <summary>
	/// Sends a chunk request to the server
	/// </summary>
	/// <param name="chunkCord">Requested chunk coord</param>
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

	/// <summary>Method which will Iterate all chunks around the player and requests chunks</summary>
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

	/// <summary>Unloads loaded chunks that are outside of render distance *4 and requests deleting</summary>
	public void UpdateLoaded() {
		//for(int i = 0; i < WD.ChunkParent.transform.childCount; i++) {//Not clean
		//	Transform chunkIGO = WD.ChunkParent.transform.GetChild(i);
		//	if(Vector3.Distance(chunkIGO.position, PlPosNow) > WorldAssets.ChunkLength * GlobalVariables.WorldData.ChunkDistance * 4) {
		//		Vector2Int cPosI = new Vector2Int((int)chunkIGO.position.x / WorldAssets.ChunkLength, (int)chunkIGO.position.y / WorldAssets.ChunkLength);
		//		if(!Chunks.TryGetValue(cPosI, out TerrainChunk tcToSave)) {
		//			Debug.LogWarning($"Destroying unkown TerrainchunkGO: Name: {chunkIGO.name}, Pos: {chunkIGO.position}");
		//			DestroyQueue.Enqueue(chunkIGO);
		//			continue;
		//		}
		//		Chunks.Remove(cPosI);
		//		DestroyQueue.Enqueue(chunkIGO);
		//		if(NetworkManager.Singleton.IsServer)
		//			WorldProfile.SaveChunk(tcToSave);
		//		if(DebugVariables.ShowLoadAndSave)
		//			Debug.Log($"Removed Chunk: {tcToSave.ChunkPositionInt}");
		//	}
		//}
	}

	/// <summary>Makes all chunks invisible outside of the renderdistance</summary>
	/// <returns>The chunks that are visible this frame</returns>
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

	/// <summary>Makes all chunks invisible</summary>
	/// <param name="chunksToDisable">Chunks that have to be visible</param>
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
	/// <summary>Registers the Response-Message</summary>
	public void Awake() {
		GlobalVariables.ClientTerrainHandler = this;
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ChunkResponse", HandleChunkResponse);
		if(DebugVariables.WorldNetworking) {
			Debug.Log("Registered Chunk Response");
			Debug.Log($"Client ID: {NetworkManager.Singleton.LocalClientId} Server ID: {NetworkManager.Singleton.ServerClientId}");
		}
	}

    public void Update() {
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
        if(DestroyQueue.Count > 0)
            Destroy(DestroyQueue.Dequeue().gameObject);
    }

    public void FixedUpdate() {
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