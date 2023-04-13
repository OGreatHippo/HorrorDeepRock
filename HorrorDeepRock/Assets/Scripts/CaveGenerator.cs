using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaveGenerator : MonoBehaviour
{
    private MeshGenerator meshGen;

    public int width;
    public int height;
    public int length;

    public string seed;
    public bool usingRandomSeed;

    [Range(45, 50)] public int fillPercent;

    int[,,] cave;

    // Start is called before the first frame update
    void Start()
    {
        meshGen = GetComponent<MeshGenerator>();

        GenerateCave();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            GenerateCave();
        }
    }

    private void GenerateCave()
    {
        cave = new int[width, height, length];
        RandomFillCave();

        for (int i = 0; i < 5; i++)
        {
            SmoothCave();
        }

        ProcessCave();

        int wallBorderSize = 1;
        int[,,] wallBorder = new int[width + wallBorderSize * 2, height + wallBorderSize * 2, length + wallBorderSize * 2];

        for (int x = 0; x < wallBorder.GetLength(0); x++)
        {
            for (int y = 0; y < wallBorder.GetLength(1); y++)
            {
                for (int z = 0; z < wallBorder.GetLength(2); z++)
                {
                    if (x >= wallBorderSize && x < width + wallBorderSize && y >= wallBorderSize && y < height + wallBorderSize && z >= wallBorderSize && z < length + wallBorderSize)
                    {
                        wallBorder[x, y, z] = cave[x - wallBorderSize, y - wallBorderSize, z - wallBorderSize];
                    }

                    else
                    {
                        wallBorder[x, y, z] = 1;
                    }
                }
            }
        }

        meshGen.GenerateMesh(wallBorder, 1);
    }

    void ProcessCave()
    {
        List<List<TileCoordinate>> wallRegions = GetRegions(1);

        int wallThresholdSize = 50;

        foreach(List<TileCoordinate> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThresholdSize)
            {
                foreach(TileCoordinate tile in wallRegion)
                {
                    cave[tile.tileX, tile.tileY, tile.tileZ] = 0;
                }
            }
        }

        //List<List<TileCoordinate>> roomRegions = GetRegions(0);

        //int roomThresholdSize = 50;

        //List<Room> survivingRooms = new List<Room>();

        //foreach (List<TileCoordinate> roomRegion in roomRegions)
        //{
        //    if (roomRegion.Count < roomThresholdSize)
        //    {
        //        foreach (TileCoordinate tile in roomRegion)
        //        {
        //            cave[tile.tileX, tile.tileY, tile.tileZ] = 1;
        //        }
        //    }

        //    else
        //    {
        //        survivingRooms.Add(new Room(roomRegion, cave));
        //    }
        //}

        //survivingRooms.Sort();

        //survivingRooms[0].isMainRoom = true;
        //survivingRooms[0].isAccessibleFromMainRoom = true;

        //ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if(forceAccessibilityFromMainRoom)
        {
            foreach(Room room in allRooms)
            {
                if(room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }

                else
                {
                    roomListA.Add(room);
                }
            }
        }

        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;

        TileCoordinate bestTileA = new TileCoordinate();
        TileCoordinate bestTileB = new TileCoordinate();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnection = false;

        foreach (Room roomA in roomListA)
        {
            if(!forceAccessibilityFromMainRoom)
            {
                possibleConnection = false;

                if(roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            


            foreach(Room roomB in roomListB)
            {
                if(roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for(int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        TileCoordinate tileA = roomA.edgeTiles[tileIndexA];
                        TileCoordinate tileB = roomB.edgeTiles[tileIndexB];

                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if(distanceBetweenRooms < bestDistance || !possibleConnection)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnection = true;

                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if(possibleConnection && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnection && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, TileCoordinate tileA, TileCoordinate tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        List<TileCoordinate> line = GetLine(tileA, tileB);

        foreach(TileCoordinate c in line)
        {
            DrawCircle(c, 2);
        }
    }

    void DrawCircle(TileCoordinate c, int r)
    {
        for(int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                for (int z = -r; z <= r; z++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int realX = c.tileX + x;
                        int realY = c.tileY + y;
                        int realZ = c.tileZ + z;

                        if (isInRange(realX, realY, realZ))
                        {
                            cave[realX, realY, realZ] = 0;
                        }
                    }
                }
            }
        }
    }

    List<TileCoordinate> GetLine(TileCoordinate from, TileCoordinate to)
    {
        List<TileCoordinate> line = new List<TileCoordinate>();

        int x = from.tileX;
        int y = from.tileY;
        int z = from.tileZ;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;
        int dz = to.tileZ - from.tileZ;

        bool inverted = false;
        int stepX = Math.Sign(dx);
        int stepY = Math.Sign(dy);
        int stepZ = Math.Sign(dz);

        int longest = Mathf.Max(Mathf.Abs(dx), Mathf.Max(Mathf.Abs(dy), Mathf.Abs(dz)));

        if (longest == Mathf.Abs(dy))
        {
            int temp = dy;
            dy = dx;
            dx = temp;
            inverted = true;
        }
        else if (longest == Mathf.Abs(dz))
        {
            int temp = dz;
            dz = dx;
            dx = temp;
            inverted = true;
        }

        int gradient1 = 2 * dy - dx;
        int gradient2 = 2 * dz - dx;

        int x2 = 0, y2 = 0, z2 = 0;
        for (int i = 0; i <= longest; i++)
        {
            line.Add(new TileCoordinate(x, y, z));

            if (inverted)
            {
                y2 += stepY;
                if (gradient1 >= 0)
                {
                    x2 += stepX;
                    gradient1 -= 2 * dx;
                }
                if (gradient2 >= 0)
                {
                    z2 += stepZ;
                    gradient2 -= 2 * dx;
                }
                gradient1 += 2 * dy;
                gradient2 += 2 * dz;
                x = x2;
                z = z2;
            }
            else
            {
                x2 += stepX;
                if (gradient1 >= 0)
                {
                    y2 += stepY;
                    gradient1 -= 2 * dx;
                }
                if (gradient2 >= 0)
                {
                    z2 += stepZ;
                    gradient2 -= 2 * dx;
                }
                gradient1 += 2 * dy;
                gradient2 += 2 * dz;
                x = x2;
                y = y2;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(TileCoordinate tile)
    {
        return new Vector3(-width / 2 + 0.5f + tile.tileX, 2, -height / 2 + 0.5f + tile.tileY);
    }

    List<List<TileCoordinate>> GetRegions(int tileType)
    {
        List<List<TileCoordinate>> regions = new List<List<TileCoordinate>>();

        int[,,] caveFlags = new int[width, height, length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    if (caveFlags[x, y, z] == 0 && cave[x, y, z] == tileType)
                    {
                        List<TileCoordinate> newRegion = GetRegionTiles(x, y, z);

                        regions.Add(newRegion);

                        foreach (TileCoordinate tile in newRegion)
                        {
                            caveFlags[tile.tileX, tile.tileY, tile.tileZ] = 1;
                        }
                    }
                }
            }
        }

        return regions;
    }

    List<TileCoordinate> GetRegionTiles(int startX, int startY, int startZ)
    {
        List<TileCoordinate> tiles = new List<TileCoordinate>();

        int[,,] caveFlags = new int[width, height, length];

        int tileType = cave[startX, startY, startZ];

        Queue<TileCoordinate> queue = new Queue<TileCoordinate>();

        queue.Enqueue(new TileCoordinate(startX, startY, startZ));
        caveFlags[startX, startY, startZ] = 1;

        while(queue.Count > 0)
        {
            TileCoordinate tile = queue.Dequeue();
            tiles.Add(tile);

            for(int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    for (int z = tile.tileZ - 1; z <= tile.tileZ + 1; z++)
                    {
                        if (isInRange(x, y, z) && (y == tile.tileZ || x == tile.tileX || z == tile.tileZ))
                        {
                            if (caveFlags[x, y, z] == 0 && cave[x, y, z] == tileType)
                            {
                                caveFlags[x, y, z] = 1;

                                queue.Enqueue(new TileCoordinate(x, y, z));
                            }
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool isInRange(int x, int y, int z)
    {
        return x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < length;
    }

    private void RandomFillCave()
    {
        if(usingRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random randomSeed = new System.Random(seed.GetHashCode());

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1 || z == 0 || z == length - 1)
                    {
                        cave[x, y, z] = 1;
                    }
                    else
                    {
                        cave[x, y, z] = (randomSeed.Next(0, 100) < fillPercent) ? 1 : 0;
                    }
                } 
            }
        }
    }

    private void SmoothCave()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    int neighbourWalls = GetWallCount(x, y, z);

                    if (neighbourWalls > 4)
                    {
                        cave[x, y, z] = 1;
                    }

                    else if (neighbourWalls < 4)
                    {
                        cave[x, y, z] = 0;
                    }
                }
            }
        }
    }

    private int GetWallCount(int gridX, int gridY, int gridZ)
    {
        int wallCount = 0;

        for(int nX = gridX - 1; nX <= gridX + 1; nX++)
        {
            for (int nY = gridY - 1; nY <= gridY + 1; nY++)
            {
                for (int nZ = gridZ - 1; nZ <= gridZ + 1; nZ++)
                {
                    if (isInRange(nX, nY, nZ))
                    {
                        if (nX != gridX || nY != gridY)
                        {
                            wallCount += cave[nX, nY, nZ];
                        }
                    }

                    else
                    {
                        wallCount++;
                    }
                }
            }
        }

        return wallCount;
    }
}
