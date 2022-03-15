using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Structure : MonoBehaviour
{
    public byte id;

	public new string name;
    public int probability;
    public int anchorPoint;
    public Tilemap foregroundTilemap;
    public Tilemap backgroundTilemap;
    public bool onSurface;
    public bool belowSurface;
    public bool aboveSurface;
    public bool replaceForeground;
    public bool replaceBackground;
    public bool removeForeground;
    public bool disableFromTo;
    public int from;
    public int to;
    [HideInInspector]
    public byte[,] blocksForeground;
    [HideInInspector]
    public byte[,] blocksBackground;
    [HideInInspector]
    public Vector3Int structureSize;
    [HideInInspector]
    public Vector3Int structurePostionInEditor;
    [HideInInspector]
    public BoundsInt structureSizeForeground;
    [HideInInspector]
    public BoundsInt structureSizeBackground;


    private void Start()
	{
        //ReadStructureFromTilemap();
        this.gameObject.SetActive(false);
    }

	public void ReadStructureFromTilemap() 
    { 
        structureSizeForeground = foregroundTilemap.cellBounds;
        TileBase[] allTilesForeground = foregroundTilemap.GetTilesBlock(structureSizeForeground);
        structureSizeBackground = backgroundTilemap.cellBounds;
        TileBase[] allTilesBackground = backgroundTilemap.GetTilesBlock(structureSizeBackground);

        int xMin = structureSizeForeground.xMin < structureSizeBackground.xMin ? structureSizeForeground.xMin : backgroundTilemap.cellBounds.xMin;
        int xMax = structureSizeForeground.xMax > structureSizeBackground.xMax ? structureSizeForeground.xMax : backgroundTilemap.cellBounds.xMax;

        int yMin = structureSizeForeground.yMin < structureSizeBackground.yMin ? structureSizeForeground.yMin : structureSizeBackground.yMin;
        int yMax = structureSizeForeground.yMax > structureSizeBackground.yMax ? structureSizeForeground.yMax : structureSizeBackground.yMax;

        structureSize = new Vector3Int(xMax - xMin, yMax - yMin, 0);

        if (anchorPoint < 0)
            anchorPoint = 0;
        else 
        if (anchorPoint > structureSize.x)
            anchorPoint = structureSize.x;


        int xPos = structureSizeForeground.position.x < structureSizeBackground.position.x ? structureSizeForeground.position.x : structureSizeBackground.position.x;
        int yPos = structureSizeForeground.position.y < structureSizeBackground.position.y ? structureSizeForeground.position.y : structureSizeBackground.position.y;

        structurePostionInEditor = new Vector3Int(xPos, yPos, 0);

        blocksForeground = new byte[structureSize.x, structureSize.y];
        blocksBackground = new byte[structureSize.x, structureSize.y];

        for (int x = 0; x < structureSizeForeground.size.x; x++)
        {
            for (int y = 0; y < structureSizeForeground.size.y; y++)
            {
                TileBase tileForeground = allTilesForeground[x + y * structureSizeForeground.size.x];
                if (tileForeground != null)
                {
                    blocksForeground[x + Math.Abs(structurePostionInEditor.x - structureSizeForeground.position.x), y + Math.Abs(structurePostionInEditor.y - structureSizeForeground.position.y)] = WorldAssets.Singleton.GetBlockFromTile(tileForeground);
                }
            }
        }
      
        for (int x = 0; x < structureSizeBackground.size.x; x++)
        {
            for(int y = 0; y < structureSizeBackground.size.y; y++)
            {
                TileBase tileBackground = allTilesBackground[x + y * structureSizeBackground.size.x];
                if (tileBackground != null)
                {
                    blocksBackground[x + Math.Abs(structurePostionInEditor.x - structureSizeBackground.position.x), y + Math.Abs(structurePostionInEditor.y - structureSizeBackground.position.y)] = WorldAssets.Singleton.GetBlockFromTile(tileBackground);
                }
            }
        }
    }
}
