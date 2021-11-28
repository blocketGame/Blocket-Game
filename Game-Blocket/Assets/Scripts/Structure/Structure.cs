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
    
    private void Start()
    {
        try
        {
           ReadFromFile();
        }
        catch
        {
           ReadStructureFromTilemap();
        }
        //this.gameObject.SetActive(false);
    }

    public void ReadStructureFromTilemap()
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
                    Debug.Log(blocks[x, y]);
                }
            }
        }
        WriteInFile();

    }

    private void WriteInFile()
    {
        string s = "";
        for(int x=0;x<blocks.GetLength(1);x++)
            for(int y = 0; y < blocks.GetLength(0); y++)
            {
                s += blocks[y, x] + "\n";
                if (y == blocks.GetLength(0) - 1) 
                    s += ".\n";
            }
                
        File.WriteAllText($"Docs/Structure{name}.txt", s);
        Debug.Log("WRITE");
    }

    private void ReadFromFile()
    {
        Debug.Log("READ");
        
        string[] lines = System.IO.File.ReadAllLines($"Docs/Structure{name}.txt");
        int x = 0;
        int y = 0;
        for(int i=0;i < lines.Length; i++)
        {
            if (lines[i].Equals("."))
            {
                y++;
                x = 0;
            }else
                blocks[x, y] = Encoding.ASCII.GetBytes(lines[i])[0];
            x++;
        }


    }
}
