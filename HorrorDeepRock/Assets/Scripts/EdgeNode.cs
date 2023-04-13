using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeNode : CentreNode
{
    public bool active;
    public CentreNode above, right, up;
    private float squareSize;

    public EdgeNode(Vector3 _pos, bool _active, float _squareSize) : base(_pos)
    {
        active = _active;
        above = new CentreNode(position + Vector3.forward * _squareSize / 2f);
        right = new CentreNode(position + Vector3.right * _squareSize / 2f);
        up = new CentreNode(position + Vector3.up * _squareSize / 2f);
        squareSize = _squareSize;
    }
}
