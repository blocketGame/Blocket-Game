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
    private Tilemap collisionTilemap;

    public void PaintBackgroundTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, backgroundTilemap, Parameters.backgroundTile);
    }

    internal void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingelTile(wallTilemap, Parameters.wallTile, position);
        PaintSingelTile(collisionTilemap, Parameters.collisionTile, position);
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
        collisionTilemap.ClearAllTiles();
    }
}
