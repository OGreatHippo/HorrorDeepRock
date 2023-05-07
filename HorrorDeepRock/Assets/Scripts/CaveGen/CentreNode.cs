using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentreNode
{
	private Vector3 position;
	private int vertexIndex = -1;

	public CentreNode(Vector3 _pos)
	{
		position = _pos;
	}

	public Vector3 GetPos()
    {
		return position;
    }

	public int GetVertexIndex()
    {
		return vertexIndex;
    }

	public int SetVertexIndex(int index)
    {
		return vertexIndex = index;
    }
}
