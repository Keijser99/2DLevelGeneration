using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;

    [SerializeField]
    private int lastRoomCenterNumber;

    public GameObject exitTilePrefab;
    public GameObject player;

    public GameObject chestPrefab;
    [Range(1, 10)]
    public int maxChestSpawnRange;
    int chestCount;

    //Function from the abstact class AbstractDungeonGenerator
    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void Awake()
    {
        GenerateDungeon();
    }

    /*
     * This function receives the room positions that are created in the ProceduralBSPLevelGenerator
     * and paints the correct tiles from the tile map with the TilemapVisualizer script
    */
    private void CreateRooms()
    {
        var roomList = ProceduralBSPLevelGenerator.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        /* 
         * A HashSet works the same way as a Dictionary, but without a value for each key. 
         * It's a collection of keys and you can quickly test if a value is part of the set or not. 
        */
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        floor = CreateSimpleRooms(roomList);

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        //Places player and the exit tile on first and last generated room respectively
        player.transform.position = new Vector3(roomCenters[0].x, roomCenters[0].y, 0f);
        SpawnPrefab(exitTilePrefab, new Vector2Int(roomCenters[roomCenters.Count - 1].x, roomCenters[roomCenters.Count - 1].y));


        ChestScatterer(roomCenters);

        //This part connects the floor parts with the corridor parts to generate the corridors later
        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    //Defines a (as straight as possible) path between rooms that are next to eachother
    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[UnityEngine.Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    //Creates the corridor that was defined in the ConnectRooms function
    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    //Searches for rooms that are next to eachother
    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    /*
     * This function determines the positions of the generated rooms to let the CreateRooms 
     * function place the tiles on the correct positions
    */
    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private void SpawnPrefab(GameObject objectToSpawn, Vector2Int placeToSpawn)
    {
        Instantiate(objectToSpawn, new Vector3(placeToSpawn.x, placeToSpawn.y, 0f), Quaternion.identity);
    }

    private void ChestScatterer(List<Vector2Int> roomCenters)
    {
        int randomValue;

        foreach (var roomCenter in roomCenters)
        {
            randomValue = UnityEngine.Random.Range(0, maxChestSpawnRange);
            if (randomValue < maxChestSpawnRange / 2)
            {
                //don't spawn
            }
            if (randomValue >= maxChestSpawnRange / 2)
            {
                int chestSpawnHandicapX = UnityEngine.Random.Range(-3, 3);
                int chestSpawnHandicapY = UnityEngine.Random.Range(-3, 3);

                if (chestSpawnHandicapX != 0 || chestSpawnHandicapY != 0)
                {
                    SpawnPrefab(chestPrefab, new Vector2Int(roomCenter.x + chestSpawnHandicapX, roomCenter.y + chestSpawnHandicapY));
                    chestCount++;
                }
            }
        }
        Debug.Log($"Amount of chests spawned: {chestCount}");
    }
}
