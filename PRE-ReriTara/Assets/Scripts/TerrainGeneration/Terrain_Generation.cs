using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;

public class Terrain_Generation: MonoBehaviour
{
    // Start is called before the first frame update

    public int width, height;
    public float maxsmoothness,seed;
    public bool seedrandomness;
    [Header("Tile Settings")]
    public TileBase groundTile,topTile;
    public Tilemap groundTilemap;

    
    int[,] map;

    public delegate void Perform(int x, int y);

    public void Start()
    {
        Generation();
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Generation();
    }

    private void Generation()
    {
        groundTilemap.ClearAllTiles();
        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        RenderMap(map, groundTilemap, groundTile , topTile);
    }

    private int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        loopXY((x, y) => {map[x, y] = (empty) ? 0 : 1;});
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

    private int[,] RenderMap(
            int[,] map,
            Tilemap groundTileMap, 
            TileBase groundTilebase , TileBase topTile
            )
    {
        loopXY(
            (x,y) =>
                {
                if (map[x, y] == 1)
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTilebase);
                if (y != 0 && map[x, y - 1] == 1 && map[x, y] == 0)
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), topTile);
                }
            );

        return map;
    }

    private void loopXY(Perform perform)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                perform(x,y);
    }


}