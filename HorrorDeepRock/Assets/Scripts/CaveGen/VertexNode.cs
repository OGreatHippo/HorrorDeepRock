using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexNode : CentreNode
{
	private bool active;
	private CentreNode above, right;

	public VertexNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
	{
		active = _active;
		above = new CentreNode(GetPos() + Vector3.forward * squareSize / 2f);
		right = new CentreNode(GetPos() + Vector3.right * squareSize / 2f);
	}

	public bool GetActive()
    {
		return active;
    }

	public CentreNode GetAbove()
    {
		return above;
    }

	public CentreNode GetRight()
	{
		return right;
	}

}
