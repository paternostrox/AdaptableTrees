using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public float killDistance;
    public float attractionDistance;
    public float segmentLength;

    public GameObject attractorDebug;

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();

    // Init Vars
    Vector3 rootPos;
    float height;
    Vector3 size;
    int attractorsAmount;

    public void Init(Vector3 rootPos, int attractorsAmount, Unit[] crownUnits)
    {
        this.rootPos = rootPos;
        this.attractorsAmount = attractorsAmount;

        //
    }

    private void Start()
    {

    }

    private void BuildTree()
    {
        // Add root node
        nodes.Add(new Node(null, rootPos));

        Vector3 crownPos = rootPos + Vector3.up * height;

        for (int i = 0; i < 500; i++)
        {
            Vector3 attractorOffset = new Vector3(Random.Range(-size.x, size.x), Random.Range(-size.y, size.y), Random.Range(-size.z, size.z));
            Attractor attractor = new Attractor();
            attractors.Add(attractor);
        }
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