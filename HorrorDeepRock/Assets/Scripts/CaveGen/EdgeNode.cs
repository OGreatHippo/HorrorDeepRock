using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeNode : CentreNode
{
	public bool active;
	public CentreNode above, right;

	public EdgeNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
	{
		active = _active;
		above = new CentreNode(position + Vector3.forward * squareSize / 2f);
		right = new CentreNode(position + Vector3.right * squareSize / 2f);
	}

}
