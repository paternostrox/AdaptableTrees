using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Node parent;
    public Vector3 position;
    public List<Attractor> influencingAttractors = new List<Attractor>();
    float jitterAmount = .1f;
    bool isTip; // For later use with thickening (maybe)

    public Node(Node parent, Vector3 position)
    {
        this.parent = parent;
        this.position = position;
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

        return averageDirection;
    }

    public Node GetNextNode(float segmentLength)
    {
        Vector3 nextPosition = position + GetAverageDirection() * segmentLength;

        return new Node(this, nextPosition);
    }
}
