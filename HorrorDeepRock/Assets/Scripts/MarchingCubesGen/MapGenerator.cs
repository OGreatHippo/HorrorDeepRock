using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public int length;

    public int neightbouringWalls = 16;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)] public int randomFillPercent;

    int[,,] map;

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new int[width, height, length];

        RandomFillMap();

        for(int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        CubeMeshGenerator meshGen = GetComponent<CubeMeshGenerator>();

        meshGen.GenerateMesh(map, 1);
    }

    void RandomFillMap()
    {
        if(useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    if(x == 0 || x == width - 1 || y == 0 || y == height - 1 || z == 0 || z == length - 1)
                    {
                        map[x, y, z] = 1;
                    }

                    else
                    {
                        map[x, y, z] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                    } 
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y, z);

                    if(neighbourWallTiles > neightbouringWalls)
                    {
                        map[x, y, z] = 1;
                    }

                    else if(neighbourWallTiles < neightbouringWalls)
                    {
                        map[x, y, z] = 0;
                    }
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY, int gridZ)
    {
        int wallCount = 0;

        for(int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                for (int neighbourZ = gridZ - 1; neighbourZ <= gridZ + 1; neighbourZ++)
                {
                    if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height && neighbourZ >= 0 && neighbourZ < length)
                    {
                        if (neighbourX != gridX || neighbourY != gridY || neighbourZ != gridZ)
                        {
                            wallCount += map[neighbourX, neighbourY, neighbourZ];
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
