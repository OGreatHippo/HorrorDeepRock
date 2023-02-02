using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool usingRandomSeed;

    [Range(0, 100)] public int fillPercent;

    int[,] cave;

    // Start is called before the first frame update
    void Start()
    {
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

    private void OnDrawGizmos()
    {
        if(cave != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (cave[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
