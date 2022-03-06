using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Netcode;

using UnityEngine;

public class ServerTerrainHandler : TerrainHandler {
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
		Debug.Log("Got Request");
		fbR.ReadValueSafe(out Vector2Int chunkCoord);
		HandleChunkRequest(clientId, chunkCoord);
	}

	public void HandleChunkRequest(ulong clientId, Vector2Int chunkCoord) => GetRequests(clientId).Add(chunkCoord);

	public bool SendChunkResponse(ChunkData cD, ulong dest) {
		Debug.Log("Send Respose");
		if(NetworkManager.Singleton.IsHost) {
			GlobalVariables.ClientTerrainHandler.HandleChunkResponse(cD);
			return true;
		}

		
		using FastBufferWriter writer = new FastBufferWriter(1000, Allocator.Temp);
		writer.WriteValueSafe(WorldProfile.ConvertChunkDataToString(cD));
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ChunkResponse", dest, writer, NetworkDelivery.Reliable);


		return true;
	}
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
				List<Vector2Int> chunks = Requests[clientId], sentChunks = new List<Vector2Int>();
				chunks.ForEach((cord) => {
					if(!Chunks.TryGetValue(cord, out TerrainChunk tc)){
						tc = WorldProfile.LoadChunk(cord);
						if(tc == null)
							TerrainGeneration.BuildChunk(cord);
						else
							lock(Chunks)
								Chunks.Add(cord, tc);
						
					}
					if(tc != null)
						if(SendChunkResponse(tc, clientId))
							sentChunks.Add(cord);
				});
				chunks.RemoveAll(c => sentChunks.Contains(c));
			}
		}
		if(GameManager.State != GameState.INGAME)
			return;
		if(false)//Temp
			CheckDrops();
	}
	#endregion

}