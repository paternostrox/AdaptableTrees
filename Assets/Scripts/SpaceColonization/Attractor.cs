using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor
{
    public Vector3 position;
    public Node influencedNode;
    public bool reached = false;

    public Attractor(Vector3 position)
    {
        this.position = position;
    }
}
