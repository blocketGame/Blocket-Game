using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Terrain_Generation_Script : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] int width, height;
    [SerializeField] float maxsmoothness;
    [SerializeField] float seed;
    public bool seedrandomness;
    [SerializeField] TileBase groundTile;
    [SerializeField] TileBase topTile;
    [SerializeField] Tilemap groundTilemap;
    int[,] map;

    void Start()
    {
        Generation();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Generation();
        }
    }

    void Generation()
    {
        groundTilemap.ClearAllTiles();
        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        RenderMap(map, groundTilemap, groundTile , topTile);
    }

    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (empty) ? 0 : 1;
            }
        }
        return map;
    }

    public int[,] TerrainGeneration(int[,] map)
    {
        if (seedrandomness == true)
            seed = Random.Range(0f, 10000f);

        int perlinHeight;
        float smoothness = 0f;
        for (int x = 0; x < width; x++)
        {
            if (x % 100==1)
            {
                smoothness = Random.Range(maxsmoothness / 30, maxsmoothness);
            }
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height);
            for (int y = 0; y < perlinHeight; y++)
            {
                map[x, y] = 1;
            }
        }
        return map;
    }

    public int[,] RenderMap(int[,] map, Tilemap groundTileMap, TileBase groundTilebase , TileBase topTile)
    {
        //map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                
                if (map[x, y] == 1)
                {
                        groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTilebase);
                }
                if(y!=0&&map[x, y-1] == 1&& map[x, y] == 0)
                {
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), topTile);
                }
            }

        }
        return map;
    }


}