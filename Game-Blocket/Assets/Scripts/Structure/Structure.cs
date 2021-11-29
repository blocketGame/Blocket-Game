using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

[SerializeField]
public class Structure : MonoBehaviour
{
    public byte[,] blocks;
    public new string name;



    //Idea 1 give it the byte of used Tiles 

    //Idea 2 search for right Tile
    

    public void Instantiate()
    {
        try
        {
            ReadFromFile();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            //ReadStructureFromTilemap();
        }
    }

    /// <summary>
    /// DON'T DELETE (USE IF NEW STRUCTURE IS NOT SAVED YET)
    /// Build won't work if activated
    /// </summary>
    
    /*
    private void ReadStructureFromTilemap()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        blocks = new byte[tilemap.editorPreviewSize.x, tilemap.editorPreviewSize.y];

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    blocks[x, y] = GlobalVariables.WorldData.getBlockFromTile(tile);
                    //Debug.Log(blocks[x, y]);
                }
            }
        }
        WriteInFile();
    }

    private void WriteInFile()
    {
        string s = $"{ GetComponent<Tilemap>().editorPreviewSize.x},{ GetComponent<Tilemap>().editorPreviewSize.y}\n";
        for(int x=0;x<blocks.GetLength(1);x++)
            for(int y = 0; y < blocks.GetLength(0); y++)
            {
                s += blocks[y, x] + "\n";
                if (y == blocks.GetLength(0) - 1) 
                    s += ".\n";
            }
                
        File.WriteAllText($"Docs/Structure{name}.txt", s);
    }*/

    private void ReadFromFile()
    {
        string[] lines = System.IO.File.ReadAllLines($"Docs/Structure{name}.txt");
        char c = ',';
        blocks = new byte[int.Parse(lines[0].Split(c)[0]), int.Parse(lines[0].Split(c)[1])];
        Debug.Log(blocks.GetLength(1));
        int x = 0;
        int y = 0;
        for(int i=1;i < lines.Length; i++)
        {
            if (lines[i].Equals("."))
            {
                y++;
                x = 0;
            }
            else
            {
                blocks[x, y] = byte.Parse(lines[i]);
                Debug.Log(blocks[x, y]);
                x++;
            }
                
        }



    }
}
