using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Room : IComparable<Room>
{
    public List<TileCoordinate> tiles;

    public List<TileCoordinate> edgeTiles;

    public List<Room> connectedRooms;

    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room()
    {

    }

    public Room(List<TileCoordinate> roomTiles, int[,,] cave)
    {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();
        edgeTiles = new List<TileCoordinate>();

        foreach (TileCoordinate tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX - 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY - 1; y++)
                {
                    for (int z = tile.tileZ - 1; z <= tile.tileZ - 1; z++)
                    {
                        if (x == tile.tileX || y == tile.tileY || z == tile.tileZ)
                        {
                            if (cave[x, y, z] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!isAccessibleFromMainRoom)
        {
            isAccessibleFromMainRoom = true;

            foreach (Room connectedRoom in connectedRooms)
            {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRooms(Room roomA, Room roomB)
    {
        if (roomA.isAccessibleFromMainRoom)
        {
            roomB.SetAccessibleFromMainRoom();
        }

        else if (roomB.isAccessibleFromMainRoom)
        {
            roomA.SetAccessibleFromMainRoom();
        }

        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom)
    {
        return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room otherRoom)
    {
        return otherRoom.roomSize.CompareTo(roomSize);
    }
}
