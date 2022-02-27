using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class AdaptableTree : MonoBehaviour
{
    public bool showCrown = false;

    public PointCloudData pointCloudData;
    public float height;
    public float killDistance;
    public float attractionDistance;
    public float segmentLength;
    public int attractorsAmount;
    public bool abortCollidingBranches = true;

    public int tubeVertexAmount;
    public float baseThickness;
    public float perChildThickness;
    public float maxDiffThickness;
    float unitHalfSize;
    Vector3 unitVec;

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    Unit[] crownUnits;
    float timer;
    [HideInInspector]
    public Voxelization voxelization;


    GameObject builderBasis;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if(transform.childCount > 0)
            builderBasis = transform.GetChild(0).gameObject;
        else
        {
            builderBasis = new GameObject("Builder");
            builderBasis.transform.parent = transform;
        }
    }

    public void CheckIntegrity()
    {
        if (killDistance > attractionDistance)
            throw new Exception("Tree could not be built. KillDistance should be smaller than AttractionDistance.");
        if (killDistance < segmentLength)
            throw new Exception("Tree could not be built. KillDistance should be bigger than AttractionDistance.");
    }

    public void TreeRegen()
    {
        

        // Clear current tree
        nodes.Clear();
        attractors.Clear();
        meshFilter.sharedMesh = new Mesh();

        // Add root node
        nodes.Add(new Node(null, transform.position));

        GenerateAttractors();

        BuildStructure();
        BuildGeometrySync();
        //EditorCoroutineUtility.StartCoroutine(BuildGeometryAsync(), this);
    }

    public void Init(Voxelization voxelization, Vector3 position, PointCloudData pointCloudData, Material material,
        float height, float killDistance, float attractionDistance, 
        float segmentLength, int attractorsAmount, bool abortCollidingBranches, 
        int tubeVertexAmount, float tubeRadius, float perChildThickness, float maxDiffThickness, float unitHalfSize)
    {
        // INIT
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        builderBasis = new GameObject("Builder");
        builderBasis.transform.parent = transform;

        // Attributions
        this.voxelization = voxelization;
        transform.position = position;
        this.pointCloudData = pointCloudData;
        meshRenderer.material = material;

        this.height = height;
        this.killDistance = killDistance;
        this.attractionDistance = attractionDistance;
        this.segmentLength = segmentLength;
        this.attractorsAmount = attractorsAmount;
        this.abortCollidingBranches = abortCollidingBranches;

        this.baseThickness = tubeRadius;
        this.perChildThickness = perChildThickness;
        this.maxDiffThickness = maxDiffThickness;
        this.tubeVertexAmount = tubeVertexAmount;
        this.unitHalfSize = unitHalfSize;
        unitVec = Vector3.one * unitHalfSize * 2f;
    }

    public void GenerateAttractors()
    {
        // Populate tree crown with attractors
        crownUnits = voxelization.GetFreeUnitsFloodFill(transform.position + Vector3.up * height, pointCloudData);

        // Add trunk attractors
        for (float h = killDistance / 8f; h < height + unitHalfSize; h += killDistance / 8f)
        {
            Attractor attractor = new Attractor(transform.position + Vector3.up * h);
            attractors.Add(attractor);
        }

        // Add crown attractors
        //for(int i = 0; i < attractorsAmount; i++)
        //{
        //    int randomUnitIndex = Random.Range(0, crownUnits.Length);
        //    Vector3 attractorPos = crownUnits[randomUnitIndex].position + new Vector3(Random.Range(-unitHalfSize, unitHalfSize),
        //            Random.Range(-unitHalfSize, unitHalfSize), Random.Range(-unitHalfSize, unitHalfSize));
        //    Attractor attractor = new Attractor(attractorPos);
        //}

        int amountPerUnit = Mathf.CeilToInt(((float)attractorsAmount) / crownUnits.Length);
        foreach (Unit unit in crownUnits)
        {
            for (int i = 0; i < amountPerUnit; i++)
            {
                Vector3 attractorPos = unit.position + new Vector3(Random.Range(-unitHalfSize, unitHalfSize),
                    Random.Range(-unitHalfSize, unitHalfSize), Random.Range(-unitHalfSize, unitHalfSize));
                Attractor attractor = new Attractor(attractorPos);
                attractors.Add(attractor);
            }
        }

        if (debugAttractors)
            DebugAttractors(attractors);
    }

    public void BuildStructure()
    {
        timer = Time.realtimeSinceStartup;
        while (attractors.Count > 0)
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

            int nodesCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node.influencingAttractors.Count > 0)
                {
                    Vector3 nextPosition = node.position + node.GetAverageDirection() * segmentLength;

                    // NOT IDEAL, BUT WORKS!!! :)
                    if (abortCollidingBranches && voxelization.GetUnit(nextPosition).occupied && i>6)
                        continue;

                    Node nextNode = new Node(node, nextPosition);
                    nodes.Add(nextNode);
                    node.isTip = false;
                }
                node.influencingAttractors.Clear();
            }
            if (nodes.Count == nodesCount)
            {
                Debug.LogWarning("Not every attraction points was terminated, maybe parameters should be ajusted.");
                break;
            }

            for (int i = attractors.Count - 1; i >= 0; i--)
            {
                if (attractors[i].reached)
                {
                    attractors.RemoveAt(i);
                }
            }
        }

        // THICKNESS
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            if (node.isTip)
            {
                // When there are multiple child nodes, use the thickest
                while (node.parent != null)
                {
                    if (node.parent.thickness < node.thickness + maxDiffThickness)
                    {
                        node.parent.thickness += perChildThickness;
                    }
                    node = node.parent;
                }
            }
        }

        Debug.Log("Structure creation time is: " + (Time.realtimeSinceStartup - timer) + "seconds.");
    }

    public IEnumerator BuildGeometryAsync()
    {
        timer = Time.realtimeSinceStartup;
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            AddTube(node, node.parent);
            if (node.isTip)
                AddTip(node, node.parent);
            yield return null;
        }
        Debug.Log("Geometry creation time is: " + (Time.realtimeSinceStartup - timer) + "seconds.");
    }

    public void BuildGeometrySync()
    {
        timer = Time.realtimeSinceStartup;

        Mesh newMesh = new Mesh();
        int tubeAmount = nodes.Count - 1;
        int tipAmount = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            if (node.isTip)
                tipAmount++;
        }

        Vector3[] vertices = new Vector3[tubeAmount * tubeVertexAmount * 4 + tipAmount * tubeVertexAmount * 3];
        Vector2[] uv = new Vector2[tubeAmount * tubeVertexAmount * 4 + tipAmount * tubeVertexAmount * 3];
        int[] triangles = new int[tubeAmount * tubeVertexAmount * 6 + tipAmount * tubeVertexAmount * 3];
        Vector3[] normals = new Vector3[tubeAmount * tubeVertexAmount * 4 + tipAmount * tubeVertexAmount * 3];

        int verticesAmount = 0;
        int trianglesAmount = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];

            if (node.parent == null)
                continue;

            // Creates new tube and adds it to arrays
            Vector3[] bottomRing = GetRing(node.parent.position, node.parent.directionFromParent, baseThickness * node.parent.thickness);
            Vector3[] topRing = GetRing(node.position, node.directionFromParent, baseThickness * node.thickness);

            for (int leftEdge = 0; leftEdge < tubeVertexAmount; leftEdge++)
            {
                int rightEdge = (leftEdge + 1) % tubeVertexAmount;
                Vector3[] verts = { bottomRing[leftEdge], topRing[leftEdge], topRing[rightEdge], bottomRing[rightEdge] };
                Vector2[] uvs = { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
                int[] tris = { verticesAmount, verticesAmount+1, verticesAmount+3,
                    verticesAmount+1, verticesAmount+2, verticesAmount+3 };
                Vector3 v1 = bottomRing[rightEdge] - bottomRing[leftEdge];
                Vector3 v2 = topRing[leftEdge] - bottomRing[leftEdge];
                Vector3 faceNormal = Vector3.Cross(v1, v2);
                Vector3[] norms = { faceNormal, faceNormal, faceNormal, faceNormal };

                verts.CopyTo(vertices, verticesAmount);
                uvs.CopyTo(uv, verticesAmount);
                tris.CopyTo(triangles, trianglesAmount);
                norms.CopyTo(normals, verticesAmount);

                verticesAmount += 4;
                trianglesAmount += 6;
            }

            if(node.isTip)
            {
                // Creates new tip and adds it to arrays
                bottomRing = topRing;
                Vector3 tipVertex = GetTip(node.position, node.directionFromParent);

                for (int leftEdge = 0; leftEdge < tubeVertexAmount; leftEdge++)
                {
                    int rightEdge = (leftEdge + 1) % tubeVertexAmount;
                    Vector3[] verts = { bottomRing[leftEdge], tipVertex, bottomRing[rightEdge] };
                    Vector2[] uvs = { Vector2.zero, Vector2.one, Vector2.right };
                    int[] tris = { verticesAmount, verticesAmount + 1, verticesAmount + 2 };
                    Vector3 v1 = bottomRing[rightEdge] - bottomRing[leftEdge];
                    Vector3 v2 = tipVertex - bottomRing[leftEdge];
                    Vector3 faceNormal = Vector3.Cross(v1, v2);
                    Vector3[] norms = { faceNormal, faceNormal, faceNormal };

                    verts.CopyTo(vertices, verticesAmount);
                    uvs.CopyTo(uv, verticesAmount);
                    tris.CopyTo(triangles, trianglesAmount);
                    norms.CopyTo(normals, verticesAmount);

                    verticesAmount += 3;
                    trianglesAmount += 3;
                }
            }
        }

        newMesh.vertices = vertices;
        newMesh.uv = uv;
        newMesh.triangles = triangles;
        newMesh.normals = normals;
        meshFilter.mesh = newMesh;

        Debug.Log("Geometry creation time is: " + (Time.realtimeSinceStartup - timer) + "seconds.");
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

    public void AddTip(Node node, Node parent)
    {

        Mesh mesh = meshFilter.sharedMesh;
        Mesh newMesh = new Mesh();
        int verticesAmount = mesh.vertices.Length;
        int trianglesAmount = mesh.triangles.Length;

        // Create arrays to accomodate old geometry + new tip
        Vector3[] vertices = new Vector3[verticesAmount + tubeVertexAmount * 3];
        Vector2[] uv = new Vector2[verticesAmount + tubeVertexAmount * 3];
        int[] triangles = new int[trianglesAmount + tubeVertexAmount * 3];
        Vector3[] normals = new Vector3[verticesAmount + tubeVertexAmount * 3];

        // Copy old geometry to arrays
        mesh.vertices.CopyTo(vertices, 0);
        mesh.uv.CopyTo(uv, 0);
        mesh.triangles.CopyTo(triangles, 0);
        mesh.normals.CopyTo(normals, 0);

        // Creates new tip and adds it to arrays
        Vector3[] bottomRing = GetRing(node.position, node.directionFromParent, baseThickness * node.thickness);
        Vector3 tipVertex = GetTip(node.position, node.directionFromParent);

        for (int leftEdge = 0; leftEdge < tubeVertexAmount; leftEdge++)
        {
            int rightEdge = (leftEdge + 1) % tubeVertexAmount;
            Vector3[] verts = { bottomRing[leftEdge], tipVertex, bottomRing[rightEdge] };
            Vector2[] uvs = { Vector2.zero, Vector2.one, Vector2.right };
            int[] tris = { verticesAmount, verticesAmount+1, verticesAmount+2 };
            Vector3 v1 = bottomRing[rightEdge] - bottomRing[leftEdge];
            Vector3 v2 = tipVertex - bottomRing[leftEdge];
            Vector3 faceNormal = Vector3.Cross(v1, v2);
            Vector3[] norms = { faceNormal, faceNormal, faceNormal };

            verts.CopyTo(vertices, verticesAmount);
            uvs.CopyTo(uv, verticesAmount);
            tris.CopyTo(triangles, trianglesAmount);
            norms.CopyTo(normals, verticesAmount);

            verticesAmount += 3;
            trianglesAmount += 3;
        }

        newMesh.vertices = vertices;
        newMesh.uv = uv;
        newMesh.triangles = triangles;
        newMesh.normals = normals;
        meshFilter.mesh = newMesh;
    }

    public void AddTube(Node node, Node parent)
    {
        if (parent == null)
            return;

        Mesh mesh = meshFilter.sharedMesh;
        Mesh newMesh = new Mesh();
        int verticesAmount = mesh.vertices.Length;
        int trianglesAmount = mesh.triangles.Length;

        // Create arrays to accomodate old geometry + new tube
        Vector3[] vertices = new Vector3[verticesAmount + tubeVertexAmount * 4];
        Vector2[] uv = new Vector2[verticesAmount + tubeVertexAmount * 4];
        int[] triangles = new int[trianglesAmount + tubeVertexAmount * 6];
        Vector3[] normals = new Vector3[verticesAmount + tubeVertexAmount * 4];

        // Copy old geometry to arrays
        mesh.vertices.CopyTo(vertices, 0);
        mesh.uv.CopyTo(uv, 0);
        mesh.triangles.CopyTo(triangles, 0);
        mesh.normals.CopyTo(normals, 0);

        // Creates new tube and adds it to arrays
        Vector3[] bottomRing = GetRing(parent.position, parent.directionFromParent, baseThickness*parent.thickness);
        Vector3[] topRing = GetRing(node.position, node.directionFromParent, baseThickness*node.thickness);

        for (int leftEdge=0; leftEdge<tubeVertexAmount; leftEdge++)
        {
            int rightEdge = (leftEdge + 1) % tubeVertexAmount;
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

    public Vector3[] GetRing(Vector3 pos, Vector3 localUp, float radius)
    {
        //builderBasis.transform.position = pos;
        builderBasis.transform.rotation = Quaternion.FromToRotation(Vector3.up, localUp);

        float angleInc = 360f / tubeVertexAmount;
        Vector3[] ring = new Vector3[tubeVertexAmount];
        for(int i=0;i<tubeVertexAmount;i++)
        {
            float angle = i * angleInc % 360f;
            Vector3 vertexPos = transform.InverseTransformPoint(pos) + builderBasis.transform.TransformDirection(
                new Vector3(Mathf.Cos(angle*Mathf.Deg2Rad),0f,Mathf.Sin(angle*Mathf.Deg2Rad)))*radius;
            ring[i] = vertexPos;
        }
        return ring;
    }

    public Vector3 GetTip(Vector3 pos, Vector3 localUp)
    {
        builderBasis.transform.rotation = Quaternion.FromToRotation(Vector3.up, localUp);
        Vector3 vertexPos = transform.InverseTransformPoint(pos) + localUp / 2f;
        return vertexPos;
    }

    public bool debugAttractors;
    GameObject debugObj;
    private void DebugAttractors(List<Attractor> attractors)
    {
        debugObj = Resources.Load<GameObject>("DebugObj");
        GameObject g = new GameObject("Debug");
        foreach (Attractor a in attractors)
        {
            Instantiate(debugObj, a.position, Quaternion.identity, g.transform);
        }
    }

    private void OnDrawGizmos()
    {
        if (showCrown && crownUnits != null)
        {
            for (int i = 0; i < crownUnits.Length; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(crownUnits[i].position, unitVec);
            }
        }
    }
}