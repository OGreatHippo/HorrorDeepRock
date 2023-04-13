using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid
{
	public SquareConfiguration[,] squares;

	public SquareGrid(int[,] cave, float squareSize)
	{
		int nodeCountX = cave.GetLength(0);
		int nodeCountY = cave.GetLength(1);
		float mapWidth = nodeCountX * squareSize;
		float mapHeight = nodeCountY * squareSize;

		EdgeNode[,] controlNodes = new EdgeNode[nodeCountX, nodeCountY];

		for (int x = 0; x < nodeCountX; x++)
		{
			for (int y = 0; y < nodeCountY; y++)
			{
				Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
				controlNodes[x, y] = new EdgeNode(pos, cave[x, y] == 1, squareSize);
			}
		}

		squares = new SquareConfiguration[nodeCountX - 1, nodeCountY - 1];
		for (int x = 0; x < nodeCountX - 1; x++)
		{
			for (int y = 0; y < nodeCountY - 1; y++)
			{
				squares[x, y] = new SquareConfiguration(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
			}
		}

	}
}
