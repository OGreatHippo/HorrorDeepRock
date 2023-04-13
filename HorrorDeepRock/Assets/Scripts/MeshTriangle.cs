using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle
{
	public int vertextIndexA;
	public int vertextIndexB;
	public int vertextIndexC;

	int[] vertices;

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
}
