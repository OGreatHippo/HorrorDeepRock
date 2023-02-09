using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    private MeshGenerator meshGen;

    public int width;
    public int height;

    public string seed;
    public bool usingRandomSeed;

    [Range(0, 100)] public int fillPercent;

    int[,] cave;

    // Start is called before the first frame update
    void Start()
    {
        meshGen = GetComponent<MeshGenerator>();

        GenerateCave();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GenerateCave();
        }
    }

    private void GenerateCave()
    {
        cave = new int[width, height];
        RandomFillCave();

        for(int i = 0; i < 5; i++)
        {
            SmoothCave();
        }

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

        meshGen.GenerateMesh(wallBorder, 1);
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
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    cave[x, y] = 1;
                }
                else
                {
                    cave[x, y] = (randomSeed.Next(0, 100) < fillPercent) ? 1 : 0;
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
                int neighbourWalls = GetWallCount(x, y);

                if (neighbourWalls > 4)
                {
                    cave[x, y] = 1;
                }

                else if(neighbourWalls < 4)
                {
                    cave[x, y] = 0;
                }
            }
        }
    }

    private int GetWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        for(int nX = gridX - 1; nX <= gridX + 1; nX++)
        {
            for (int nY = gridY - 1; nY <= gridY + 1; nY++)
            {
                if(nX >= 0 && nX < width && nY >= 0 && nY < height)
                {
                    if (nX != gridX || nY != gridY)
                    {
                        wallCount += cave[nX, nY];
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
