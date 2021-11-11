using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using MLAPI.Messaging;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class TerrainGeneration : MonoBehaviour {

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

	public void Awake() {
		ChunksVisibleLastUpdate = new List<TerrainChunk>();
		//World.putBlocksIntoTxt();
		//World.putBiomsIntoTxt();
		prng = new System.Random(World.Seed);
	}

	public void FixedUpdate() {
		UpdateChunks();
		foreach(TerrainChunk tc in ChunkCollisionQueue) {
			tc.BuildCollisions();
		}
		ChunkCollisionQueue.Clear();
	}

	public void UpdateChunks() {
		foreach(TerrainChunk t in ChunksVisibleLastUpdate) {
			t.SetChunkState(false);
		}
		ChunksVisibleLastUpdate.Clear();
		CheckChunksAroundPlayer(GlobalVariables.PlayerPos);
	}

	/// <summary>
	/// Activates and deactivates Chunks
	/// </summary>
	public void CheckChunksAroundPlayer(Vector3 playerPos) {
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(playerPos.x / World.ChunkWidth), Mathf.RoundToInt(playerPos.y / World.ChunkHeight));

		for(int xOffset = -World.ChunkDistance; xOffset < World.ChunkDistance; xOffset++) {
			for(int yOffset = -World.ChunkDistance; yOffset < World.ChunkDistance; yOffset++) {
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
				if(!World.Chunks.ContainsKey(viewedChunkCoord)) {
					//Request Chunk
					BuildChunk(viewedChunkCoord);
				} else if(World.Chunks.ContainsKey(viewedChunkCoord)) {
					World.Chunks[viewedChunkCoord].SetChunkState(true);
					ChunksVisibleLastUpdate.Add(World.Chunks[viewedChunkCoord]);
				}
			}
		}
	}

	/// <summary>
	/// Checks whether or not a Biom is complete 
	/// (Biomnr/Biomsize)
	/// </summary>

	/// <summary>
	///     Generates Chunk From Noisemap without any extra consideration
	/// </summary>
	[ServerRpc]
	private void BuildChunk(Vector2Int position) {
		TerrainChunk chunk = new TerrainChunk(position, World, ChunkParent, null);
		List<Biom> bioms;
		if(position.y > -20)
			bioms = World.GetBiomsByType(Biomtype.OVERWORLD);
		else
			bioms = World.GetBiomsByType(Biomtype.UNDERGROUND);
		chunk.GenerateChunk(
			  NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth),
			  NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, World.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.Cave),
			  NoiseGenerator.generateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, World.OffsetY + position.y * World.ChunkHeight), bioms));
		World.Chunks[position] = chunk;
		ChunksVisibleLastUpdate.Add(chunk);
		ChunkCollisionQueue.Enqueue(chunk);
	}
}