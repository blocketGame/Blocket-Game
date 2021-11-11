
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
	public enum NoiseMode
    {
		Terrain,
		Cave,
		Biom
    }

	public static System.Random prng;

	public static float[] GenerateNoiseMap1D(int mapWith, int seed, float scale, int octaves, float persistance, float lacunarity, float offset)
	{
		//Noise
		float[] noiseMap = new float[mapWith];

		//Random
		prng = new System.Random(seed);

		float[] octaveOffsets = new float[octaves];

		float amplitude = 1;

		for (int i = 0; i < octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + offset;

			octaveOffsets[i] = offsetX;

			amplitude *= persistance;
		}

		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		float halfWidth = mapWith / 2f;

		
		for (int x = 0; x < mapWith; x++)
		{
			amplitude = 1;
			float frequency = 1, noiseHeight = 0;
			for (int i = 0; i < octaves; i++)
			{
				float sample = (x - halfWidth + octaveOffsets[i]) / scale * frequency;

				float perlinValue = Unity.Mathematics.noise.snoise(new Vector2(sample, 0));
				noiseHeight += perlinValue * amplitude;

				amplitude *= persistance;
				frequency *= lacunarity;
			}
			noiseMap[x] = Mathf.Clamp(noiseHeight, 0, 1);
		}
		return noiseMap;
	}

	public static float[,] GenerateNoiseMap2D(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NoiseMode noiseMode)
	{
		//Noise
		float[,] noiseMap = new float[mapWidth, mapHeight];

		//Random
		prng = new System.Random(seed);

		Vector2[] octaveOffsets = new Vector2[octaves];

		float amplitude = 1;

		for (int i = 0; i < octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);
		}

		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				amplitude = 1;
				float frequency = 1;
				float noiseHeight = 0;
				for (int i = 0; i < octaves; i++)
				{
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;
					Unity.Mathematics.float2 perlinValue = new Unity.Mathematics.float2(0, 0);

					if (noiseMode == NoiseMode.Terrain)
                    {

						perlinValue = Unity.Mathematics.noise.snoise(new Vector2(sampleX, sampleY));
					}
					else
					if(noiseMode == NoiseMode.Cave|| noiseMode == NoiseMode.Biom)
                    {
						perlinValue = Unity.Mathematics.noise.cellular(new Unity.Mathematics.float2(sampleX, sampleY));
					}
					noiseHeight += perlinValue.x * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}
				noiseMap[x, y] = noiseHeight;
			}
		}
		return noiseMap;
	}

	public static float[,] GenerateBiom(int mapWidth, int mapHeight, int seed, int octaves, float persistance, float lacunarity, Vector2 offset, List<Biom> bioms)
    {
		float[,] biomnoisemaps = new float[mapWidth,mapHeight];

		int offsets = 0;
		foreach (Biom b in bioms)
		{
			float[,] biomn = GenerateNoiseMap2D(mapWidth, mapHeight, (seed+offsets), b.Size, octaves, persistance , lacunarity , offset, NoiseMode.Biom);

			//if (b.Index == 0)
			offsets += 10000;
			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					if (biomn[x, y] >= (0.9f) && (b.Index != 0))
					{
						biomnoisemaps[x, y] = b.Index;
					}
				}
			}
		}
		return biomnoisemaps;
    }
}
