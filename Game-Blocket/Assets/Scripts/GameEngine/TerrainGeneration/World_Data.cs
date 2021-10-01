using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class World_Data : MonoBehaviour
{

    public GameObject player;
    public Terrain_Generation terraingeneration;
    public TerrainChunk[] chunk;
    public Biom[] biom;
    public int chunkDistance;
    public int seed;
    public float scale;
    public int octives;
    public float persistance;
    public float lacurinarity;
    public float offsetX;
    public int heightMultiplier;



    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void GetChunkFromCoordinate(int x)
    {

    }
}

public struct Biom
{
    public string name;
    public OreData[] ores;
    public RegionData[] regions;
    public AnimationCurve heightCurve;
    public float noiseValueFrom;
    public float noiseValueTo;
}

public struct OreData
{
    public string name;
    public float noiseValueFrom;
    public float noiseValueTo;
    public byte blockID;
}

public struct RegionData
{
    public string name;
    public int startHeight;
    public byte blockID;
}
