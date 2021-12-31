using UnityEngine;

[SerializeField]
public class Structure : MonoBehaviour
{
	public byte[,] blocks;
	public new string name;

	//Idea 1 give it the byte of used Tiles 

	//Idea 2 search for right Tile
	
	private void Start()
	{
		//ReadStructureFromTilemap();
		//this.gameObject.SetActive(false);
	}

	public void ReadStructureFromTilemap()
	{
		//Tilemap tilemap = GetComponent<Tilemap>();
		//blocks = new byte[tilemap.editorPreviewSize.x, tilemap.editorPreviewSize.y];

		//BoundsInt bounds = tilemap.cellBounds;
		//TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

		//for (int x = 0; x < bounds.size.x; x++)
		//{
		//    for (int y = 0; y < bounds.size.y; y++)
		//    {
		//        TileBase tile = allTiles[x + y * bounds.size.x];
		//        if (tile != null)
		//        {
		//            blocks[x, y] = GlobalVariables.WorldData.getBlockFromTile(tile);
		//            Debug.Log(blocks[x, y]);
		//        }
		//    }
		//}
	}
}
