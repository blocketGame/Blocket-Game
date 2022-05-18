using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="SimpleRandomWalk_", menuName = "PCG/SimpleRandomWalkData")]
public class DungeonSO : ScriptableObject
{
    //Dungeon Generation
    public Vector2Int startPosition = Vector2Int.zero;
    public int seed = -1;
    public int iterations = 10, walkLength = 10;
    public bool startRandomlyEachIteration = true;
    public int minRoomWidth = 4, minRoomHeight = 4;
    public int dungeonWidth = 20, dungeonHeight = 20;
    [Range(0, 10)]
    public int offset = 1;
    public bool randomWalkRooms = false;
    public List<GameObject> gameObjects = new List<GameObject> ();
    [Range(0, 20)]
    public int corridorWidth;
    [Range(1, 20)]
    public int platformSpaceRoom;
    [Range(1, 20)]
    public int platformSpaceCorridor;

    //Tilemapvisulizer
    public TileBase backgroundTile;
    public TileBase wallTile;
    public TileBase plattformMiddleTile;
    public TileBase plattformRightTile;
    public TileBase plattformLeftTile;

    //Mobhandling
    public List<GameObject> enemies;
    [Range(0, 50)]
    public int enemieProbability;
    public GameObject boss;
}
