using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid
{
	private SquareConfiguration[,] squares;

	public SquareGrid(int[,] cave, float squareSize)
	{
		int nodeCountX = cave.GetLength(0);
		int nodeCountZ = cave.GetLength(1);
		float mapWidth = nodeCountX * squareSize;
		float mapHeight = nodeCountZ * squareSize;

		VertexNode[,] controlNodes = new VertexNode[nodeCountX, nodeCountZ];

		for (int x = 0; x < nodeCountX; x++)
		{
			for (int z = 0; z < nodeCountZ; z++)
			{
				Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + z * squareSize + squareSize / 2);
				controlNodes[x, z] = new VertexNode(pos, cave[x, z] == 1, squareSize);
			}
		}

		squares = new SquareConfiguration[nodeCountX - 1, nodeCountZ - 1];
		for (int x = 0; x < nodeCountX - 1; x++)
		{
			for (int z = 0; z < nodeCountZ - 1; z++)
			{
				squares[x, z] = new SquareConfiguration(controlNodes[x, z + 1], controlNodes[x + 1, z + 1], controlNodes[x + 1, z], controlNodes[x, z]);
			}
		}
	}

	public int GetAmountInGrid(int index)
    {
		return squares.GetLength(index);
    }

	public SquareConfiguration GetSquareAtCoords(int x, int z)
    {
		return squares[x, z];
    }
}
