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
            floor = CreateSipleRooms(roomsList);
        }

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (BoundsInt room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintBackgroundTiles(floor);
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

    private HashSet<Vector2Int> CreateSipleRooms(List<BoundsInt> roomList)
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

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        Vector2Int currentRoomCenter = roomCenters[UnityEngine.Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointToCenter(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destinaiton)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        Vector2Int position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destinaiton.y)
        {
            if (destinaiton.y > position.y)
            {
                position += Vector2Int.up;
            }
            else
            if (destinaiton.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position + Vector2Int.left);
            corridor.Add(position);
            corridor.Add(position + Vector2Int.right);
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
            corridor.Add(position + Vector2Int.down);
            corridor.Add(position);
            corridor.Add(position + Vector2Int.up);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointToCenter(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (Vector2Int position in roomCenters)
        {
            float currentDistatnce = Vector2.Distance(position, currentRoomCenter);
            if (currentDistatnce < distance)
            {
                distance = currentDistatnce;
                closest = position;
            }
        }
        return closest;
    }
}
