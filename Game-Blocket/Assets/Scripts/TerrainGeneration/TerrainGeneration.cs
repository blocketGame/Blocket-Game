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

	public Vector3 chunksUpdatedLastCheck;

	public void Awake() {
		GlobalVariables.TerrainGeneration = this;
		ChunksVisibleLastUpdate = new List<TerrainChunk>();
		//World.putBlocksIntoTxt();
		//World.putBiomsIntoTxt();
		prng = new System.Random(World.Seed);
	}

	public void FixedUpdate() {
		if(chunksUpdatedLastCheck == null || 
			Vector3.Distance(GlobalVariables.PlayerPos, chunksUpdatedLastCheck) >= (GlobalVariables.WorldData.ChunkDistance* GlobalVariables.WorldData.ChunkWidth)/4)
			UpdateChunks();
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

	public void UpdateChunks()
	{
		chunksUpdatedLastCheck = GlobalVariables.PlayerPos;
		CheckChunksAroundPlayer(GlobalVariables.PlayerPos);

		//Domas collision thing
		foreach (TerrainChunk tc in ChunkCollisionQueue)
			tc.BuildCollisions();
		ChunkCollisionQueue.Clear();
		
		
	}

	/// <summary>
	/// Activates and deactivates Chunks
	/// </summary>
	public void CheckChunksAroundPlayer(Vector3 playerPos) {
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(playerPos.x / World.ChunkWidth), Mathf.RoundToInt(playerPos.y / World.ChunkHeight));

		for(int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++) {
			for(int yOffset = -World.ChunkDistance; yOffset < World.ChunkDistance; yOffset++) {
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
				///Chunks
				if(NetworkManager.Singleton.IsClient)
					RequestChunkServerRpc(viewedChunkCoord);
                else {
					TerrainChunk chunk = null;
					if (!World.Chunks.ContainsKey(viewedChunkCoord))
					{
						//Request Chunk
						chunk = BuildChunk(viewedChunkCoord);
						chunk.BuildCollisions();
					}
					else if (World.Chunks.ContainsKey(viewedChunkCoord))
						chunk = World.Chunks[viewedChunkCoord];
					chunk.ChunkVisible = true;
					ChunkCollisionQueue.Enqueue(chunk);
					ChunksVisibleLastUpdate.Add(chunk);
				}
			}
		}


		foreach(TerrainChunk chunk in ChunksVisibleLastUpdate)
        {
			if (Vector2Int.Distance(chunk.ChunkPositionWorldSpace, currentChunkCoord) >= GlobalVariables.WorldData.ChunkDistance * GlobalVariables.WorldData.ChunkWidth)
				if(chunk.ChunkVisible)
                    chunk.ChunkVisible = false;
		}
		ChunksVisibleLastUpdate.Clear();
	}

	public static Vector2Int CastVector2ToInt(Vector2 vectorToCast) => new Vector2Int((int)vectorToCast.x, (int)vectorToCast.y);

	[ServerRpc]
	public void RequestChunkServerRpc(Vector2 position)
	{
		Vector2Int viewedChunkCoord = CastVector2ToInt(position);
		TerrainChunk chunk = null;
		if (!World.Chunks.ContainsKey(viewedChunkCoord))
		{
			//Request Chunk
			chunk = BuildChunk(viewedChunkCoord);
			chunk.BuildCollisions();
		}else if (World.Chunks.ContainsKey(viewedChunkCoord)){
			chunk = World.Chunks[viewedChunkCoord];
		}
		if (chunk != null)
			ReturnChunkClientRpc(Chunk.TransferBlocksToChunk(chunk), position);
		else
			Debug.LogWarning("Chunk is null!");
	}

	[ClientRpc]
	public void ReturnChunkClientRpc(Chunk chunkObj, Vector2 position){
		Vector2Int viewedChunkCoord = CastVector2ToInt(position);
		TerrainChunk terrainChunkToReturn = Chunk.TransferChunkToBlocks(chunkObj, ChunkParent);
		World.Chunks[viewedChunkCoord] = terrainChunkToReturn;
		terrainChunkToReturn.ChunkVisible = true;
		terrainChunkToReturn.BuildCollisions();
		ChunksVisibleLastUpdate.Add(terrainChunkToReturn);
	}

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