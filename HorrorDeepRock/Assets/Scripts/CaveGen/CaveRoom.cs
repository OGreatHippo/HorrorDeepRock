using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

class CaveRoom : IComparable<CaveRoom>
{
	private List<TileCoordinate> roomTiles;
	private List<TileCoordinate> borderTiles;
	private List<CaveRoom> connectedRooms;
	private int roomSize;
	private bool accessibleToMainRoom;
	private bool isMainRoom;

	public CaveRoom()
	{

	}

	public CaveRoom(List<TileCoordinate> _roomTiles, int[,] cave)
	{
		roomTiles = _roomTiles;
		roomSize = roomTiles.Count;
		connectedRooms = new List<CaveRoom>();

		borderTiles = new List<TileCoordinate>();

		foreach (TileCoordinate tile in roomTiles)
		{
			for (int x = tile.GetTileX() - 1; x <= tile.GetTileX() + 1; x++)
			{
				for (int z = tile.GetTileZ() - 1; z <= tile.GetTileZ() + 1; z++)
				{
					if (x == tile.GetTileX() || z == tile.GetTileZ())
					{
						if (cave[x, z] == 1)
						{
							borderTiles.Add(tile);
						}
					}
				}
			}
		}
	}

	public void SetAccessibleToMainRoom()
	{
		if (!accessibleToMainRoom)
		{
			accessibleToMainRoom = true;
			foreach (CaveRoom connectedRoom in connectedRooms)
			{
				connectedRoom.SetAccessibleToMainRoom();
			}
		}
	}

	public static void SetConnectedRooms(CaveRoom roomA, CaveRoom roomB)
	{
		if (roomA.accessibleToMainRoom)
		{
			roomB.SetAccessibleToMainRoom();
		}
		else if (roomB.accessibleToMainRoom)
		{
			roomA.SetAccessibleToMainRoom();
		}
		roomA.connectedRooms.Add(roomB);
		roomB.connectedRooms.Add(roomA);
	}

	public bool IsConnected(CaveRoom otherRoom)
	{
		return connectedRooms.Contains(otherRoom);
	}

	public int CompareTo(CaveRoom otherRoom)
	{
		return otherRoom.roomSize.CompareTo(roomSize);
	}

	public bool GetMainRoom()
    {
		return isMainRoom;
    }

	public bool SetIsMainRoom(bool _value)
    {
		return isMainRoom = _value;
    }

	public bool GetAccesibleToMainRoom()
    {
		return accessibleToMainRoom;
    }

	public int GetBorderTilesCount()
    {
		return borderTiles.Count;
    }

	public TileCoordinate GetBorderTilesListIndex(int index)
    {
		for(int i = 0; i < borderTiles.Count; i++)
        {
			if(i == index)
            {
				return borderTiles[i];
			}

            else
            {
				continue;
            }
        }

		return null;
    }

	public int GetConnectedRooms()
    {
		return connectedRooms.Count;
    }
}
