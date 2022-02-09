using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node
{
    public Node parent;
    public Vector3 position;
    public Vector3 directionFromParent = Vector3.up;
    public List<Attractor> influencingAttractors = new List<Attractor>();
    public bool isTip = true; // For thickening
    public float thickness = 1f;

    // jitter to avoid getting stuck
    static float jitterAmount = .001f;

    public Node(Node parent, Vector3 position)
    {
        this.parent = parent;
        this.position = position;
        if (parent != null)
            directionFromParent = position - parent.position;
    }

    public Vector3 GetAverageDirection()
    {
        // Calculate average direction based on attractors influence
        Vector3 averageDirection = Vector3.zero;
        foreach(Attractor a in influencingAttractors)
        {
            averageDirection += (a.position - position).normalized;
        }
        // add jitter to avoid getting stuck
        //averageDirection += new Vector3(Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount)).normalized;

        averageDirection.Normalize();

        return averageDirection;
    }

    public Node GetNextNode(float segmentLength)
    {
        Vector3 nextPosition = position + GetAverageDirection() * segmentLength;

        return new Node(this, nextPosition);
    }
}
