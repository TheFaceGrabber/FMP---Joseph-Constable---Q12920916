using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCube : MonoBehaviour
{

    static Vector3[] verts = new Vector3[24]
    {
        //Front face
        new Vector3(-0.5f,  0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3( 0.5f,  0.5f, 0.5f),
        new Vector3( 0.5f, -0.5f, 0.5f),
        
        //Back face
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f, -0.5f, -0.5f),

        //Left face
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f,  0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),

        //Right face
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        
        //Top face
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3( 0.5f,  0.5f,  0.5f),

        //Bottom face
        new Vector3(-0.5f,  -0.5f,  -0.5f),
        new Vector3( 0.5f,  -0.5f,  -0.5f),
        new Vector3(-0.5f,  -0.5f,   0.5f),
        new Vector3( 0.5f,  -0.5f,   0.5f),
    };

    static int[] indices = new int[36]
    {
        //Front face
        1,3,0,
        2,0,3,
        
        //Front face
        4,7,5,
        7,4,6,
        
        //Left face
        9,11,8,
        10,8,11,
            
        //Right face
        12,15,13,
        15,12,14,
        
        //Top face
        16,19,17,
        19,16,18,
        
        //Bottom face
        21,23,20,
        22,20,23
    };

    /// <summary>
    /// Spawns a procedural cube
    /// </summary>
    /// <param name="location">Where in the world to spawn the cube</param>
    /// <param name="size">The size of the cube (default is 1x1x1)</param>
    public static ProceduralCube Create(Vector3 location = default, Vector3 size = default)
    {
        GameObject cube = new GameObject("Procedural Cube");
        Material mat = Instantiate(Resources.Load<Material>("DefaultMaterial"));
        ProceduralCube proc = cube.AddComponent<ProceduralCube>();
        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();
        renderer.material = mat;
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

        mesh.UploadMeshData(false);

        filter.mesh = mesh;
        return proc;
    }
}
