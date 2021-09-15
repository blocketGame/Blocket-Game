using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;

/*
 * @Author : Cse19455 / Thomas Boigner
 */
public class Terrain_Generation: MonoBehaviour
{
    //public variables
    public int width, height;
    public float maxsmoothness,seed;
    public bool seedrandomness;

    [Header("Tile Settings")]
    public TileBase groundTile,topTile;
    public Tilemap groundTilemap;

    //private variables
    private int[,] map;

    //delegates
    public delegate void loopContent(int x, int y);


    public void Start()
    {
        Generation();
    }

    public void Update()
    {
        //Generation();
    }

    private void Generation()
    {
        groundTilemap.ClearAllTiles();
        map = GenerateArray(width, height);
        map = TerrainGeneration(map);
        RenderMap(map);
    }

    private int[,] GenerateArray(int width, int height)
    {
        int[,] map = new int[width, height];
        loopXY((x, y) => {map[x, y] = 0;});
        return map;
    }

    private int[,] TerrainGeneration(int[,] map)
    {
        if (seedrandomness == true)
            seed = UnityEngine.Random.Range (0f, 10000f);
        
        for (int x = 0; x < width; x++)
        {
            int perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / maxsmoothness, seed) * height);
            for (int y = 0; y < perlinHeight; y++)
                map[x, y] = 1;
        }
        
        return map;
    }

    private int[,] RenderMap(int[,] map)
    {
        loopXY(
            (x,y) =>
                {
                    if (map[x, y] == 1)
                        PlaceTile(x, y, groundTile);
                    else if (y != 0 && map[x, y - 1] == 1)
                        PlaceTile(x, y, topTile);
                }
            );
        return map;
    }

    /*
     * Automatition functions
     */

    private void loopXY(loopContent function)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                function(x,y);
    }

    
    private void PlaceTile(int x,int y,TileBase tile)=> groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
}