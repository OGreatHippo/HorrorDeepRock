using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoordinate
{
    private int tileX;
    private int tileZ;

    public TileCoordinate()
    {

    }

    public TileCoordinate(int x, int z)
    {
        tileX = x;
        tileZ = z;
    }

    public int GetTileX()
    {
        return tileX;
    }

    public int GetTileZ()
    {
        return tileZ;
    }
}
