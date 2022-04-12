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

	public static string ThreadName(Vector2Int position) => $"Build Chunk: {position}";

	public static List<string> TerrainGenerationTaskNames { get; } = new List<string>();


	/// <summary>
	/// TODO
	/// </summary>
	/// <param name="position"></param>
	/// <param name="parent"></param>
	public static void BuildChunk(Vector2Int position) {
		Task t = null;
		lock(TerrainHandler.Chunks)
			if(TerrainHandler.Chunks.ContainsKey(position))
				return;
		lock(TerrainGenerationTaskNames) {
			if(TerrainGenerationTaskNames.Contains(ThreadName(position))) {
				if(DebugVariables.ShowMultipleTasksOrExecution)
					Debug.LogWarning($"Tasks exists!: {ThreadName(position)}");
				return;
			}

			t = new Task(BuildChunk, position);
			TerrainGenerationTaskNames.Add(ThreadName(position));
		}
		t?.Start();
	}

	/// <summary>Generates Chunk From Noisemap without any extra consideration</summary>
	private static void BuildChunk(object obj) {
		Vector2Int position = (Vector2Int)obj;
		try {
			lock(TerrainHandler.Chunks)
				if(TerrainHandler.Chunks.ContainsKey(position))
					return;
			try{ 
				Thread.CurrentThread.Name = ThreadName(position);
			}catch(InvalidOperationException ioE){
				Debug.LogWarning(ioE);
            }

			TerrainChunk chunk = new TerrainChunk(position);
			List<Biom> bioms;
			if(position.y > WorldData.Singleton.SkyBiomStartChunk)
				bioms = WorldAssets.Singleton.GetBiomsByType(Biomtype.FLYING);
			else
			if(position.y > WorldData.Singleton.UndergroundBiomStartChunk)
				bioms = WorldAssets.Singleton.GetBiomsByType(Biomtype.OVERWORLD);
			else
				bioms = WorldAssets.Singleton.GetBiomsByType(Biomtype.UNDERGROUND);

			float[] noisemap;
			lock(WorldData.Singleton.Noisemaps) {
				if(WorldData.Singleton.Noisemaps.ContainsKey(position.x)) {
					noisemap = WorldData.Singleton.Noisemaps[position.x];
				} else {
					noisemap = NoiseGenerator.GenerateNoiseMap1D(WorldAssets.ChunkLength, WorldData.Singleton.Seed, WorldData.Singleton.Scale, WorldData.Singleton.Octives, WorldData.Singleton.Persistance, WorldData.Singleton.Lacurinarity, WorldData.Singleton.OffsetX + position.x * WorldAssets.ChunkLength);
					WorldData.Singleton.Noisemaps.Add(position.x, noisemap);
				}
			}

			float[,] caveNoiseMap = NoiseGenerator.GenerateNoiseMap2D(WorldAssets.ChunkLength, WorldAssets.ChunkHeight, WorldData.Singleton.Seed, WorldData.Singleton.UndergroundScale, WorldData.Singleton.UndergroundOctives, WorldData.Singleton.UndergroundPersistance, WorldData.Singleton.UndergroundLacurinarity, new Vector2(WorldData.Singleton.UndergroundOffsetX + position.x * WorldAssets.ChunkLength, WorldData.Singleton.UndergroundOffsetY + position.y * WorldAssets.ChunkHeight));
			byte[,] oreNoiseMap = NoiseGenerator.GenerateOreNoiseMap(WorldAssets.ChunkLength, WorldAssets.ChunkHeight, WorldData.Singleton.Seed, WorldData.Singleton.UndergroundScale, WorldData.Singleton.UndergroundOctives, WorldData.Singleton.UndergroundPersistance, WorldData.Singleton.UndergroundLacurinarity, new Vector2(WorldData.Singleton.UndergroundOffsetX + position.x * WorldAssets.ChunkLength, WorldData.Singleton.UndergroundOffsetY + position.y * WorldAssets.ChunkHeight), bioms);
			int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(WorldAssets.ChunkLength, WorldAssets.ChunkHeight, WorldData.Singleton.Seed, WorldData.Singleton.BiomOctives, WorldData.Singleton.BiomPersistance, WorldData.Singleton.BiomLacurinarity, new Vector2(WorldData.Singleton.BiomOffsetX + position.x * WorldAssets.ChunkLength, WorldData.Singleton.BiomOffsetY + position.y * WorldAssets.ChunkHeight), bioms);
			float[,] skyNoiseMap = NoiseGenerator.GenerateNoiseMap2D(WorldAssets.ChunkLength, WorldAssets.ChunkHeight, WorldData.Singleton.Seed, WorldData.Singleton.SkyScale, WorldData.Singleton.SkyOctives, WorldData.Singleton.SkyPersistance, WorldData.Singleton.SkyLacurinarity, new Vector2(WorldData.Singleton.SkyOffsetX + position.x * WorldAssets.ChunkLength, WorldData.Singleton.SkyOffsetY + position.y * WorldAssets.ChunkHeight));

			GenerateChunk(chunk, noisemap, caveNoiseMap, oreNoiseMap, biomNoiseMap, skyNoiseMap);
			lock(TerrainHandler.Chunks)
				if(!TerrainHandler.Chunks.ContainsKey(position))
					TerrainHandler.Chunks.Add(position, chunk);
			
		} catch(Exception e) {
			Debug.LogError(e);
		}finally{ 
			lock(TerrainGenerationTaskNames)
				TerrainGenerationTaskNames.Remove(ThreadName(position));
		}
	}

	/// <summary>Add the ids of the blocks to the blockIDs array</summary>
	/// <param name="noisemap">Noisemap that determines the hight of hills and mountains</param>
	/// <param name="biomindex">Index of the biom of the chunk</param>
	public static void GenerateChunk(TerrainChunk tc, float[] noisemap, float[,] caveNoisemap, byte[,] oreNoiseMap, int[,] biomNoiseMap, float[,] skyNoiseMap) {
		GenerateStructureCoordinates(tc);

		float caveSize = WorldData.Singleton.InitCaveSize - tc.ChunkPositionInt.y * WorldAssets.ChunkHeight * WorldData.Singleton.CaveSizeIncrease;
		if(caveSize < WorldData.Singleton.InitCaveSize)
			caveSize = WorldData.Singleton.InitCaveSize;
		
		if (caveSize > 0.4f)
			caveSize = 0.4f;

		float ilandSize;
		if(tc.chunkPosition.y > WorldData.Singleton.SkyBiomStartChunk)
        {
			ilandSize = WorldData.Singleton.IlandInitSize + tc.ChunkPositionInt.y * WorldAssets.ChunkHeight * WorldData.Singleton.IlandSizeIncrease;
			if(ilandSize < WorldData.Singleton.IlandInitSize)
				ilandSize = WorldData.Singleton.IlandInitSize;

			if (ilandSize > 0.3f)
				ilandSize = 0.3f;
        }
        else
        {
			ilandSize = WorldData.Singleton.IlandInitSize;
		}

		for (int x = 0; x < WorldAssets.ChunkLength; x++) {
			AnimationCurve heightCurve = new AnimationCurve(WorldData.Singleton.Heightcurve.keys);
			int positionHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[x]) * WorldData.Singleton.HeightMultiplier);

			for (int y = WorldAssets.ChunkHeight - 1; y >= 0; y--)
			{
				Biom biom = WorldAssets.Singleton.bioms[biomNoiseMap[x, y]];
				if (y + tc.ChunkPositionInt.y * WorldAssets.ChunkHeight < positionHeight)
				{

					foreach (RegionData regionBG in biom.Regions)
					{
						if (regionBG.RegionRange <= positionHeight - (y + tc.ChunkPositionInt.y * WorldAssets.ChunkHeight))
						{
							tc.bgBlocks[x, y] = regionBG.BlockID;
						}
					}

					if (caveNoisemap[x, y] > caveSize)
					{
						if (caveNoisemap[x, y] < caveSize + WorldData.Singleton.StoneSize)
						{
							tc.blocks[x, y] = biom.StoneBlockId;
							tc.bgBlocks[x, y] = biom.StoneBlockIdBg;
						}
						else
						{
							foreach (RegionData region in biom.Regions)
							{
								if (region.RegionRange <= positionHeight - (y + tc.ChunkPositionInt.y * WorldAssets.ChunkHeight))
								{
									tc.blocks[x, y] = region.BlockID;
								}
							}

							foreach (OreData oreData in biom.Ores)
							{
								if (oreData.BlockID == oreNoiseMap[x, y])
								{
									tc.blocks[x, y] = oreNoiseMap[x, y];
								}
							}
						}
					}
				}
				else
				if (skyNoiseMap[x, y] < ilandSize && biom.Biomtype.Contains(Biomtype.FLYING))
				{
					if (skyNoiseMap[x, y] > ilandSize - WorldData.Singleton.OutsideSize)
					{
						if (caveNoisemap[x, y] > caveSize)
							tc.blocks[x, y] = biom.SkyBlockOutsideId;
						tc.bgBlocks[x, y] = biom.SkyBlockOutsideBg;
					}
					else
					{
						if (caveNoisemap[x, y] > caveSize)
							tc.blocks[x, y] = biom.SkyBlockInsideId;
						tc.bgBlocks[x, y] = biom.SkyBlockInsideIdBg;
					}
					if (caveNoisemap[x, y] > caveSize)
						foreach (OreData oreData in biom.Ores)
						{
							if (oreData.BlockID == oreNoiseMap[x, y])
							{
								tc.blocks[x, y] = oreNoiseMap[x, y];
							}
						}
				}
				GenerateStructures(x, y, tc);
			}
		}
	}

	public static void GenerateStructureCoordinates(TerrainChunk tc) {
		foreach(Structure structure in StructureAssets.Singleton.Structures) {
			if(structure.onSurface) {
				for(int x = -structure.structureSize.x; x < WorldAssets.ChunkLength + structure.structureSize.x; x++)
					if(NoiseGenerator.GenerateStructureCoordinates1d(WorldAssets.ChunkLength * tc.ChunkPositionInt.x + x, WorldData.Singleton.Seed, structure.probability, structure.id)) {
						AnimationCurve heightCurve = new AnimationCurve(WorldData.Singleton.Heightcurve.keys);
						float[] noisemap = NoiseGenerator.GenerateNoiseMap1D(1, WorldData.Singleton.Seed, WorldData.Singleton.Scale, WorldData.Singleton.Octives, WorldData.Singleton.Persistance, WorldData.Singleton.Lacurinarity, WorldData.Singleton.OffsetX + tc.chunkPosition.x * WorldAssets.ChunkLength + x);
						int y = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[0]) * WorldData.Singleton.HeightMultiplier);

						Biomtype biomType;
						if (y / WorldAssets.ChunkHeight > WorldData.Singleton.SkyBiomStartChunk)
							biomType = Biomtype.FLYING;
						else
						if (y / WorldAssets.ChunkHeight > WorldData.Singleton.UndergroundBiomStartChunk)
							biomType = Biomtype.OVERWORLD;
						else
							biomType = Biomtype.UNDERGROUND;

						int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(1, 1, WorldData.Singleton.Seed, WorldData.Singleton.Octives, WorldData.Singleton.Persistance, WorldData.Singleton.Lacurinarity, new Vector2(WorldData.Singleton.OffsetX + tc.ChunkPositionInt.x * WorldAssets.ChunkLength + x, WorldData.Singleton.OffsetY + y), WorldAssets.Singleton.GetBiomsByType(biomType));
						Biom b = WorldAssets.Singleton.bioms[biomNoiseMap[0, 0]];
						if(Array.Exists(b.Structures, id => id == structure.id)) {
							if(!tc.structureCoordinates.ContainsKey(structure.id))
								tc.structureCoordinates.Add(structure.id, new List<Vector2Int>());

							tc.structureCoordinates[structure.id].Add(new Vector2Int(x, y - tc.ChunkPositionInt.y * WorldAssets.ChunkHeight));
						}
					}
			} else
			if(structure.belowSurface || structure.aboveSurface)
				for(int x = -structure.structureSize.x; x < WorldAssets.ChunkLength + structure.structureSize.x; x++)
					for(int y = -structure.structureSize.y; y < WorldAssets.ChunkHeight + structure.structureSize.y; y++)
						if(NoiseGenerator.GenerateStructureCoordinates2d(WorldAssets.ChunkLength * tc.ChunkPositionInt.x + x, WorldAssets.ChunkHeight * tc.ChunkPositionInt.y + y, WorldData.Singleton.Seed, structure.probability, structure.id)) {
							AnimationCurve heightCurve = new AnimationCurve(WorldData.Singleton.Heightcurve.keys);
							float[] noisemap = NoiseGenerator.GenerateNoiseMap1D(1, WorldData.Singleton.Seed, WorldData.Singleton.Scale, WorldData.Singleton.Octives, WorldData.Singleton.Persistance, WorldData.Singleton.Lacurinarity, WorldData.Singleton.OffsetX + tc.chunkPosition.x * WorldAssets.ChunkLength + x);
							int terrainHeight = Mathf.FloorToInt(heightCurve.Evaluate(noisemap[0]) * WorldData.Singleton.HeightMultiplier);

							Biomtype biomType;
							if(y / WorldAssets.ChunkHeight > -20)
								biomType = Biomtype.OVERWORLD;
							else
								biomType = Biomtype.UNDERGROUND;

							int[,] biomNoiseMap = NoiseGenerator.GenerateBiom(1, 1, WorldData.Singleton.Seed, WorldData.Singleton.Octives, WorldData.Singleton.Persistance, WorldData.Singleton.Lacurinarity, new Vector2(WorldData.Singleton.OffsetX + tc.ChunkPositionInt.x * WorldAssets.ChunkLength + x, WorldData.Singleton.OffsetY + tc.ChunkPositionInt.y * WorldAssets.ChunkHeight + y), WorldAssets.Singleton.GetBiomsByType(biomType));
							Biom b = WorldAssets.Singleton.bioms[biomNoiseMap[0, 0]];
							if(Array.Exists(b.Structures, id => id == structure.id)) {
								if(structure.disableFromTo ||
								   (y + tc.chunkPosition.y * WorldAssets.ChunkHeight < structure.from &&
									y + tc.chunkPosition.y * WorldAssets.ChunkHeight > structure.to)) {
									if((y + tc.chunkPosition.y * WorldAssets.ChunkHeight < terrainHeight && structure.belowSurface) ||
										(y + tc.chunkPosition.y * WorldAssets.ChunkHeight > terrainHeight && structure.aboveSurface)) {
										if(!tc.structureCoordinates.ContainsKey(structure.id))
											tc.structureCoordinates.Add(structure.id, new List<Vector2Int>());

										tc.structureCoordinates[structure.id].Add(new Vector2Int(x, y));
									}
								}
							}
						}
		}
	}

	public static void GenerateStructures(int x, int y, TerrainChunk tc) {
		foreach(Structure structure in StructureAssets.Singleton.Structures) {
			if(tc.structureCoordinates.ContainsKey(structure.id)) {
				foreach(Vector2Int structurePosition in tc.structureCoordinates[structure.id]) {
					if(structurePosition.x - structure.anchorPoint < x &&
						structurePosition.x + structure.structureSize.x - structure.anchorPoint + 1 > x &&
						structurePosition.y + structure.structureSize.y > y &&
						structurePosition.y <= y) {
						byte blockIdForeground = structure.blocksForeground[(int)(x - (structurePosition.x - structure.anchorPoint + 1)), (int)(y - structurePosition.y)];
						if(blockIdForeground != 0 && (structure.replaceForeground || tc.blocks[x, y] == 0))
							tc.blocks[x, y] = blockIdForeground;

						byte blockIdBackground = structure.blocksBackground[(int)(x - (structurePosition.x - structure.anchorPoint + 1)), (int)(y - structurePosition.y)];
						if(blockIdBackground != 0 && (structure.replaceBackground || tc.bgBlocks[x, y] == 0)) {
							tc.bgBlocks[x, y] = blockIdBackground;
							if(structure.removeForeground && blockIdForeground == 0)
								tc.blocks[x, y] = 0;
						}
					}
				}
			}
		}
	}
}