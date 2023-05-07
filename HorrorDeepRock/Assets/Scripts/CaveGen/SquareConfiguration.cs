using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareConfiguration
{
	private VertexNode topLeft, topRight, bottomRight, bottomLeft;
	private CentreNode centreTop, centreRight, centreBottom, centreLeft;
	private int configuration;

	public SquareConfiguration(VertexNode _topLeft, VertexNode _topRight, VertexNode _bottomRight, VertexNode _bottomLeft)
	{
		topLeft = _topLeft;
		topRight = _topRight;
		bottomRight = _bottomRight;
		bottomLeft = _bottomLeft;

		centreTop = topLeft.GetRight();
		centreRight = bottomRight.GetAbove();
		centreBottom = bottomLeft.GetRight();
		centreLeft = bottomLeft.GetAbove();

		if (topLeft.GetActive())
		{
			configuration += 8;
		}

		if (topRight.GetActive())
		{
			configuration += 4;
		}

		if (bottomRight.GetActive())
		{
			configuration += 2;
		}

		if (bottomLeft.GetActive())
		{
			configuration += 1;
		}
	}

	public int GetConfiguration()
    {
		return configuration;
    }

	public VertexNode GetTopLeft()
    {
		return topLeft;
    }

	public VertexNode GetTopRight()
	{
		return topRight;
	}

	public VertexNode GetBottomRight()
	{
		return bottomRight;
	}

	public VertexNode GetBottomLeft()
	{
		return bottomLeft;
	}

	public CentreNode GetCentreTop()
    {
		return centreTop;
    }

	public CentreNode GetCentreRight()
	{
		return centreRight;
	}

	public CentreNode GetCentreBottom()
	{
		return centreBottom;
	}

	public CentreNode GetCentreLeft()
	{
		return centreLeft;
	}
}
