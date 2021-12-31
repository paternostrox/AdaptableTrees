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

    float jitterAmount;

    public Node(Node parent, Vector3 position)
    {
        this.parent = parent;
        this.position = position;
        if (parent != null)
            directionFromParent = position - parent.position;
        jitterAmount = SceneManager.Instance.nodeJitterAmount;
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
        averageDirection += new Vector3(Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount)).normalized;

        averageDirection.Normalize();

        return averageDirection;
    }

    public Node GetNextNode(float segmentLength)
    {
        Vector3 nextPosition = position + GetAverageDirection() * segmentLength;
        //if(nextPosition.y > SceneManager.Instance.treeHeight + SceneManager.Instance.treeSize)
        //{
        //    Debug.Log("Shootout! Pos is: " + nextPosition);
        //}

        return new Node(this, nextPosition);
    }
}
