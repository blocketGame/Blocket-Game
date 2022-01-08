using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Threading.Tasks;

///<summary>Author: Cse19455 / Thomas Boigner
/// TODO: Move Worldata here
/// </summary>
public class TerrainGeneration {
	/// <summary>Shortcut</summary>
	private static WorldData WD => GlobalVariables.WorldData;

	public static string ThreadName(Vector2Int position) => $"Build Chunk: {position}";

	public static List<string> TerrainGenerationTaskNames { get; } = new List<string>();


	/// <summary>
	/// TODO
	/// </summary>
	/// <param name="position"></param>
	/// <param name="parent"></param>
	public static void BuildChunk(Vector2Int position, GameObject parent) {
		Task t = null;
		lock (TerrainGenerationTaskNames) { 
			if (TerrainGenerationTaskNames.Contains(ThreadName(position))){
				if(DebugVariables.showMultipleTasksOrExecution)
					Debug.LogWarning($"Tasks exists!: {ThreadName(position)}");
				return;
			}
			
			t = new Task(BuildChunk, new Tuple<Vector2Int, GameObject>(position, parent));
			TerrainGenerationTaskNames.Add(ThreadName(position));
		}
		t?.Start();
	}

	/// <summary>Generates Chunk From Noisemap without any extra consideration</summary>
	private static void BuildChunk(object obj) {
		Tuple<Vector2Int, GameObject> tuple = obj as Tuple<Vector2Int, GameObject> ?? throw new ArgumentException("Tuple wrong!");
		Vector2Int position = tuple.Item1;
		GameObject parent = tuple.Item2;

		Thread.CurrentThread.Name = ThreadName(position);

		TerrainChunk chunk = new TerrainChunk(position, parent);
		List<Biom> bioms;
		if (position.y > -20)
			bioms = WD.GetBiomsByType(Biomtype.OVERWORLD);
		else
			bioms = WD.GetBiomsByType(Biomtype.UNDERGROUND);

		float[] noisemap;
		lock (WD.Noisemaps) {
			if (WD.Noisemaps.ContainsKey(position.x)) {
				noisemap = WD.Noisemaps[position.x];
			} else {
				noisemap = NoiseGenerator.GenerateNoiseMap1D(WD.ChunkWidth, WD.Seed, WD.Scale, WD.Octives, WD.Persistance, WD.Lacurinarity, WD.OffsetX + position.x * WD.ChunkWidth);
				WD.Noisemaps.Add(position.x, noisemap);
			}
		}

		float[,] caveNoiseMap = NoiseGenerator.GenerateNoiseMap2D(WD.ChunkWidth, WD.ChunkHeight, WD.Seed, WD.Scale, WD.Octives, WD.Persistance, WD.Lacurinarity, new Vector2(WD.OffsetX + position.x * WD.ChunkWidth, WD.OffsetY + position.y * WD.ChunkHeight), NoiseGenerator.NoiseMode.snoise);
		byte[,] oreNoiseMap = NoiseGenerator.GenerateOreNoiseMap(WD.ChunkWidth, WD.ChunkHeight, WD.Seed, WD.Scale, WD.Octives, WD.Persistance, WD.Lacurinarity, new Vector2(WD.OffsetX + position.x * WD.ChunkWidth, WD.OffsetY + position.y * WD.ChunkHeight), NoiseGenerator.NoiseMode.snoise, bioms);
		int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(WD.ChunkWidth, WD.ChunkHeight, WD.Seed, WD.Octives, WD.Persistance, WD.Lacurinarity, new Vector2(WD.OffsetX + position.x * WD.ChunkWidth, WD.OffsetY + position.y * WD.ChunkHeight), bioms);

		chunk.GenerateChunk(noisemap,caveNoiseMap,oreNoiseMap,biomNoiseMap);
		QueueChunkForLoad(chunk, position);
		lock(TerrainGenerationTaskNames)
			TerrainGenerationTaskNames.Remove(ThreadName(position));
	}

	public static void QueueChunkForLoad(TerrainChunk tc, Vector2Int position) {
		if (position != null)
			lock (TerrainHandler.Chunks) {
				TerrainHandler.Chunks[position] = tc;
			}
		else
			throw new ArgumentException();
		lock (TerrainHandler.ChunkCollisionQueue) {
			TerrainHandler.ChunkCollisionQueue.Enqueue(tc);
		}
		lock (TerrainHandler.ChunkTileInitializationQueue) {
			TerrainHandler.ChunkTileInitializationQueue.Enqueue(tc);
		}
	}
}