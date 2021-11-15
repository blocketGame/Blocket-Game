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


        for(int x = -1; x < t.editorPreviewSize.x; x++)
        {
            for (int y = -1; y < t.editorPreviewSize.y; y++)
            {
                if (t.GetTile(new Vector3Int(x, y, 0)) != null)
                {
                    blocks[0, 0] = world.getBlockFromTile(t.GetTile(new Vector3Int(x, y, 0)));
                }
            }
        }

        //Debug.Log(blocks[t.editorPreviewSize.x - 1, t.editorPreviewSize.y - 1]);

        Vector3 v = t.GetCellCenterLocal(new Vector3Int(0, 0, 0));
        t.SetTile(new Vector3Int((int)v.x,(int)v.y,(int)v.z), world.Blocks[4].Tile);
        
    }
}
