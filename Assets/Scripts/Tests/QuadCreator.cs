using UnityEngine;
using System.Collections;
using System;

public class QuadCreator : MonoBehaviour
{
    public float width = 1;
    public float height = 1;

    MeshFilter meshFilter;

    public void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        //Vector3[] vertices = new Vector3[4]
        //{
        //    new Vector3(0, 0, 0),
        //    new Vector3(width, 0, 0),
        //    new Vector3(0, height, 0),
        //    new Vector3(width, height, 0)
        //};
        //mesh.vertices = vertices;

        //int[] tris = new int[6]
        //{
        //    // lower left triangle
        //    0, 2, 1,
        //    // upper right triangle
        //    2, 3, 1
        //};
        //mesh.triangles = tris;

        //Vector3[] normals = new Vector3[4]
        //{
        //    -Vector3.forward,
        //    -Vector3.forward,
        //    -Vector3.forward,
        //    -Vector3.forward
        //};
        //mesh.normals = normals;

        //Vector2[] uv = new Vector2[4]
        //{
        //    new Vector2(0, 0),
        //    new Vector2(1, 0),
        //    new Vector2(0, 1),
        //    new Vector2(1, 1)
        //};
        //mesh.uv = uv;

        meshFilter.mesh = mesh;

        StartCoroutine(MeshAddition());
    }

    public int quadAmount = 0;

    public IEnumerator MeshAddition()
    {
        for (; ; )
        {
            Mesh mesh = meshFilter.mesh;
            Mesh newMesh = new Mesh();
            int vertexAmount = quadAmount * 4;

            Vector3[] newVertices = new Vector3[4]
            {
            new Vector3(0, height*quadAmount, 0),
            new Vector3(width, height*quadAmount, 0),
            new Vector3(0, height*quadAmount + height, 0),
            new Vector3(width, height*quadAmount + height, 0)
            };
            Vector3[] vertices = new Vector3[mesh.vertices.Length + newVertices.Length];
            mesh.vertices.CopyTo(vertices, 0);
            newVertices.CopyTo(vertices, mesh.vertices.Length);
            newMesh.vertices = vertices;

            int[] newTris = new int[6]
            {
            // lower left triangle
            vertexAmount, vertexAmount+2, vertexAmount+1,
            // upper right triangle
            vertexAmount+2, vertexAmount+3, vertexAmount+1
            };
            int[] tris = new int[mesh.triangles.Length + newTris.Length];
            mesh.triangles.CopyTo(tris, 0);
            newTris.CopyTo(tris, mesh.triangles.Length);
            newMesh.triangles = tris;

            Vector3[] newNormals = new Vector3[4]
            {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
            };
            Vector3[] normals = new Vector3[mesh.normals.Length + newNormals.Length];
            mesh.normals.CopyTo(normals, 0);
            newNormals.CopyTo(normals, mesh.normals.Length);
            newMesh.normals = normals;

            Vector2[] newUV = new Vector2[4]
            {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
            };
            Vector2[] uv = new Vector2[mesh.uv.Length + newUV.Length];
            mesh.uv.CopyTo(uv, 0);
            newUV.CopyTo(uv, mesh.uv.Length);
            newMesh.uv = uv;

            meshFilter.mesh = newMesh;
            quadAmount++;
            yield return new WaitForSeconds(2f);
        }
    }
}