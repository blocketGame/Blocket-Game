using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

/// <summary>
/// Author: HyFabi
/// </summary>
public abstract class TerrainHandler : MonoBehaviour {
	public static TerrainHandler Singleton => ServerTerrainHandler.Singleton as TerrainHandler ?? ClientTerrainHandler.Singleton as TerrainHandler ?? null;

	protected readonly byte _updatePayload = 2;
	protected readonly byte _pickUpDist = 2;

	/// <summary>
	/// Server: Chunks = World<br></br>
	/// Client: Cached World
	/// </summary>

	public static Dictionary<Vector2Int, TerrainChunk> Chunks { get; } = new Dictionary<Vector2Int, TerrainChunk>();
	
	public WorldData WD => WorldData.Singleton;

	#region UtilMethods
	
	/// <summary>Returns the chunk the given coordinate is in</summary>
	/// <param name="x">coordinate in a chunk</param>
	/// <returns></returns>
	public TerrainChunk GetChunkFromCoordinate(float x, float y){
		Vector2Int chunkPosition = new Vector2Int(Mathf.FloorToInt(x / WorldAssets.ChunkLength), Mathf.FloorToInt(y / WorldAssets.ChunkLength));

		return Chunks.TryGetValue(chunkPosition, out TerrainChunk chunk) ? chunk : null;
	}

	public static Vector2Int CastVector2ToInt(Vector2 vectorToCast) => new Vector2Int((int)vectorToCast.x, (int)vectorToCast.y);
	
	/// <summary>Returns the block on any coordinate</summary>
	/// <param name="x">x coordinate</param>
	/// <param name="y">y coordinate</param>
	/// <returns></returns>
	public byte GetBlockFormCoordinate(int x, int y)
	{
		ChunkData chunk = GetChunkFromCoordinate(x, y);
		if (chunk != null)
		{
			int chunkX = x - WorldAssets.ChunkLength * chunk.ChunkPositionInt.x;
			int chunkY = y - WorldAssets.ChunkLength * chunk.ChunkPositionInt.y;
			if (chunkX < WorldAssets.ChunkLength && chunkY < WorldAssets.ChunkLength)
			{
				return chunk.blocks[chunkX, chunkY];
			}
		}
		return 1;
		}
	#endregion

	public static void Ping(ulong reciever, object param) => Debug.Log($"Pong {reciever}, param: {param}");
	public static void Ping(ulong reciever) => Debug.Log($"Pong {reciever}");
	public static void Ping() => Debug.Log($"Pong");
	

}