using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid
{
	public SquareConfiguration[,,] squares;

	public SquareGrid(int[,,] cave, float squareSize)
	{
		int nodeCountX = cave.GetLength(0);
		int nodeCountY = cave.GetLength(1);
		int nodeCountZ = cave.GetLength(2);

		float mapWidth = nodeCountX * squareSize;
		float mapHeight = nodeCountY * squareSize;
		float mapLength = nodeCountZ * squareSize;

		EdgeNode[,,] controlNodes = new EdgeNode[nodeCountX, nodeCountY, nodeCountZ];

		for (int x = 0; x < nodeCountX; x++)
		{
			for (int y = 0; y < nodeCountY; y++)
			{
				for (int z = 0; z < nodeCountZ; z++)
				{
					Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, -mapHeight / 2 + y * squareSize + squareSize / 2, -mapLength / 2 + z * squareSize + squareSize / 2);
					controlNodes[x, y, z] = new EdgeNode(pos, cave[x, y, z] == 1, squareSize);
				}
			}
		}

		squares = new SquareConfiguration[nodeCountX - 1, nodeCountY - 1, nodeCountZ - 1];
		for (int x = 0; x < nodeCountX - 1; x++)
		{
			for (int y = 0; y < nodeCountY - 1; y++)
			{
				for (int z = 0; z < nodeCountZ - 1; z++)
				{
					squares[x, y, z] = new SquareConfiguration(controlNodes[x, y, z], controlNodes[x, y, z + 1], controlNodes[x, y + 1, z + 1], controlNodes[x + 1, y, z + 1], controlNodes[x + 1, y, z], controlNodes[x + 1, y + 1, z], controlNodes[x + 1, y + 1, z + 1], controlNodes[x, y + 1, z]);
				}
			}
		}
	}
}
