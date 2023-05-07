using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle
{
	private int vertextIndexA;
	private int vertextIndexB;
	private int vertextIndexC;

	private int[] vertices;

	public MeshTriangle(int a, int b, int c)
	{
		vertextIndexA = a;
		vertextIndexB = b;
		vertextIndexC = c;

		vertices = new int[3];
		vertices[0] = a;
		vertices[1] = b;
		vertices[2] = c;
	}

	public int this[int i]
	{
		get
		{
			return vertices[i];
		}
	}

	public bool Contains(int vertexIndex)
	{
		return vertexIndex == vertextIndexA || vertexIndex == vertextIndexB || vertexIndex == vertextIndexC;
	}

	public int GetVertexA()
    {
		return vertextIndexA;
    }

	public int GetVertexB()
	{
		return vertextIndexB;
	}

	public int GetVertexC()
	{
		return vertextIndexC;
	}

	public int[] GetVertices()
    {
		return vertices;
    }
}
