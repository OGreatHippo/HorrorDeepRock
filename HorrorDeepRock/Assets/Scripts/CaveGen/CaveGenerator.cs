using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaveGenerator : MonoBehaviour
{
    private MeshGenerator meshGen;

    private int width;
    private int length;
    private string seed;

    private int fillPercent = 48;

    private int[,] cave;

    public GameObject Player;
    private bool playerSpawned;

    public GameObject Enemy;
    private bool enemySpawned;

    private int size;

    private bool spawnPlayer;
    private bool infiniteGen;

    // Start is called before the first frame update
    private void Start()
    {
        meshGen = transform.Find("CaveCreator").GetComponent<MeshGenerator>();

        GenerateCave(size);
    }

    public void GenerateCave(int size)
    {
        width = size;
        length = size;

        cave = new int[width, length];

        RandomFillCave();

        for(int i = 0; i < 5; i++)
        {
            SmoothCave();
        }

        ProcessCave();

        ConnectChunks();

        int wallBorderSize = 1;
        int[,] wallBorder = new int[width + wallBorderSize * 2, length + wallBorderSize * 2];

        for (int x = 0; x < wallBorder.GetLength(0); x++)
        {
            for (int z = 0; z < wallBorder.GetLength(1); z++)
            {
                if(x >= wallBorderSize && x < width + wallBorderSize && z >= wallBorderSize && z < length + wallBorderSize)
                {
                    wallBorder[x, z] = cave[x - wallBorderSize, z - wallBorderSize];
                }

                else if(x == 0 && z == length / 2 || x == width && z == length / 2 || x == width / 2 && length == 0 || x == width / 2 && z == length)
                {
                    wallBorder[x, z] = 1;
                }

                else
                {
                    wallBorder[x, z] = 0;
                }
            }
        }

        meshGen.GenerateMesh(wallBorder, 1);
    }

    private void ProcessCave()
    {
        List<List<TileCoordinate>> wallRegions = FindRegions(1);

        int wallSize = 50;

        foreach(List<TileCoordinate> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallSize)
            {
                foreach(TileCoordinate tile in wallRegion)
                {
                    cave[tile.GetTileX(), tile.GetTileZ()] = 0;
                }
            }   
        }

        List<List<TileCoordinate>> roomRegions = FindRegions(0);

        int roomSize = 50;

        List<CaveRoom> remainingRooms = new List<CaveRoom>();

        foreach (List<TileCoordinate> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomSize)
            {
                foreach (TileCoordinate tile in roomRegion)
                {
                    cave[tile.GetTileX(), tile.GetTileZ()] = 1;
                }
            }

            else
            {
                remainingRooms.Add(new CaveRoom(roomRegion, cave));
            }
        }

        remainingRooms.Sort();

        remainingRooms[0].SetIsMainRoom(true);
        remainingRooms[0].SetAccessibleToMainRoom();

        ConnectRooms(remainingRooms);
    }

    private void ConnectRooms(List<CaveRoom> rooms, bool forceAccessibilityToMainRoom = false)
    {
        List<CaveRoom> unConnectedRooms = new List<CaveRoom>();
        List<CaveRoom> connectedRooms = new List<CaveRoom>();

        if (forceAccessibilityToMainRoom)
        {
            foreach (CaveRoom room in rooms)
            {
                if (room.GetAccesibleToMainRoom())
                {
                    connectedRooms.Add(room);
                }
                else
                {
                    unConnectedRooms.Add(room);
                }
            }
        }
        else
        {
            unConnectedRooms = rooms;
            connectedRooms = rooms;
        }

        int shortestDistance = 0;
        TileCoordinate bestTileInA = new TileCoordinate();
        TileCoordinate bestTileInB = new TileCoordinate();
        CaveRoom closestRoomA = new CaveRoom();
        CaveRoom closestRoomB = new CaveRoom();
        bool connectionFound = false;

        foreach (CaveRoom roomA in unConnectedRooms)
        {
            if (!forceAccessibilityToMainRoom)
            {
                connectionFound = false;

                if (roomA.GetConnectedRooms() > 0)
                {
                    continue;
                }
            }

            foreach (CaveRoom roomB in connectedRooms)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.GetBorderTilesCount(); tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.GetBorderTilesCount(); tileIndexB++)
                    {
                        TileCoordinate tileA = roomA.GetBorderTilesListIndex(tileIndexA);
                        TileCoordinate tileB = roomB.GetBorderTilesListIndex(tileIndexB);

                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.GetTileX() - tileB.GetTileX(), 2) + Mathf.Pow(tileA.GetTileZ() - tileB.GetTileZ(), 2));

                        if (distanceBetweenRooms < shortestDistance || !connectionFound)
                        {
                            shortestDistance = distanceBetweenRooms;
                            connectionFound = true;
                            bestTileInA = tileA;
                            bestTileInB = tileB;
                            closestRoomA = roomA;
                            closestRoomB = roomB;
                        }
                    }
                }
            }

            if (connectionFound && !forceAccessibilityToMainRoom)
            {
                CreatePath(closestRoomA, closestRoomB, bestTileInA, bestTileInB);
            }
        }

        if (connectionFound && forceAccessibilityToMainRoom)
        {
            CreatePath(closestRoomA, closestRoomB, bestTileInA, bestTileInB);
            ConnectRooms(rooms, true);
        }

        if (!forceAccessibilityToMainRoom)
        {
            ConnectRooms(rooms, true);
        }
    }

    private void ConnectChunks()
    {
        TileCoordinate topMostTile = new TileCoordinate();
        TileCoordinate rightMostTile = new TileCoordinate();
        TileCoordinate bottomMostTile = new TileCoordinate();
        TileCoordinate leftMostTile = new TileCoordinate();

        TileCoordinate topTile = new TileCoordinate();
        TileCoordinate rightTile = new TileCoordinate();
        TileCoordinate bottomTile = new TileCoordinate();
        TileCoordinate leftTile = new TileCoordinate();

        int highestPoint = 0;
        int rightMostPoint = 0;

        bool bottomTileSet = false;
        bool leftTileSet = false;

        List<TileCoordinate> line;
        List<TileCoordinate> bline;
        List<TileCoordinate> rline;
        List<TileCoordinate> lline;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if(x == width / 2)
                {
                    bottomTile.SetTileX(x);
                    bottomTile.SetTileZ(0);

                    topMostTile.SetTileX(x);

                    if (cave[x, z] == 0 && !bottomTileSet)
                    {
                        bottomMostTile.SetTileX(x);
                        bottomMostTile.SetTileZ(z);

                        bottomTileSet = true;
                    }

                    if(cave[x, z] == 0 && z > highestPoint)
                    {
                        highestPoint = z;
                    }

                    if(z == length - 1)
                    {
                        topTile.SetTileX(x);
                        topTile.SetTileZ(z);
                    }
                }

                else if(z == length / 2)
                {
                    leftTile.SetTileX(0);
                    leftTile.SetTileZ(z);

                    rightMostTile.SetTileZ(z);

                    if (cave[x, z] == 0 && !leftTileSet)
                    {
                        leftMostTile.SetTileX(x);
                        leftMostTile.SetTileZ(z);

                        leftTileSet = true;
                    }

                    if (cave[x, z] == 0 && x > rightMostPoint)
                    {
                        rightMostPoint = x;
                    }

                    if(x == width - 1)
                    {
                        rightTile.SetTileX(x);
                        rightTile.SetTileZ(z);
                    }
                }
            }
        }

        topMostTile.SetTileZ(highestPoint);
        rightMostTile.SetTileX(rightMostPoint);

        line = CreateLine(topMostTile, topTile);
        bline = CreateLine(bottomMostTile, bottomTile);
        rline = CreateLine(rightMostTile, rightTile);
        lline = CreateLine(leftMostTile, leftTile);

        foreach (TileCoordinate c in rline)
        {
            CreateEmptySpace(c, 2);
        }

        foreach (TileCoordinate c in lline)
        {
            CreateEmptySpace(c, 2);
        }

        foreach (TileCoordinate c in line)
        {
            CreateEmptySpace(c, 2);
        }

        foreach (TileCoordinate c in bline)
        {
            CreateEmptySpace(c, 2);
        }

        //if (gameObject.transform.position.x < 0)
        //{
        //    rline = CreateLine(rightMostTile, rightTile);

        //    foreach (TileCoordinate c in rline)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }
        //}

        //if (gameObject.transform.position.x > 0)
        //{
        //    lline = CreateLine(leftMostTile, leftTile);

        //    foreach (TileCoordinate c in lline)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }
        //}

        //if (gameObject.transform.position.x == 0)
        //{
        //    rline = CreateLine(rightMostTile, rightTile);
        //    lline = CreateLine(leftMostTile, leftTile);

        //    foreach (TileCoordinate c in rline)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }

        //    foreach (TileCoordinate c in lline)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }
        //}

        //if (gameObject.transform.position.z < 0)
        //{
        //    line = CreateLine(topMostTile, topTile);

        //    foreach (TileCoordinate c in line)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }
        //}

        //if (gameObject.transform.position.z > 0)
        //{
        //    bline = CreateLine(bottomMostTile, bottomTile);

        //    foreach (TileCoordinate c in bline)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }
        //}

        //if (gameObject.transform.position.z == 0)
        //{
        //    line = CreateLine(topMostTile, topTile);
        //    bline = CreateLine(bottomMostTile, bottomTile);

        //    foreach (TileCoordinate c in line)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }

        //    foreach (TileCoordinate c in bline)
        //    {
        //        CreateEmptySpace(c, 2);
        //    }
        //}
    }

    private void CreatePath(CaveRoom roomA, CaveRoom roomB, TileCoordinate tileA, TileCoordinate tileB)
    {
        CaveRoom.SetConnectedRooms(roomA, roomB);
       
        List<TileCoordinate> line = CreateLine(tileA, tileB);

        foreach (TileCoordinate c in line)
        {
            CreateEmptySpace(c, 5);
        }
    }

    private void CreateEmptySpace(TileCoordinate c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int z = -r; z <= r; z++)
            {
                if (x * x + z * z <= r * r)
                {
                    int drawX = c.GetTileX() + x;
                    int drawZ = c.GetTileZ() + z;
                    if (IsInRange(drawX, drawZ))
                    {
                        cave[drawX, drawZ] = 0;
                    }
                }
            }
        }
    }

    private List<TileCoordinate> CreateLine(TileCoordinate from, TileCoordinate to)
    {
        List<TileCoordinate> line = new List<TileCoordinate>();

        int x = from.GetTileX();
        int z = from.GetTileZ();

        int dx = to.GetTileX() - from.GetTileX();
        int dz = to.GetTileZ() - from.GetTileZ();

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dz);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dz);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dz);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dz);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new TileCoordinate(x, z));

            if (inverted)
            {
                z += step;
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
                    z += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    private List<List<TileCoordinate>> FindRegions(int tileType)
    {
        List<List<TileCoordinate>> regions = new List<List<TileCoordinate>>();
        int[,] mapFlags = new int[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (mapFlags[x, z] == 0 && cave[x, z] == tileType)
                {
                    List<TileCoordinate> newRegion = GetRegionTiles(x, z);
                    regions.Add(newRegion);

                    foreach (TileCoordinate tile in newRegion)
                    {
                        mapFlags[tile.GetTileX(), tile.GetTileZ()] = 1;
                    }
                }
            }
        }

        return regions;
    }

    private List<TileCoordinate> GetRegionTiles(int startX, int startZ)
    {
        List<TileCoordinate> tiles = new List<TileCoordinate>();
        int[,] mapFlags = new int[width, length];
        int tileType = cave[startX, startZ];

        Queue<TileCoordinate> queue = new Queue<TileCoordinate>();
        queue.Enqueue(new TileCoordinate(startX, startZ));
        mapFlags[startX, startZ] = 1;

        while (queue.Count > 0)
        {
            TileCoordinate tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.GetTileX() - 1; x <= tile.GetTileX() + 1; x++)
            {
                for (int z = tile.GetTileZ() - 1; z <= tile.GetTileZ() + 1; z++)
                {
                    if (IsInRange(x, z) && (z == tile.GetTileZ() || x == tile.GetTileX()))
                    {
                        if (mapFlags[x, z] == 0 && cave[x, z] == tileType)
                        {
                            mapFlags[x, z] = 1;
                            queue.Enqueue(new TileCoordinate(x, z));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    private bool IsInRange(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < length;
    }

    private void RandomFillCave()
    {
        int random = UnityEngine.Random.Range(0, 10000000);

        seed = random.ToString();

        System.Random randomSeed = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for(int z = 0; z < length; z++)
            {
                if (x == 0 || x == width - 1 || z == 0 || z == length - 1)
                {
                    if(x == 0 && z == length / 2)
                    {
                        cave[x, z] = 0;
                    }

                    else if (x == width - 1 && z == length / 2)
                    {
                        cave[x, z] = 0;
                    }

                    else if (x == width / 2 && z == 0)
                    {
                        cave[x, z] = 0;
                    }

                    else if (x == 0 && z == length - 1)
                    {
                        cave[x, z] = 0;
                    }

                    else
                    {
                        cave[x, z] = 1;
                    }  
                }
                else
                {
                    cave[x, z] = (randomSeed.Next(0, 100) < fillPercent) ? 1 : 0;
                }

                if(spawnPlayer)
                {
                    if (gameObject.name == "Chunk[0, 0]")
                    {
                        if (cave[x, z] == 0 && !playerSpawned)
                        {
                            Instantiate(Player, new Vector3(x + 5, -5, z + 5), Quaternion.Euler(0, 145, 0));
                            playerSpawned = true;
                        }
                    }
                }
            }
        }

        if(spawnPlayer)
        {
            if (gameObject.name == "Chunk[0, 0]")
            {
                for (int x = width - 1; x > 0; x--)
                {
                    for (int z = length - 1; z > 0; z--)
                    {
                        if (cave[x, z] == 0 && !enemySpawned)
                        {
                            Instantiate(Enemy, new Vector3(x / 2 - 5, -5, z / 2 - 5), Quaternion.Euler(0, -145, 0));
                            enemySpawned = true;
                        }
                    }
                }
            }
        }    
    }

    private void SmoothCave()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                int neighbourWallTiles = GetWallCount(x, z);

                if (neighbourWallTiles > 4)
                {
                    cave[x, z] = 1;
                }
                    
                else if (neighbourWallTiles < 4)
                {
                    cave[x, z] = 0;
                }
            }
        }
    }

    private int GetWallCount(int gridX, int gridZ)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourZ = gridZ - 1; neighbourZ <= gridZ + 1; neighbourZ++)
            {
                if (IsInRange(neighbourX, neighbourZ))
                {
                    if (neighbourX != gridX || neighbourZ != gridZ)
                    {
                        wallCount += cave[neighbourX, neighbourZ];
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

    public int SetSize(int _size)
    {
        return size = _size;
    }

    public bool SetSpawnPlayer(bool _value)
    {
        return spawnPlayer = _value;
    }
}
