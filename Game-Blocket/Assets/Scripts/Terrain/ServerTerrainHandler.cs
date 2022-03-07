using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Netcode;

using UnityEditor.PackageManager;

using UnityEngine;

public class ServerTerrainHandler : TerrainHandler {
	
	public byte UpdatePayload { get{
			if(GameManager.State == GameState.LOADING)
				return 100;
			return 3;
		}
	}

	/// <summary>
	/// Stores the Loading tasks
	/// </summary>
	public Dictionary<Vector2Int, Task> LoadTasks { get; } = new Dictionary<Vector2Int, Task>();

	protected Dictionary<ulong, List<Vector2Int>> Requests { get; } = new Dictionary<ulong, List<Vector2Int>>();
	public List<Vector2Int> GetRequests(ulong clientId) {
		if(!Requests.ContainsKey(clientId))
			Requests[clientId] = new List<Vector2Int>();
		return Requests[clientId];
	}

	#region Server Side
	/// <summary>
	/// TODO: Make Network trhingy
	/// </summary>
	protected void CheckDrops() {
		TerrainChunk tc = GetChunkFromCoordinate(GlobalVariables.LocalPlayerPos.x, GlobalVariables.LocalPlayerPos.y) ?? throw new NullReferenceException($"Chunk not found!");

		foreach(Drop drop in tc.Drops) {
			if(Vector3.Distance(drop.GameObject.transform.position, GlobalVariables.LocalPlayerPos) < _pickUpDist) {
				tc.PickedUpDrop(drop);
				break;
			}
		}
	}

	public void HandleChunkRequest(ulong clientId, FastBufferReader fbR) {
		if(DebugVariables.ShowChunkHandle)
			Debug.Log("Got Request");
		fbR.ReadValueSafe(out Vector2Int chunkCoord);
		HandleChunkRequest(clientId, chunkCoord);
	}

	public void HandleChunkRequest(ulong clientId, Vector2Int chunkCoord) => GetRequests(clientId).Add(chunkCoord);

	public bool SendChunkResponse(ChunkData cD, ulong dest) {
		if(DebugVariables.ShowChunkHandle)
			Debug.Log($"Send Respose: {cD.ChunkPositionInt}");
		GetRequests(dest).Remove(cD.ChunkPositionInt);
		if(NetworkManager.Singleton.IsHost) {
			GlobalVariables.ClientTerrainHandler.HandleChunkResponse(cD);
			return true;
		}
		using FastBufferWriter writer = new FastBufferWriter(1000, Allocator.Temp);
		writer.WriteValueSafe(WorldProfile.ConvertChunkDataToString(cD));
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ChunkResponse", dest, writer, NetworkDelivery.Reliable);
		return true;
	}

	/// <summary>
	/// Loads the missing Chunk
	/// </summary>
	/// <param name="cord">Int-Coordinate</param>
	public void LoadChunkFromFile(Vector2Int cord){
		TerrainChunk tc = WorldProfile.LoadChunk(cord);
		if(tc == null){ 
			//Send to TerrainGen
			lock(LoadTasks)
				LoadTasks.Remove(cord);
			TerrainGeneration.BuildChunk(cord);
			return;
		}
		lock(LoadTasks)
			LoadTasks.Remove(cord);
		lock(Chunks)
			Chunks.Add(cord, tc);
		
	}

	/// <summary>
	/// TODO:...
	/// </summary>
	//public void UpdateLoaded() {
	//	for(int i = 0; i < WD.ChunkParent.transform.childCount; i++) {//Not clean
	//		Transform chunkIGO = WD.ChunkParent.transform.GetChild(i);
	//		if(Vector3.Distance(chunkIGO.position, PlPosNow) > WorldAssets.ChunkLength * GlobalVariables.WorldData.ChunkDistance * 2) {
	//			Vector2Int cPosI = new Vector2Int((int)chunkIGO.position.x / WorldAssets.ChunkLength, (int)chunkIGO.position.y / WorldAssets.ChunkLength);
	//			if(!Chunks.TryGetValue(cPosI, out TerrainChunk tcToSave)) {
	//				Debug.LogWarning($"Destroying unkown TerrainchunkGO: Name: {chunkIGO.name}, Pos: {chunkIGO.position}");
	//				Destroy(chunkIGO.gameObject);
	//				continue;
	//			}
	//			Chunks.Remove(cPosI);
	//			Destroy(chunkIGO.gameObject);
	//			if(NetworkManager.Singleton.IsServer)
	//				WorldProfile.SaveChunk(tcToSave);
	//			if(DebugVariables.ShowLoadAndSave)
	//				Debug.Log($"Removed Chunk: {tcToSave.ChunkPositionInt}");
	//		}
	//	}
	//}
	#endregion

	#region Unity Scripts
	public void Awake() {
		GlobalVariables.ServerTerrainHandler = this;
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("RequestChunk", HandleChunkRequest);
		if(DebugVariables.WorldNetworking){
			Debug.Log("Registered Chunk Request");
			Debug.Log($"Server ID: {NetworkManager.Singleton.ServerClientId} Client ID: {NetworkManager.Singleton.LocalClientId}");
		}
	}

	public void FixedUpdate() {
		///Handle Requests
		if(NetworkManager.Singleton.IsServer) {
			foreach(ulong clientId in Requests.Keys) {
				List<Vector2Int> coords = Requests[clientId], sentChunks = new List<Vector2Int>();
				for(int i = 0; i < coords.Count && i < UpdatePayload; i++ ){
					Vector2Int cord = coords[i];
					lock(Chunks){ 
						if(!Chunks.TryGetValue(cord, out TerrainChunk tc)){
							if(LoadTasks.ContainsKey(cord))
								return;
							//If not found in Chunks-Dic and not in Load-Queue
							Task t = new Task(() => LoadChunkFromFile(cord));
							lock(LoadTasks)
								LoadTasks.Add(cord, t);
							t.Start();
						}
						else if(tc != null){
							if(SendChunkResponse(tc, clientId))
								sentChunks.Add(cord);
						}
					}
				}
				coords.RemoveAll(c => sentChunks.Contains(c));
			}
		}
		if(GameManager.State != GameState.INGAME)
			return;
		if(false)//Temp
			CheckDrops();
	}
	#endregion

}