using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tree : MonoBehaviour
{
    bool built = false;

    public float killDistance;
    public float attractionDistance;
    public float segmentLength;
    public float unitHalfSize;
    public int attractorsAmount;

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    new Transform transform;

    float buildTime;

    public void Init(Vector3 rootPos, Unit[] crownUnits)
    {
        // Position tree at node
        gameObject.transform.position = rootPos;

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
        transform = gameObject.transform;
        builderBasis = new GameObject("Builder");
        builderBasis.transform.parent = transform;
        //builderBasis.transform.localPosition = Vector3.zero;

        buildTime = Time.time;

        AddTube(Vector3.zero, Vector3.one * 2);
        AddTube(Vector3.one * 2, Vector3.one * 5);
    }

    private void Update()
    {
        if (!built)
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

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node.influencingAttractors.Count > 0)
                {
                    Node nextNode = node.GetNextNode(segmentLength);
                    nodes.Add(nextNode);
                    AddTube(node.position, nextNode.position);
                }
                //if(node.parent != null)
                //    Debug.DrawLine(node.position, node.parent.position);
                node.influencingAttractors.Clear();
            }

            for (int i = attractors.Count - 1; i >= 0; i--)
            {
                if (attractors[i].reached)
                {
                    attractors.RemoveAt(i);
                }
            }

            if (attractors.Count == 0)
            {
                built = true;
                print("Build time is: " + (Time.time - buildTime) + "seconds.");
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

    public void BuildTree()
    {
        foreach(Node n in nodes)
        {
            AddTube(n.position, n.parent.position);
        }
    }

    int pointAmount = 5;
    float tubeRadius = .05f;

    public void AddTube(Vector3 start, Vector3 end)
    {
        Mesh mesh = meshFilter.mesh;
        Mesh newMesh = new Mesh();
        int verticesAmount = mesh.vertices.Length;
        int trianglesAmount = mesh.triangles.Length;

        // Create arrays to accomodate old geometry + new tube
        Vector3[] vertices = new Vector3[verticesAmount + pointAmount * 4];
        Vector2[] uv = new Vector2[verticesAmount + pointAmount * 4];
        int[] triangles = new int[trianglesAmount + pointAmount * 6];
        Vector3[] normals = new Vector3[verticesAmount + pointAmount * 4];

        // Copy old geometry to arrays
        mesh.vertices.CopyTo(vertices, 0);
        mesh.uv.CopyTo(uv, 0);
        mesh.triangles.CopyTo(triangles, 0);
        mesh.normals.CopyTo(normals, 0);

        // Creates new tube and adds it to arrays
        Vector3[] bottomRing = GetRing(start, end - start, tubeRadius);
        Vector3[] topRing = GetRing(end, end - start, tubeRadius);

        for (int leftEdge=0; leftEdge<pointAmount; leftEdge++)
        {
            int rightEdge = (leftEdge + 1) % pointAmount;
            Vector3[] verts = { bottomRing[leftEdge], topRing[leftEdge], topRing[rightEdge], bottomRing[rightEdge] };
            Vector2[] uvs = { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
            int[] tris = { verticesAmount, verticesAmount+1, verticesAmount+3, 
                verticesAmount+1, verticesAmount+2, verticesAmount+3 };
            Vector3 v1 = bottomRing[rightEdge] - bottomRing[leftEdge];
            Vector3 v2 = topRing[leftEdge] - bottomRing[leftEdge];
            Vector3 faceNormal = Vector3.Cross(v1,v2);
            Vector3[] norms = { faceNormal, faceNormal, faceNormal, faceNormal };

            verts.CopyTo(vertices, verticesAmount);
            uvs.CopyTo(uv, verticesAmount);
            tris.CopyTo(triangles, trianglesAmount);
            norms.CopyTo(normals, verticesAmount);

            verticesAmount += 4;
            trianglesAmount += 6;
        }

        newMesh.vertices = vertices;
        newMesh.uv = uv;
        newMesh.triangles = triangles;
        newMesh.normals = normals;
        meshFilter.mesh = newMesh;
    }

    GameObject builderBasis;

    public Vector3[] GetRing(Vector3 pos, Vector3 localUp, float radius)
    {
        //builderBasis.transform.position = pos;
        builderBasis.transform.rotation = Quaternion.FromToRotation(Vector3.up, localUp);

        float angleInc = 360f / pointAmount;
        Vector3[] ring = new Vector3[pointAmount];
        for(int i=0;i<pointAmount;i++)
        {
            float angle = i * angleInc % 360f;
            Vector3 vertexPos = transform.InverseTransformPoint(pos) + builderBasis.transform.TransformDirection(
                new Vector3(Mathf.Cos(angle*Mathf.Deg2Rad),0f,Mathf.Sin(angle*Mathf.Deg2Rad)))*radius;
            ring[i] = vertexPos;
        }
        return ring;
    }
}