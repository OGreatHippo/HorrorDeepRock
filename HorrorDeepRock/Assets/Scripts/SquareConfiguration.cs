using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareConfiguration
{
	public EdgeNode topLeft, topRight, bottomRight, bottomLeft;
	public CentreNode centreTop, centreRight, centreBottom, centreLeft;
	public int configuration;

	public SquareConfiguration(EdgeNode _topLeft, EdgeNode _topRight, EdgeNode _bottomRight, EdgeNode _bottomLeft)
	{
		topLeft = _topLeft;
		topRight = _topRight;
		bottomRight = _bottomRight;
		bottomLeft = _bottomLeft;

		centreTop = topLeft.right;
		centreRight = bottomRight.above;
		centreBottom = bottomLeft.right;
		centreLeft = bottomLeft.above;

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
	}
}
