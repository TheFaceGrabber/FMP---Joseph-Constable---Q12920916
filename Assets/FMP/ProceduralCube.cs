using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCube : MonoBehaviour
{

    static Vector3[] verts = new Vector3[8]
    {
        new Vector3(-0.5f,  0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3( 0.5f,  0.5f, 0.5f),
        new Vector3( 0.5f, -0.5f, 0.5f),

        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f, -0.5f, -0.5f),
    };

    static int[] indices = new int[36]
    {
        //Front face
        1,3,0,
        2,0,3,

        //Left face
        4,5,1,
        0,4,1,
            
        //Back face
        4,7,5,
        7,4,6,
            
        //Right face
        3,7,6,
        3,6,2,

        //Top face
        4,0,2,
        6,4,2,

        //Bottom face
        3,1,5,
        3,5,7
    };

    /// <summary>
    /// Spawns a procedural cube
    /// </summary>
    /// <param name="location">Where in the world to spawn the cube</param>
    /// <param name="size">The size of the cube (default is 1x1x1)</param>
    public static ProceduralCube Create(Vector3 location = default, Vector3 size = default)
    {
        GameObject cube = new GameObject("Procedural Cube");
        ProceduralCube proc = cube.AddComponent<ProceduralCube>();
        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        if (size == default)
            size = Vector3.one;

        List<Vector3> vertices = new List<Vector3>();
        foreach (var vert in verts)
        {
            vertices.Add(new Vector3(vert.x * size.x, vert.y * size.y, vert.z * size.z));
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        filter.mesh = mesh;
        return proc;
    }
}
