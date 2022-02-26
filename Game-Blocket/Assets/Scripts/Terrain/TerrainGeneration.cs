using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
				if(DebugVariables.ShowMultipleTasksOrExecution)
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
		try{ 
			Tuple<Vector2Int, GameObject> tuple = obj as Tuple<Vector2Int, GameObject> ?? throw new ArgumentException("Tuple wrong!");
			Vector2Int position = tuple.Item1;
			GameObject parent = tuple.Item2;

			Thread.CurrentThread.Name = ThreadName(position);

			TerrainChunk chunk = new TerrainChunk(position);
			List<Biom> bioms;
			if (position.y > -20)
				bioms = GlobalVariables.WorldAssets.GetBiomsByType(Biomtype.OVERWORLD);
			else
				bioms = GlobalVariables.WorldAssets.GetBiomsByType(Biomtype.UNDERGROUND);

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

			GenerateChunk(chunk, noisemap,caveNoiseMap,oreNoiseMap,biomNoiseMap);
			QueueChunkForImport(chunk, position);
			lock(TerrainGenerationTaskNames)
				TerrainGenerationTaskNames.Remove(ThreadName(position));
		}catch(Exception e){
			Debug.LogError(e);
        }
	}

	/// <summary>Add the ids of the blocks to the blockIDs array</summary>
	/// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
	/// <param name="biomindex">Index of the biom of the chunk</param>
	public static void GenerateChunk(TerrainChunk tc, float[] noisemap, float[,] caveNoisepmap, byte[,] oreNoiseMap, int[,] biomNoiseMap) {
		float caveSize = GlobalVariables.WorldData.InitCaveSize;
		if (tc.chunkPosition.y < 0) {
			caveSize = GlobalVariables.WorldData.InitCaveSize - tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight * 0.001f;
		} else if (tc.ChunkPositionInt.y > 0) {
			caveSize = GlobalVariables.WorldData.InitCaveSize + tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight * 0.001f;
		}

		if (caveSize > 0)
			caveSize = 0;

		for (int x = 0; x < GlobalVariables.WorldData.ChunkWidth; x++) {
			AnimationCurve heightCurve = new AnimationCurve(GlobalVariables.WorldData.Heightcurve.keys);
			int positionHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[x]) * GlobalVariables.WorldData.HeightMultiplier) + 1;

			for (int y = GlobalVariables.WorldData.ChunkHeight - 1; y >= 0; y--) {
				Biom biom = GlobalVariables.WorldAssets.bioms[biomNoiseMap[x, y]];
				if (y + tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight < positionHeight) {
					if (caveNoisepmap[x, y] > caveSize) {
						if (caveNoisepmap[x, y] < caveSize + GlobalVariables.WorldData.StoneSize) {
							tc.blocks[x, y] = biom.StoneBlockId;
						} else {
							foreach (RegionData region in biom.Regions) {
								if (region.RegionRange <= positionHeight - (y + tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight)) {
									tc.blocks[x, y] = region.BlockID;
								}
							}

							foreach (OreData oreData in biom.Ores) {
								if (oreData.BlockID == oreNoiseMap[x, y]) {
									tc.blocks[x, y] = oreNoiseMap[x, y];
								}
							}
						}
					}

					foreach (RegionData regionBG in biom.BgRegions) {
						if (regionBG.RegionRange <= positionHeight - (y + tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight)) {
							tc.bgBlocks[x, y] = regionBG.BlockID;
						}
					}
				}
				//Place Trees.
				if (x % 5 == 0 && tc.ChunkPositionInt.y == 0) {
					//	//try to spawn a Tree
					//GenerateTrees(x, positionHeight, biom.Index);
				}
			}
		}
	}

	public static void QueueChunkForImport(TerrainChunk tc, Vector2Int position) {
		if (position != null)
			lock (TerrainHandler.Chunks) {
				TerrainHandler.Chunks[position] = tc;
			}
		else
			throw new ArgumentException();
		//lock (TerrainHandler.ChunkCollisionQueue) {
		//	TerrainHandler.ChunkCollisionQueue.Enqueue(tc);
		//}

		lock (TerrainHandler.ChunkTileInitializationQueue) {
			TerrainHandler.ChunkTileInitializationQueue.Enqueue(tc);
		}
	}
}