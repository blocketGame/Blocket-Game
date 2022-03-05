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
	}

	/// <summary>Add the ids of the blocks to the blockIDs array</summary>
	/// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
	/// <param name="biomindex">Index of the biom of the chunk</param>
	public static void GenerateChunk(TerrainChunk tc, float[] noisemap, float[,] caveNoisepmap, byte[,] oreNoiseMap, int[,] biomNoiseMap) {
		GenerateStructureCoordinates(tc);
		
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
			int positionHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[x]) * GlobalVariables.WorldData.HeightMultiplier);

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
				GenerateStructures(x, y, tc);
			}
		}
	}

	public static void GenerateStructureCoordinates(TerrainChunk tc)
	{
		foreach (Structure structure in GlobalVariables.Structures.structures)
		{
			if (structure.onSurface && tc.chunkPosition.y == 0)
			{
				for (int x = -structure.structureSize.x; x < GlobalVariables.WorldData.ChunkWidth + structure.structureSize.x; x++)
					if (NoiseGenerator.GenerateStructureCoordinates1d(GlobalVariables.WorldData.ChunkWidth * tc.ChunkPositionInt.x + x, GlobalVariables.WorldData.Seed, structure.probability, structure.id))
					{
						AnimationCurve heightCurve = new AnimationCurve(GlobalVariables.WorldData.Heightcurve.keys);
						float[] noisemap = NoiseGenerator.GenerateNoiseMap1D(1, GlobalVariables.WorldData.Seed, GlobalVariables.WorldData.Scale, GlobalVariables.WorldData.Octives, GlobalVariables.WorldData.Persistance, GlobalVariables.WorldData.Lacurinarity, GlobalVariables.WorldData.OffsetX + tc.chunkPosition.x * GlobalVariables.WorldData.ChunkWidth + x);
						int y = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[0]) * GlobalVariables.WorldData.HeightMultiplier);

						Biomtype biomType;
						if (y / GlobalVariables.WorldData.ChunkHeight > -20)
							biomType = Biomtype.OVERWORLD;
						else
							biomType = Biomtype.UNDERGROUND;

						int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(1, 1, GlobalVariables.WorldData.Seed, GlobalVariables.WorldData.Octives, GlobalVariables.WorldData.Persistance, GlobalVariables.WorldData.Lacurinarity, new Vector2(GlobalVariables.WorldData.OffsetX + tc.ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth + x, GlobalVariables.WorldData.OffsetY + tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight + y), GlobalVariables.WorldAssets.GetBiomsByType(biomType));
						Biom b = GlobalVariables.WorldAssets.bioms[biomNoiseMap[0, 0]];
						if (Array.Exists(b.Structures, id => id == structure.id))
						{
							if (!tc.structureCoordinates.ContainsKey(structure.id))
								tc.structureCoordinates.Add(structure.id, new List<Vector2Int>());

							tc.structureCoordinates[structure.id].Add(new Vector2Int(x, y));
						}
					}
			}
			else
			if (structure.belowSurface || structure.aboveSurface)
				for (int x = -structure.structureSize.x; x < GlobalVariables.WorldData.ChunkWidth + structure.structureSize.x; x++)
					for (int y = -structure.structureSize.y; y < GlobalVariables.WorldData.ChunkHeight + structure.structureSize.y; y++)
						if (NoiseGenerator.GenerateStructureCoordinates2d(GlobalVariables.WorldData.ChunkWidth * tc.ChunkPositionInt.x + x, GlobalVariables.WorldData.ChunkHeight * tc.ChunkPositionInt.y + y, GlobalVariables.WorldData.Seed, structure.probability, structure.id))
						{
							AnimationCurve heightCurve = new AnimationCurve(GlobalVariables.WorldData.Heightcurve.keys);
							float[] noisemap = NoiseGenerator.GenerateNoiseMap1D(1, GlobalVariables.WorldData.Seed, GlobalVariables.WorldData.Scale, GlobalVariables.WorldData.Octives, GlobalVariables.WorldData.Persistance, GlobalVariables.WorldData.Lacurinarity, GlobalVariables.WorldData.OffsetX + tc.chunkPosition.x * GlobalVariables.WorldData.ChunkWidth + x);
							int terrainHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[0]) * GlobalVariables.WorldData.HeightMultiplier);

							Biomtype biomType;
							if (y / GlobalVariables.WorldData.ChunkHeight > -20)
								biomType = Biomtype.OVERWORLD;
							else
								biomType = Biomtype.UNDERGROUND;

							int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(1, 1, GlobalVariables.WorldData.Seed, GlobalVariables.WorldData.Octives, GlobalVariables.WorldData.Persistance, GlobalVariables.WorldData.Lacurinarity, new Vector2(GlobalVariables.WorldData.OffsetX + tc.ChunkPositionInt.x * GlobalVariables.WorldData.ChunkWidth + x, GlobalVariables.WorldData.OffsetY + tc.ChunkPositionInt.y * GlobalVariables.WorldData.ChunkHeight + y), GlobalVariables.WorldAssets.GetBiomsByType(biomType));
							Biom b = GlobalVariables.WorldAssets.bioms[biomNoiseMap[0, 0]];
							if (Array.Exists(b.Structures, id => id == structure.id))
							{
                                if (structure.disableFromTo ||
								   (y + tc.chunkPosition.y * GlobalVariables.WorldData.ChunkHeight < structure.from &&
									y + tc.chunkPosition.y * GlobalVariables.WorldData.ChunkHeight > structure.to))
                                {
									if ((y + tc.chunkPosition.y * GlobalVariables.WorldData.ChunkHeight < terrainHeight && structure.belowSurface) ||
										(y + tc.chunkPosition.y * GlobalVariables.WorldData.ChunkHeight > terrainHeight && structure.aboveSurface))
									{
										if (!tc.structureCoordinates.ContainsKey(structure.id))
											tc.structureCoordinates.Add(structure.id, new List<Vector2Int>());

										tc.structureCoordinates[structure.id].Add(new Vector2Int(x, y));
									}
                                }
							}
						}
		}
	}
	public static void GenerateStructures(int x, int y, TerrainChunk tc)
	{
		foreach (Structure structure in GlobalVariables.Structures.structures)
		{
			if (tc.structureCoordinates.ContainsKey(structure.id))
			{
				foreach (Vector2Int structurePosition in tc.structureCoordinates[structure.id])
				{
					if (structurePosition.x - structure.anchorPoint < x &&
						structurePosition.x + structure.structureSize.x - structure.anchorPoint + 1 > x &&
						structurePosition.y + structure.structureSize.y > y &&
						structurePosition.y <= y)
					{						
						byte blockIdForeground = structure.blocksForeground[(int)(x - (structurePosition.x - structure.anchorPoint + 1)), (int)(y - structurePosition.y)];
						if (blockIdForeground != 0 && (structure.replaceForeground || tc.blocks[x, y] == 0))
							tc.blocks[x, y] = blockIdForeground;

						byte blockIdBackground = structure.blocksBackground[(int)(x - (structurePosition.x - structure.anchorPoint + 1)), (int)(y - structurePosition.y)];
						if (blockIdBackground != 0 && (structure.replaceBackground || tc.bgBlocks[x, y] == 0))
                        {
							tc.bgBlocks[x, y] = blockIdBackground;
                            if (structure.removeForeground && blockIdForeground == 0)
                            {
								tc.blocks[x, y] = 0;
                            }
                        }
					}
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