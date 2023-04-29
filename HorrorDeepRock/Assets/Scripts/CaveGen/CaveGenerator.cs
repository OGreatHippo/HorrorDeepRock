using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaveGenerator : MonoBehaviour
{
    private MeshGenerator meshGen;

    private int width;
    private int height;

    public string seed;
    public bool usingRandomSeed;

    private int fillPercent = 48;

    int[,] cave;

    public GameObject Player;
    private bool playerSpawned;

    public GameObject Enemy;
    private bool enemySpawned;

    // Start is called before the first frame update
    void Start()
    {
        meshGen = transform.Find("CaveCreator").GetComponent<MeshGenerator>();

        //GenerateCave();
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.P))
        //{
        //    GenerateCave();
        //}
    }

    public void GenerateCave(Vector3 position, int size)
    {
        width = size;
        height = size;

        cave = new int[width, height];
        RandomFillCave();

        for(int i = 0; i < 5; i++)
        {
            SmoothCave();
        }

        ProcessCave();

        int wallBorderSize = 1;
        int[,] wallBorder = new int[width + wallBorderSize * 2, height + wallBorderSize * 2];

        for (int x = 0; x < wallBorder.GetLength(0); x++)
        {
            for (int y = 0; y < wallBorder.GetLength(1); y++)
            {
                if(x >= wallBorderSize && x < width + wallBorderSize && y >= wallBorderSize && y < height + wallBorderSize)
                {
                    wallBorder[x, y] = cave[x - wallBorderSize, y - wallBorderSize];
                }

                else
                {
                    wallBorder[x, y] = 1;
                }
            }
        }

        meshGen.GenerateMesh(cave, 1);
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
                    cave[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<TileCoordinate>> roomRegions = GetRegions(0);

        int roomThresholdSize = 50;

        List<Room> survivingRooms = new List<Room>();

        foreach (List<TileCoordinate> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (TileCoordinate tile in roomRegion)
                {
                    cave[tile.tileX, tile.tileY] = 1;
                }
            }

            else
            {
                survivingRooms.Add(new Room(roomRegion, cave));
            }
        }

        survivingRooms.Sort();

        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
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
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        TileCoordinate tileA = roomA.edgeTiles[tileIndexA];
                        TileCoordinate tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
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
        foreach (TileCoordinate c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(TileCoordinate c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInRange(drawX, drawY))
                    {
                        cave[drawX, drawY] = 0;
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

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new TileCoordinate(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(TileCoordinate tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<TileCoordinate>> GetRegions(int tileType)
    {
        List<List<TileCoordinate>> regions = new List<List<TileCoordinate>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && cave[x, y] == tileType)
                {
                    List<TileCoordinate> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (TileCoordinate tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<TileCoordinate> GetRegionTiles(int startX, int startY)
    {
        List<TileCoordinate> tiles = new List<TileCoordinate>();
        int[,] mapFlags = new int[width, height];
        int tileType = cave[startX, startY];

        Queue<TileCoordinate> queue = new Queue<TileCoordinate>();
        queue.Enqueue(new TileCoordinate(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            TileCoordinate tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && cave[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new TileCoordinate(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }


    bool IsInRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    private void RandomFillCave()
    {
        int random = UnityEngine.Random.Range(0, 10000000);

        if(usingRandomSeed)
        {
            seed = random.ToString();
        }

        System.Random randomSeed = new System.Random(seed.GetHashCode());

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    cave[x, y] = 1;
                }
                else
                {
                    cave[x, y] = (randomSeed.Next(0, 100) < fillPercent) ? 1 : 0;
                }

                //if (cave[x, y] == 0 && !playerSpawned)
                //{
                //    Instantiate(Player, new Vector3(x + 5, -5, y + 5), Quaternion.Euler(0, 145, 0));
                //    playerSpawned = true;
                //}
            }
        }

        //for (int x = width - 1; x > 0; x--)
        //{
        //    for (int y = height - 1; y > 0; y--)
        //    {
        //        if (cave[x, y] == 0 && !enemySpawned)
        //        {
        //            Instantiate(Enemy, new Vector3(x / 2 - 5, -5, y / 2 - 5), Quaternion.Euler(0, -145, 0));
        //            enemySpawned = true;
        //        }
        //    }
        //}
    }

    void SmoothCave()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetWallCount(x, y);

                if (neighbourWallTiles > 4)
                    cave[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    cave[x, y] = 0;

            }
        }
    }

    int GetWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += cave[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
}
