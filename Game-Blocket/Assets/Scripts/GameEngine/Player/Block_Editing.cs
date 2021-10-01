using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Block_Editing : MonoBehaviour
{
    /// <summary>
    ///  NOT FUNCTIONAL AT THIS POINT
    ///  CONTACT @CSE19455 FOR FURTHER INFORMATION
    /// </summary>

    public Tilemap tilemap;
    public GameObject player;
    public KeyCode delete;
    public KeyCode create;
    public Grid grid;
    public Camera mainCamera;
    public TileBase selectedBlock;

    // Start is called before the first frame update
    public void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {        
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int coordinate = grid.WorldToCell(mouseWorldPos);
        coordinate.z = 0;
        if (Input.mousePosition.x-959 < -200 || Input.mousePosition.x-959 > 200 )
            return;
        //print(Input.mousePosition.x-959);
        if (Input.GetKeyDown(delete))
        {
            //RemoveTile(tilemap, coordinate);
        }
        if (Input.GetKeyDown(create))
        {
            //CreateTile(tilemap, coordinate, selectedBlock);
        }
    }

    private void RemoveTile(Tilemap tileMap, Vector3Int position)
    {
        tileMap.SetTile(position, null);
    }

    private void CreateTile(Tilemap tileMap, Vector3Int position , TileBase block)
    {
        tileMap.SetTile(position, block);
    }
}
