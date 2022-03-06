using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAlgorithms
{
    /// <summary>
    /// Simple random walk algorithm
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="walkLength"></param>
    /// <returns>Position of background tiles</returns>
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        path.Add(startPosition);
        Vector2Int previosposition = startPosition;

        for (int i = 0; i < walkLength; i++)
        {
            Vector2Int newPosition = previosposition + Direction2d.GetRandomCardinalDirection();
            path.Add(newPosition);
            previosposition = newPosition;
        }
        return path;
    }

    /**public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int CorridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        Vector2Int direction = Direction2d.GetRandomCardinalDirection();
        Vector2Int currentPosition = startPosition;
        corridor.Add(startPosition); 

        for (int i = 0; i < CorridorLength; i++)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }
        return corridor;
    }**/

    /// <summary>
    /// Splits the space in as many pieces as possible 
    /// </summary>
    /// <param name="seed">sets the seed of the dungeon. If the seed is -1 a random seed will be picked</param>
    /// <param name="minWidth"></param>
    /// <param name="roomsQueue"></param>
    /// <param name="room"></param>
    public static List<BoundsInt> BinarySpacePartitioning(int seed, BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        if (seed != -1)
        {
            Random.InitState(seed);
        }
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while (roomsQueue.Count > 0)
        {
            BoundsInt room = roomsQueue.Dequeue();
            if(room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if (Random.value < 0.5f)
                {
                    if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else
                    if(room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else
                    if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int xSplit = Random.Range(1, room.size.x);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int ySplit = Random.Range(1, room.size.y);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }
}

public static class Direction2d
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0, 1), //Up
        new Vector2Int(1, 0), //Right
        new Vector2Int(0, -1), //Down
        new Vector2Int(-1, 0) //Left
    };

    /// <summary>
    /// Returns a random direchtion
    /// </summary>
    /// <returns>random direction</returns>
    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
    }
}

