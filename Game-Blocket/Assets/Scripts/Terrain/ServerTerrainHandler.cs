using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Netcode;

using UnityEngine;

/// <summary>Handles the <b>Serverside</b> of the Infrastructure</summary>
public class ServerTerrainHandler : TerrainHandler {
	public static ServerTerrainHandler Singleton { get; private set; }

	/// <summary>The payload for handle client requests</summary>
	public byte UpdatePayload { get{
			if(GameManager.State == GameState.LOADING)
				return 100;
			return 3;
		}
	}

	/// <summary>Stores the Loading tasks</summary>
	public Dictionary<Vector2Int, Task> LoadTasks { get; } = new Dictionary<Vector2Int, Task>();

	/// <summary>
	/// Stores the client Rrquests
	/// </summary>
	protected Dictionary<ulong, List<Vector2Int>> Requests { get; } = new Dictionary<ulong, List<Vector2Int>>();
	
	/// <summary>
	/// Returns the List of a specific client from <see cref="ServerTerrainHandler.Requests"/>
	/// </summary>
	/// <param name="clientId">Client to find</param>
	/// <returns>List of specific client</returns>
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

	/// <summary>
	/// First Method for the chunk-request-arrival
	/// </summary>
	/// <param name="clientId">Sender Client Id</param>
	/// <param name="fbR">The Stream (Networked)</param>
	public void HandleChunkRequest(ulong clientId, FastBufferReader fbR) {
		if(DebugVariables.ShowChunkHandle)
			Debug.Log("Got Request");
		fbR.ReadValueSafe(out Vector2Int chunkCoord);
		HandleChunkRequest(clientId, chunkCoord);
	}

	/// <summary>
	/// Inner Method of chunk-request-arrival
	/// </summary>
	/// <param name="clientId">Sender Client Id</param>
	/// <param name="chunkCoord">Requested Chunk</param>
	public void HandleChunkRequest(ulong clientId, Vector2Int chunkCoord) => GetRequests(clientId).Add(chunkCoord);

	/// <summary>Sends a chunk to a client via string</summary>
	/// <param name="cD"><see cref="ChunkData"/></param>
	/// <param name="dest">client id</param>
	public void SendChunkResponse(ChunkData cD, ulong dest) {
		if(DebugVariables.ShowChunkHandle)
			Debug.Log($"Send Respose: {cD.ChunkPositionInt}");
		GetRequests(dest).Remove(cD.ChunkPositionInt);

		//If Host (due to Netcode: "mehh not sending to serverId == clientId)
		if(NetworkManager.Singleton.IsHost) {
			GlobalVariables.ClientTerrainHandler.HandleChunkResponse(cD);
			return;
		}

		//Networked
		using FastBufferWriter writer = new FastBufferWriter(1000, Allocator.Temp);
		writer.WriteValueSafe(WorldProfile.ConvertChunkDataToString(cD));
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ChunkResponse", dest, writer, NetworkDelivery.Reliable);
	}

	/// <summary>
	/// Standartised name for loading-tasks (thread in the task)
	/// </summary>
	/// <param name="cord">Coordinate of the loading instruction</param>
	/// <returns>The name</returns>
	private string NameThread(Vector2Int cord) => $"Load Task: {cord}";
	/// <summary>
	/// Loads the missing Chunk
	/// </summary>
	/// <param name="cord">Int-Coordinate</param>
	public void LoadChunkFromFile(Vector2Int cord){
		Thread.CurrentThread.Name = NameThread(cord);
		//Load from File
		TerrainChunk tc = WorldProfile.LoadChunk(cord);
		if(tc == null){ 
			//Send to TerrainGen
			lock(LoadTasks)
				LoadTasks.Remove(cord);
			TerrainGeneration.BuildChunk(cord);
			return;
		}
		//If finished
		lock(LoadTasks)
			LoadTasks.Remove(cord);
		lock(Chunks)
			Chunks.Add(cord, tc);
		
	}
	#endregion

	#region Unity Scripts
	/// <summary>
	/// Sets the Messagehadler
	/// </summary>
	public void Awake() {
		Singleton = this;
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("RequestChunk", HandleChunkRequest);
		
		//Debug
		if(DebugVariables.WorldNetworking){
			Debug.Log("Registered Chunk Request");
			Debug.Log($"Server ID: {NetworkManager.Singleton.ServerClientId} Client ID: {NetworkManager.Singleton.LocalClientId}");
		}
	}

	/// <summary>
	/// Handles the Requests
	/// </summary>
	public void FixedUpdate() {
		///Handle Requests
		if(NetworkManager.Singleton.IsServer) {
			foreach(ulong clientId in Requests.Keys) {
				//coords = the List from requests f an specific client; sentChunks = Chunks responded
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
						}else if(tc != null){
							SendChunkResponse(tc, clientId);
							sentChunks.Add(cord);
						}
					}
				}
				coords.RemoveAll(c => sentChunks.Contains(c));
			}
		}
		if(GameManager.State != GameState.INGAME)
			return;
		if(1 == 2)//Temp
			CheckDrops();
	}
	#endregion

}