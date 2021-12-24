using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public float killDistance;
    public float attractionDistance;
    public float segmentLength;
    public float unitHalfSize;
    public int attractorsAmount;

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();

    public void Init(Vector3 rootPos, Unit[] crownUnits)
    {
        // Position tree at node
        transform.position = rootPos;

        // Add root node
        nodes.Add(new Node(null, rootPos));

        int amountPerUnit = Mathf.CeilToInt(((float) attractorsAmount) / crownUnits.Length);

        foreach(Unit unit in crownUnits)
        {
            for(int i=0; i< amountPerUnit; i++)
            {
                Vector3 attractorPos = unit.position + new Vector3(Random.Range(-unitHalfSize, unitHalfSize),
                    Random.Range(-unitHalfSize, unitHalfSize), Random.Range(-unitHalfSize, unitHalfSize));
                Attractor attractor = new Attractor(attractorPos);
                attractors.Add(attractor);
                //Instantiate(new GameObject("Attractor"), attractorPos, Quaternion.identity, transform);
            }
        }
    }

    public void SetParams(float killDistance, float attractionDistance, float segmentLength, int attractorsAmount, float unitHalfSize)
    {
        this.killDistance = killDistance;
        this.attractionDistance = attractionDistance;
        this.segmentLength = segmentLength;

        this.attractorsAmount = attractorsAmount;
        this.unitHalfSize = unitHalfSize;
    }

    private void Update()
    {
        foreach (Attractor a in attractors)
        {
            Node closestNode = GetClosestNode(a);
            if (closestNode != null)
            {
                a.influencedNode = closestNode;
                closestNode.influencingAttractors.Add(a);
            }
        }

        for(int i=0; i<nodes.Count; i++)
        {
            Node node = nodes[i];
            if (node.influencingAttractors.Count > 0)
            {
                Node nextNode = node.GetNextNode(segmentLength);
                nodes.Add(nextNode);
            }
            if(node.parent != null)
                Debug.DrawLine(node.position, node.parent.position);
            node.influencingAttractors.Clear();
        }

        for(int i = attractors.Count - 1; i >= 0; i--)
        {
            if(attractors[i].reached)
            {
                attractors.RemoveAt(i);
            }
        }

        if(attractors.Count == 0)
        {
            // BUILT
        }
    }

    public Node GetClosestNode(Attractor attractor)
    {
        float minDist = Mathf.Infinity;
        Node minDistNode = null;
        foreach(Node n in nodes)
        {
            float dist = Vector3.Distance(n.position, attractor.position);
            if(dist < killDistance)
            {
                attractor.reached = true;
                minDistNode = null;
                // Return here?
            }
            else if(dist < minDist && dist < attractionDistance)
            {
                minDist = dist;
                minDistNode = n;
            }
        }
        return minDistNode;
    }
}