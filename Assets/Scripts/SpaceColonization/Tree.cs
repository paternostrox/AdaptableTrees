using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tree : MonoBehaviour
{
    public float killDistance;
    public float attractionDistance;
    public float segmentLength;
    public float unitHalfSize;
    public int attractorsAmount;

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

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

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
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

    int pointAmount = 3;
    float tubeRadius = 0.05f;

    public void AddTube(Vector3 start, Vector3 end)
    {
        Mesh mesh = meshFilter.mesh;
        Mesh newMesh = new Mesh();

        Vector3[] verts = new Vector3[pointAmount * 4];
        int[] tris = new int[pointAmount * 6];
        Vector3[] normals = new Vector3[pointAmount * 4];
        Vector2[] uvs = new Vector2[pointAmount * 4];

        Vector3[] bottomRing = GetRing(start, end - start, tubeRadius);
        Vector3[] topRing = GetRing(end, end - start, tubeRadius);

        // makes the squares
        for (int leftEdge=0; leftEdge<pointAmount; leftEdge++)
        {
            int rightEdge = (leftEdge + 1) % pointAmount;
            int refVert = 4 * leftEdge;
            verts[refVert] = bottomRing[leftEdge];
            verts[refVert + 1] = topRing[leftEdge];
            verts[refVert + 2] = topRing[rightEdge];
            verts[refVert + 3] = bottomRing[rightEdge];

            tris[].
        }
    }

    GameObject builderBasis;

    public Vector3[] GetRing(Vector3 pos, Vector3 localUp, float radius)
    {
        //builderBasis.transform.position = pos1;
        builderBasis.transform.rotation = Quaternion.FromToRotation(Vector3.up, localUp);

        float angleInc = 360f / pointAmount;
        Vector3[] ring = new Vector3[pointAmount];
        for(int i=0;i<pointAmount;i++)
        {
            float angle = i * angleInc % 360f;
            Vector3 vertexPos = pos + builderBasis.transform.TransformPoint(new Vector3(Mathf.Cos(angle),0f,Mathf.Sin(angle)));
            ring[i] = vertexPos;
        }
        return ring;
    }
}