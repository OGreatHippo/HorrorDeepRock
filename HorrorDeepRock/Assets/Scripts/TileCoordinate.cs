using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoordinate
{
    public int tileX;
    public int tileY;
    public int tileZ;
    public TileCoordinate()
    {

    }

    public TileCoordinate(int x, int y, int z)
    {
        tileX = x;
        tileY = y;
        tileZ = z;
    }
}
