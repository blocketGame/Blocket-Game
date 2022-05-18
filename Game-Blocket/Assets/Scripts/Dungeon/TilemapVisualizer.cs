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
    public GameObject enemyGo;

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
            BoundsInt actualRoom = new BoundsInt(new Vector3Int(room.position.x + Parameters.offset, room.position.y + Parameters.offset, room.position.z), new Vector3Int(room.size.x - Parameters.offset * 2, room.size.y - Parameters.offset * 2, 1));
            
            for (int y = 0; y < actualRoom.size.y; y++)
            {
                int platformOffset = Random.Range(0, actualRoom.size.x / 2);
                for (int x = 0; x < (actualRoom.size.x / 2); x++)
                {
                    if((y+1)%Parameters.platformSpaceRoom == 0)
                    {
                        if(x == 0)
                        {
                            PaintSinglePlattform(new Vector2Int(x + platformOffset + actualRoom.position.x, y + actualRoom.position.y), Vector2.left);
                        }
                        else
                        if(x == (actualRoom.size.x / 2) - 1)
                        {
                            PaintSinglePlattform(new Vector2Int(x + platformOffset + actualRoom.position.x, y + actualRoom.position.y), Vector2.right);
                        }
                        else
                        {
                            PaintSinglePlattform(new Vector2Int(x + platformOffset + actualRoom.position.x, y + actualRoom.position.y), Vector2.zero);
                            int id = 0;
                            foreach(GameObject enemy in Parameters.enemies)
                            {
                                if(y + 1 + enemy.transform.localScale.y <= actualRoom.size.y &&
                                    NoiseGenerator.GenerateStructureCoordinates2d(x + platformOffset + actualRoom.position.x, y + actualRoom.position.y + 1, Parameters.seed, Parameters.enemieProbability*Parameters.enemies.Count, id))
                                    Instantiate(enemy, new Vector3Int(x + platformOffset + actualRoom.position.x, y + actualRoom.position.y + 1, 0), Quaternion.identity, enemyGo.transform);
                                id++;
                            }
                        }
                    }
                }
            }
            for(int x = 0; x < actualRoom.size.x; x++)
            {
                int id = 0;
                foreach (GameObject enemy in Parameters.enemies)
                {
                    if (NoiseGenerator.GenerateStructureCoordinates2d(x + actualRoom.position.x, actualRoom.position.y, Parameters.seed, Parameters.enemieProbability * Parameters.enemies.Count, id))
                        Instantiate(enemy, new Vector3Int(x + actualRoom.position.x, actualRoom.position.y, 0), Quaternion.identity, enemyGo.transform);
                    id++;
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
