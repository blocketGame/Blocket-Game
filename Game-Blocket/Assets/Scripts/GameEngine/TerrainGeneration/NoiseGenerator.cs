using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
	public enum NoiseMode
    {
		Terrain,
		Cave
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

				float perlinValue = ((Unity.Mathematics.noise.snoise(new Vector2(sample, 0)) + 1) / 2) * 2 - 1;
				noiseHeight += perlinValue * amplitude;

				amplitude *= persistance;
				frequency *= lacunarity;
			}
			noiseMap[x] = Mathf.Clamp(noiseHeight, 0, 1);
		}
		return noiseMap;
	}

	public static float[,] GenerateNoiseMap2D(int mapWith, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NoiseMode noiseMode)
	{
		//Noise
		float[,] noiseMap = new float[mapWith, mapHeight];

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

		float halfWidth = mapWith / 2f;
		float halfHeight = mapHeight / 2f;

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWith; x++)
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
						perlinValue = ((Unity.Mathematics.noise.snoise(new Vector2(sampleX, sampleY)) + 1) / 2) * 2 - 1;
					}
					else
					if(noiseMode == NoiseMode.Cave)
                    {
						perlinValue = Unity.Mathematics.noise.cellular(new Unity.Mathematics.float2(sampleX, sampleY)) * 2 - 1;
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
}
