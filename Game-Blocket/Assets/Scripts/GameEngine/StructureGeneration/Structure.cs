using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : MonoBehaviour
{
    public byte[,] blocks;
    public string name;

    //Should be removed (Test case)
    public WorldData world;


    //Idea 1 give it the byte of used Tiles 

    //Idea 2 search for right Tile

    private void Awake()
    {
        ReadStructureFromTilemap();
        this.gameObject.SetActive(false);
    }

    public void ReadStructureFromTilemap()
    {
        Tilemap t =this.GetComponent<Tilemap>();
        blocks = new byte[t.editorPreviewSize.x, t.editorPreviewSize.y];
        Debug.Log(t.editorPreviewSize);

        ///PROBLEM : Tilemap starts to count in the middle

        for(int x = 0; x < t.editorPreviewSize.x; x++)
        {
            for (int y = 0; y < t.editorPreviewSize.y; y++)
            {
                if (t.GetTile(new Vector3Int(x, y, 0)) != null)
                {
                    blocks[x, y] = world.getBlockFromTile(t.GetTile(new Vector3Int(x, y, 0)));
                }
            }
        }
    }
}
