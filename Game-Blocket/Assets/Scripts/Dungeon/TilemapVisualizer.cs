using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    public DungeonSO Parameters { get; set; }
    [SerializeField]
    private Tilemap backgroundTilemap;
    [SerializeField]
    private Tilemap wallTilemap;
    [SerializeField]
    private Tilemap platformTilemap;

    public void PaintBackgroundTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, backgroundTilemap, Parameters.backgroundTile);
    }

    public void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingelTile(wallTilemap, Parameters.wallTile, position);
    }

    public void PaintSinglePlattform(Vector2Int position, Vector2 direction)
    {
        if(direction == Vector2.left)
            PaintSingelTile(platformTilemap, Parameters.plattformLeftTile, position);
        if(direction == Vector2.right)
            PaintSingelTile(platformTilemap, Parameters.plattformRightTile, position);
        if(direction == Vector2.zero)
            PaintSingelTile(platformTilemap, Parameters.plattformMiddleTile, position);
    }

    public void PaintPlattfroms(List<BoundsInt> rooms)
    {
        foreach (BoundsInt room in rooms)
        {
            for (int y = Parameters.offset; y < room.size.y - Parameters.offset; y++)
            {
                for (int x = Parameters.offset; x < room.size.x - Parameters.offset; x++)
                {
                    if(y%Parameters.platformSpace == 0)
                    {
                        if(x == Parameters.offset)
                        {
                            PaintSinglePlattform(new Vector2Int(x + room.position.x, y + room.position.y), Vector2.left);
                        }
                        else
                        if(x == room.size.x - Parameters.offset - 1)
                        {
                            PaintSinglePlattform(new Vector2Int(x + room.position.x, y + room.position.y), Vector2.right);
                        }
                        else
                        {
                            PaintSinglePlattform(new Vector2Int(x + room.position.x, y + room.position.y), Vector2.zero);
                        }
                    }
                }
            }
        }
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingelTile(tilemap, tile, position);
        }
    }

    private void PaintSingelTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        backgroundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        platformTilemap.ClearAllTiles();
    }
}
