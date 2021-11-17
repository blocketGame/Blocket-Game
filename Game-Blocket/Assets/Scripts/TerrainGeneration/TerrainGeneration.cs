using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using MLAPI.Messaging;
using static TerrainChunk;
using System;
using MLAPI;
using UnityEngine.UIElements;
using MLAPI.Serialization;
using static TerrainChunk.ChunkList;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class TerrainGeneration : NetworkBehaviour {



	#region Serialization
	[SerializeField]
	private List<TerrainChunk> chunksVisibleLastUpdate;
	[SerializeField]
	private GameObject chunkParent;

	public List<TerrainChunk> ChunksVisibleLastUpdate { get => chunksVisibleLastUpdate; set => chunksVisibleLastUpdate = value; }
	public GameObject ChunkParent { get => chunkParent; set => chunkParent = value; }
	#endregion

	private Queue<TerrainChunk> chunkCollisionQueue = new Queue<TerrainChunk>();

	public WorldData World { get => GlobalVariables.WorldData; }
	public Queue<TerrainChunk> ChunkCollisionQueue { get => chunkCollisionQueue; set => chunkCollisionQueue = value; }

	public static System.Random prng;

	public Dictionary<ulong, Vector3> PlayerLastUpdate { get; } = new Dictionary<ulong, Vector3>();

	public void Awake() {
		GlobalVariables.TerrainGeneration = this;
		ChunksVisibleLastUpdate = new List<TerrainChunk>();
		//World.putBlocksIntoTxt();
		//World.putBiomsIntoTxt();
		prng = new System.Random(World.Seed);
	}

	public void FixedUpdate() {
		NetworkObject localPlayerNO = GlobalVariables.LocalPlayer.GetComponent<NetworkObject>();
		if (GlobalVariables.generateChunksOnClient)
		{
			UpdateChunks(null);
		}
		else
		{
			if (NetworkManager.Singleton.IsServer)
			foreach (NetworkObject no in GameManager.Players)
				if (!PlayerLastUpdate.ContainsKey(no.NetworkInstanceId) || Vector3.Distance(no.gameObject.transform.position, PlayerLastUpdate[no.NetworkInstanceId]) >= (GlobalVariables.WorldData.ChunkDistance * GlobalVariables.WorldData.ChunkWidth) / 8)
					UpdateChunks(no);
		}
		if (NetworkManager.Singleton.IsClient)
			if (!PlayerLastUpdate.ContainsKey(localPlayerNO.NetworkInstanceId) || Vector3.Distance(GlobalVariables.LocalPlayerPos, PlayerLastUpdate[localPlayerNO.NetworkInstanceId]) >= (GlobalVariables.WorldData.ChunkDistance * GlobalVariables.WorldData.ChunkWidth) / 8)
				{
				if (!GlobalVariables.generateChunksOnClient)
					ActivateLocalChunks();
				DisableChunksOutOfRange();
			}
	}

	/// <summary>
	/// TODO: 
	/// </summary>
	//public void UpdateChunks() {
	//	chunksUpdatedLastCheck = GlobalVariables.PlayerPos;
	//	foreach (TerrainChunk t in ChunksVisibleLastUpdate) {
	//		t.SetChunkState(false);
	//	}
	//	ChunksVisibleLastUpdate.Clear();
	//	CheckChunksAroundPlayer(GlobalVariables.PlayerPos);
	//	foreach (TerrainChunk tc in ChunkCollisionQueue)
	//	{
	//		tc.BuildCollisions();
	//	}
	//	ChunkCollisionQueue.Clear();
	//}

	public void UpdateChunks(NetworkObject playerNO)
	{

		if (GlobalVariables.generateChunksOnClient)
		{
			CheckChunksAroundPlayerStatic();
			return;
		}
		PlayerLastUpdate[playerNO.NetworkInstanceId] = playerNO.gameObject.transform.position;
		CheckChunksAroundPlayerNetworked(playerNO);
	}

	private void DisableChunksOutOfRange(){
		foreach (GameObject chunkGo in GameObject.FindGameObjectsWithTag("Chunk")){
			float f = Vector2.Distance(chunkGo.transform.position, GlobalVariables.LocalPlayerPos);
			if (f >= GlobalVariables.WorldData.ChunkDistance * GlobalVariables.WorldData.ChunkWidth){
				if (chunkGo.activeSelf){
					chunkGo.SetActive(false);
				}
			}
		}
	}

	public void ActivateLocalChunks()
	{
		Vector3 playerPos = GlobalVariables.LocalPlayer.transform.position;
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(playerPos.x / World.ChunkWidth), Mathf.RoundToInt(playerPos.y / World.ChunkHeight));

		for (int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++)
		{
			for (int yOffset = -World.ChunkDistance; yOffset < World.ChunkDistance; yOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
				if (World.Chunks.ContainsKey(viewedChunkCoord))
				{
					World.Chunks[viewedChunkCoord].ChunkVisible = true;
				}
			}
		}
	}

	public void CheckChunksAroundPlayerStatic()
	{
		Vector3 playerPos = GlobalVariables.LocalPlayerPos;
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(playerPos.x / World.ChunkWidth), Mathf.RoundToInt(playerPos.y / World.ChunkHeight));

		for (int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++)
		{
			for (int yOffset = -World.ChunkDistance; yOffset < World.ChunkDistance; yOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
				///Chunks
				TerrainChunk chunk = null;
				if (!World.Chunks.ContainsKey(viewedChunkCoord))
				{
					//Request Chunk
					chunk = BuildChunk(viewedChunkCoord);
					World.Chunks[viewedChunkCoord] = chunk;
					chunk.BuildCollisions();
				}
				else if (World.Chunks.ContainsKey(viewedChunkCoord))
					chunk = World.Chunks[viewedChunkCoord];
				chunk.ChunkVisible = true;
			}
		}
	}
		/// <summary>
		/// Activates and deactivates Chunks
		/// </summary>
		public void CheckChunksAroundPlayerNetworked(NetworkObject playerNO) {
		Vector3 playerPos = playerNO.transform.position;
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(playerPos.x / World.ChunkWidth), Mathf.RoundToInt(playerPos.y / World.ChunkHeight));


		ChunkList chunksToSend = new ChunkList();
		for(int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++) {
			for(int yOffset = -World.ChunkDistance; yOffset < World.ChunkDistance; yOffset++) {
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
				///Chunks
				TerrainChunk chunk;
				if (!World.Chunks.ContainsKey(viewedChunkCoord))
				{
					//Request Chunk
					chunk = BuildChunk(viewedChunkCoord);
					chunk.BuildCollisions();
				}else{
					chunk = World.Chunks[viewedChunkCoord];
				}
				if (chunk == null)
					Debug.LogWarning("Chunk is null!");
				if (GlobalVariables.generateChunksOnClient)
				{
					World.Chunks[viewedChunkCoord] = chunk;
					chunk.ChunkVisible = true;
					World.Chunks[viewedChunkCoord].BuildCollisions();
				}
				else
					chunksToSend.Add(Chunk.TransferBlocksToChunk(chunk));
			}
		}
		if(!GlobalVariables.generateChunksOnClient)
			SendChunkClientRpc(chunksToSend, playerNO.OwnerClientId);
	}

	[ClientRpc]
	public void SendChunkClientRpc(ChunkList chunkList, ulong clientID)
	{
		Debug.Log(clientID);
		foreach (Chunk chunk in chunkList)
		{
			TerrainChunk terrainChunkToReturn = Chunk.TransferChunkToBlocks(chunk, ChunkParent);
			World.Chunks[CastVector2ToInt(chunk.chunkPositionWorldSpace)] = terrainChunkToReturn;
			terrainChunkToReturn.ChunkVisible = true;
		}
	}


	public static Vector2Int CastVector2ToInt(Vector2 vectorToCast) => new Vector2Int((int)vectorToCast.x, (int)vectorToCast.y);

	//[ServerRpc]
	//public void RequestChunkServerRpc(Vector2 position, out Chunk chunkReturn)
	//{
	//	Vector2Int viewedChunkCoord = CastVector2ToInt(position);
	//	TerrainChunk chunk = null;
	//	if (!World.Chunks.ContainsKey(viewedChunkCoord))
	//	{
	//		//Request Chunk
	//		chunk = BuildChunk(viewedChunkCoord);
	//		chunk.BuildCollisions();
	//	}else if (World.Chunks.ContainsKey(viewedChunkCoord)){
	//		chunk = World.Chunks[viewedChunkCoord];
	//	}
	//	if (chunk == null)
	//		Debug.LogWarning("Chunk is null!");
	//	chunkReturn = Chunk.TransferBlocksToChunk(chunk);
	//}

	//[ClientRpc]
	//public void ReturnChunkClientRpc(Chunk chunkObj, Vector2 position){
	//	Vector2Int viewedChunkCoord = CastVector2ToInt(position);
	//	TerrainChunk terrainChunkToReturn = Chunk.TransferChunkToBlocks(chunkObj, ChunkParent);
	//	World.Chunks[viewedChunkCoord] = terrainChunkToReturn;
	//	terrainChunkToReturn.ChunkVisible = true;
	//	terrainChunkToReturn.BuildCollisions();
	//	ChunksVisibleLastUpdate.Add(terrainChunkToReturn);
	//	Debug.Log("a");
	//}

	/// <summary>
	/// Checks whether or not a Biom is complete 
	/// (Biomnr/Biomsize)
	/// </summary>

	/// <summary>
	///     Generates Chunk From Noisemap without any extra consideration
	/// </summary>
	private TerrainChunk BuildChunk(Vector2Int position) {
		TerrainChunk chunk = new TerrainChunk(position, ChunkParent);
		List<Biom> bioms;
		if (position.y > -20)
			bioms = World.GetBiomsByType(Biomtype.OVERWORLD);
		else
			bioms = World.GetBiomsByType(Biomtype.UNDERGROUND);
		chunk.GenerateChunk(
			  NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth),
			  NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, World.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
			  NoiseGenerator.generateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, World.OffsetY + position.y * World.ChunkHeight), bioms));
		return chunk;
	}

}