using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    //Parameters
    [SerializeField]
    private DungeonSO parameters;
    [SerializeField]
    private TilemapVisualizer tilemapVisualizer;
    [HideInInspector]
    public Vector3 startposition;

    /// <summary>
    /// Starting point of the dungeongenertation
    /// </summary>
    public void GenerateDungeon()
    {
        tilemapVisualizer.Parameters = parameters;

        tilemapVisualizer.Clear();
        List<BoundsInt> roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(parameters.seed, new BoundsInt((Vector3Int)parameters.startPosition, new Vector3Int(parameters.dungeonWidth, parameters.dungeonHeight, 0)), parameters.minRoomWidth, parameters.minRoomHeight);
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (parameters.randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomsList);
        }
        else
        {
            floor = CreateSimpleRooms(roomsList);
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomsList);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintBackgroundTiles(floor);
        tilemapVisualizer.PaintPlattfroms(roomsList);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);


    }

    /// <summary>
    /// Creates Rooms using the random walk algorithm
    /// </summary>
    /// <param name="roomsList"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            BoundsInt roomBounds = roomsList[i];
            Vector2Int roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            HashSet<Vector2Int> roomFloor = RunRandomWalk(parameters, roomCenter);
            foreach (Vector2Int position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + parameters.offset) && position.x <= (roomBounds.xMax - parameters.offset) && position.y >= (roomBounds.yMin + parameters.offset) && position.y <= (roomBounds.yMax - parameters.offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    /// <summary>
    /// Random walk algorithm
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="positon"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> RunRandomWalk(DungeonSO parameters, Vector2Int positon)
    {
        Vector2Int currentPosition = positon;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < parameters.iterations; i++)
        {
            HashSet<Vector2Int> path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            floorPositions.UnionWith(path);
            if (parameters.startRandomlyEachIteration)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }
        }
        return floorPositions;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (BoundsInt room in roomList)
        {
            for (int col = parameters.offset; col < room.size.x - parameters.offset; col++)
            {
                for (int row = parameters.offset; row < room.size.y - parameters.offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<BoundsInt> roomsList)
    {
        List<BoundsInt> temporalRoomsList = new List<BoundsInt>(roomsList);
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        BoundsInt currentRoom = temporalRoomsList[Random.Range(0, temporalRoomsList.Count)];
        startposition = new Vector3Int(currentRoom.position.x + parameters.offset + 1, currentRoom.position.y + parameters.offset + 1, currentRoom.position.z);
        temporalRoomsList.Remove(currentRoom);

        while (temporalRoomsList.Count > 0)
        {
            BoundsInt closest = FindClosestPointToCenter(currentRoom, temporalRoomsList);
            temporalRoomsList.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoom, closest, roomsList);
            currentRoom = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(BoundsInt currentRoom, BoundsInt closest, List<BoundsInt> roomsList)
    {
        Vector2Int currentRoomCenter = Vector2Int.RoundToInt(currentRoom.center);
        Vector2Int destinaiton = Vector2Int.RoundToInt(closest.center);

        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        Vector2Int position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destinaiton.y)
        {
            if (destinaiton.y > position.y)
            {
                position += Vector2Int.up;
                if (!IsInRoom(position, roomsList))
                {
                    if (position.y % parameters.platformSpace == 0)
                    {
                        tilemapVisualizer.PaintSinglePlattform(position, Vector2.zero);

                        for (int i = 0; i < parameters.corridorWidth - 1; i++)
                        {
                            tilemapVisualizer.PaintSinglePlattform(position + i * Vector2Int.left, Vector2.zero);
                            tilemapVisualizer.PaintSinglePlattform(position + i * Vector2Int.right, Vector2.zero);
                        }
                        tilemapVisualizer.PaintSinglePlattform(position + (parameters.corridorWidth - 1) * Vector2Int.left, Vector2.left);
                        tilemapVisualizer.PaintSinglePlattform(position + (parameters.corridorWidth - 1) * Vector2Int.right, Vector2.right);
                    }
                }
            }
            else
            if (destinaiton.y < position.y)
            {
                position += Vector2Int.down;
                if (!IsInRoom(position, roomsList))
                {
                    if (position.y % parameters.platformSpace == 0)
                    {
                        tilemapVisualizer.PaintSinglePlattform(position, Vector2.zero);

                        for (int i = 0; i < parameters.corridorWidth - 1; i++)
                        {
                            tilemapVisualizer.PaintSinglePlattform(position + i * Vector2Int.left, Vector2.zero);
                            tilemapVisualizer.PaintSinglePlattform(position + i * Vector2Int.right, Vector2.zero);
                        }
                        tilemapVisualizer.PaintSinglePlattform(position + (parameters.corridorWidth - 1) * Vector2Int.left, Vector2.left);
                        tilemapVisualizer.PaintSinglePlattform(position + (parameters.corridorWidth - 1) * Vector2Int.right, Vector2.right);
                    }
                }
            }
            corridor.Add(position);
            for (int i = 0; i < parameters.corridorWidth; i++)
            {
                corridor.Add(position + i * Vector2Int.left);
                corridor.Add(position + i * Vector2Int.right);
            }
        }
        while (position.x != destinaiton.x)
        {
            if (destinaiton.x > position.x)
            {
                position += Vector2Int.right;
            }
            else
            if (destinaiton.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
            for (int i = 0; i < parameters.corridorWidth; i++)
            {
                corridor.Add(position + i * Vector2Int.up);
                corridor.Add(position + i * Vector2Int.down);
            }
        }
        return corridor;
    }

    public bool IsInRoom(Vector2Int position, List<BoundsInt> roomsList)
    {
        foreach(BoundsInt room in roomsList)
        {
            BoundsInt actualRoom = new BoundsInt(new Vector3Int(room.position.x + parameters.offset, room.position.y + parameters.offset, room.position.z), new Vector3Int(room.size.x - parameters.offset * 2, room.size.y - parameters.offset * 2, 1));
            if (actualRoom.Contains(new Vector3Int(position.x, position.y, 0)))
                return true;
        }
        return false;
    }

    private BoundsInt FindClosestPointToCenter(BoundsInt currentRoom, List<BoundsInt> roomsList)
    {
        BoundsInt closest = currentRoom;
        float distance = float.MaxValue;
        foreach (BoundsInt room in roomsList)
        {
            float currentDistatnce = Vector2.Distance(Vector2Int.RoundToInt(room.center), Vector2Int.RoundToInt(currentRoom.center));
            if (currentDistatnce < distance)
            {
                distance = currentDistatnce;
                closest = room;
            }
        }
        return closest;
    }
}
