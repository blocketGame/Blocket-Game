using System.Collections.Generic;
using System.Threading;

using MLAPI;
using MLAPI.Messaging;

using static TerrainChunk;
using static TerrainChunk.ChunkList;

using UnityEngine;

/// <summary>
/// Used to generate and display chunks<br></br>
/// <b>TODO: Split up to TerrainHandler and TerrainGeneration</b><br></br>
/// Author: Cse19455 / Thomas Boigner / Fabiii
/// </summary>
public class TerrainGeneration : NetworkBehaviour {

	#region Serialization
	[SerializeField]
	private List<TerrainChunk> chunksVisibleLastUpdate;
	[SerializeField]
	private GameObject chunkParent;

	public List<TerrainChunk> ChunksVisibleLastUpdate { get => chunksVisibleLastUpdate; set => chunksVisibleLastUpdate = value; }
	public GameObject ChunkParent { get => chunkParent; set => chunkParent = value; }
	#endregion

	public WorldData World => GlobalVariables.WorldData;
	public Queue<TerrainChunk> ChunkCollisionQueue { get; set; } = new Queue<TerrainChunk>();

	public static System.Random prng;

	private readonly Queue<TerrainChunk> chunkTileInitializationQueue = new Queue<TerrainChunk>();
	public Dictionary<ulong, Vector3> PlayerLastUpdate { get; } = new Dictionary<ulong, Vector3>();

	public void Awake() {
		GlobalVariables.TerrainGeneration = this;
		ChunksVisibleLastUpdate = new List<TerrainChunk>();
		prng = new System.Random(World.Seed);
	}

	public void FixedUpdate() {
		if (GlobalVariables.World == null)
			return;

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
		HandleChunks();
	}

	private void HandleChunks() {
		if (chunkTileInitializationQueue.Count > 0) {
			lock (chunkTileInitializationQueue) {
				foreach (TerrainChunk terrainChunk in chunkTileInitializationQueue) {
					terrainChunk.PlaceAllTiles();
				}
				chunkTileInitializationQueue.Clear();
			}
		}
		if (ChunkCollisionQueue.Count > 0) {
			lock (ChunkCollisionQueue) {
				foreach (TerrainChunk terrainChunk in ChunkCollisionQueue) {
					terrainChunk.BuildCollisions();
				}
				ChunkCollisionQueue.Clear();
			}
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
		//CheckChunksAroundPlayerNetworked(playerNO);
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
				if (!World.Chunks.ContainsKey(viewedChunkCoord)) {
					//Request Chunk
					/*chunk =*/
					BuildChunk(viewedChunkCoord);
					//World.Chunks[viewedChunkCoord] = chunk;
					//chunk.BuildCollisions();
				} else if (World.Chunks.ContainsKey(viewedChunkCoord)) {
					if (World.Chunks[viewedChunkCoord].ChunkObject == null)
						BuildChunk(World.Chunks[viewedChunkCoord]);
					else {
						if (!World.Chunks[viewedChunkCoord].ChunkVisible)
							World.Chunks[viewedChunkCoord].ChunkVisible = true;
					}
						
				}
			}
		}
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
	private void BuildChunk(Vector2Int position) {
		TerrainChunk chunk = new TerrainChunk(position, ChunkParent);
		ThreadStart threadStart = delegate {
			Thread.CurrentThread.IsBackground = true;
			List<Biom> bioms;
			if (position.y > -20)
				bioms = GlobalVariables.WorldData.GetBiomsByType(Biomtype.OVERWORLD);
			else
				bioms = GlobalVariables.WorldData.GetBiomsByType(Biomtype.UNDERGROUND);

			float[] noisemap;
			lock (GlobalVariables.WorldData.Noisemaps) {
				if (GlobalVariables.WorldData.Noisemaps.ContainsKey(position.x)) {
					noisemap = GlobalVariables.WorldData.Noisemaps[position.x];
				} else {
					noisemap = NoiseGenerator.GenerateNoiseMap1D(World.ChunkWidth, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, World.OffsetX + position.x * World.ChunkWidth);
					GlobalVariables.WorldData.Noisemaps.Add(position.x, noisemap);
				}
			}

			float[,] caveNoiseMap = NoiseGenerator.GenerateNoiseMap2D(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, GlobalVariables.WorldData.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.snoise);
			byte[,] oreNoiseMap = NoiseGenerator.GenerateOreNoiseMap(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Scale, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, GlobalVariables.WorldData.OffsetY + position.y * World.ChunkHeight), NoiseGenerator.NoiseMode.snoise, bioms);
			int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(World.ChunkWidth, World.ChunkHeight, World.Seed, World.Octives, World.Persistance, World.Lacurinarity, new Vector2(World.OffsetX + position.x * World.ChunkWidth, GlobalVariables.WorldData.OffsetY + position.y * World.ChunkHeight), bioms);

			chunk.GenerateChunk(
				  noisemap,
				  caveNoiseMap,
				  oreNoiseMap,
				  biomNoiseMap);
			GenerateFrontendChunk(position, chunk);
		};
		new Thread(threadStart).Start();
	}

	/// <summary>
	/// Used if the Chunk is loaded from a file and no Tilmaps are existing
	/// </summary>
	/// <param name="terrainChunk"></param>
	private void BuildChunk(TerrainChunk terrainChunk) {
		if (terrainChunk.ChunkObject != null)
			return;
		terrainChunk.ChunkObject = terrainChunk.BuildAllChunkLayers(chunkParent);
		new Thread(() => {
			Thread.CurrentThread.IsBackground = true;
			GenerateFrontendChunk(terrainChunk.ChunkPositionWorldSpace, terrainChunk);
			Thread.CurrentThread.Join();
		}).Start();
	}

	/// <summary>
	/// Generates the Frontend of the Chunk
	/// </summary>
	/// <param name="position"></param>
	/// <param name="chunk"></param>
	private void GenerateFrontendChunk(Vector2Int position, TerrainChunk chunk) {
		lock (GlobalVariables.WorldData.Chunks) {
			World.Chunks[position] = chunk;
		}
		//lock (chunksVisibleLastUpdate) {
		//	chunksVisibleLastUpdate.Add(chunk);
		//}
		lock (ChunkCollisionQueue) {
			ChunkCollisionQueue.Enqueue(chunk);
		}
		lock (chunkTileInitializationQueue) {
			chunkTileInitializationQueue.Enqueue(chunk);
		}
	}

}