using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareConfiguration
{
    public EdgeNode topLeft, topRight, topUp, bottomRight, bottomLeft, bottomUp, rightUp, leftUp;
    public CentreNode centreTop, centreRight, centreBottom, centreLeft, centreUp, centreDown;
    public int configuration;

    public SquareConfiguration(
    EdgeNode _topLeft, EdgeNode _topRight, EdgeNode _topUp,
    EdgeNode _rightUp, EdgeNode _bottomRight, EdgeNode _bottomUp,
    EdgeNode _bottomLeft, EdgeNode _leftUp)
    {
        topLeft = _topLeft;
        topRight = _topRight;
        topUp = _topUp;
        rightUp = _rightUp;
        bottomRight = _bottomRight;
        bottomUp = _bottomUp;
        bottomLeft = _bottomLeft;
        leftUp = _leftUp;

        configuration = 0;

        if (topLeft.active)
        {
            configuration += 8;
        }

        if (topRight.active)
        {
            configuration += 4;
        }

        if (bottomRight.active)
        {
            configuration += 2;
        }

        if (bottomLeft.active)
        {
            configuration += 1;
        }

        if (topUp.active)
        {
            configuration += 128;
        }

        if (rightUp.active)
        {
            configuration += 64;
        }

        if (bottomUp.active)
        {
            configuration += 32;
        }

        if (leftUp.active)
        {
            configuration += 16;
        }
    }
}
