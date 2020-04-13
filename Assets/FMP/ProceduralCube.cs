using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class ProceduralCube : MonoBehaviour
{
    #region Statics
    //Static vertex and triangle information to be accessed by all procedural cubes on creation
    //Faces each have seperate vertices so that normals can be generated properly
    //TODO: Uvs
    static readonly Vector3[] Vertices = new Vector3[24]
    {
        //Front face
        new Vector3(0,  1f, 1f),
        new Vector3(0, 0, 1f),
        new Vector3( 1f,  1f, 1f),
        new Vector3( 1f, 0, 1f),
        
        //Back face
        new Vector3(0,  1f, 0),
        new Vector3(0, 0, 0),
        new Vector3( 1f,  1f, 0),
        new Vector3( 1f, 0, 0),

        //Left face
        new Vector3(0,  1f, 0),
        new Vector3(0, 0, 0),
        new Vector3(0,  1f, 1f),
        new Vector3(0, 0, 1f),

        //Right face
        new Vector3(1f,  1f, 0),
        new Vector3(1f, 0, 0),
        new Vector3(1f,  1f, 1f),
        new Vector3(1f, 0, 1f),
        
        //Top face
        new Vector3(0,  1f, 0),
        new Vector3( 1f,  1f, 0),
        new Vector3(0,  1f,  1f),
        new Vector3( 1f,  1f,  1f),

        //Bottom face
        new Vector3(0,  0,  0),
        new Vector3( 1f,  0,  0),
        new Vector3(0,  0,   1f),
        new Vector3( 1f,  0,   1f),
    };

    static readonly int[] Indices = new int[36]
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
    public static ProceduralCube Create(Vector3 location, Vector3 size, float gridSize)
    {
        GameObject cube = new GameObject("Procedural Cube");
        cube.transform.position = Grid.Snap(location, gridSize);
        Material mat = Instantiate(Resources.Load<Material>("DefaultMaterial"));
        ProceduralCube proc = cube.AddComponent<ProceduralCube>();
        proc.EndPosition = cube.transform.position + size;
        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();
        renderer.material = mat;

        cube.AddComponent<BoxCollider>();

        proc.UpdateMesh(gridSize);

        return proc;
    }

    #endregion

    [HideInInspector]
    public Vector3 EndPosition;

    public void UpdateMesh(float gridSize)
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        EndPosition = Grid.SnapSize(EndPosition, gridSize);

        var size = EndPosition - transform.position;

        foreach (var vertex in Vertices)
        {
            vertices.Add(new Vector3(vertex.x * size.x, vertex.y * size.y, vertex.z * size.z));
        }

        mesh.vertices = vertices.ToArray();

        for (int i = 0; i < vertices.Count; i+=4)
        {
            var v1 = vertices[i];
            var v2 = vertices[i + 1];
            var v3 = vertices[i + 2];
            var v4 = vertices[i + 3];

            if (v1.z == v2.z && v1.z == v3.z && v1.z == v4.z)
            {
                uvs.Add(new Vector2(v1.x,v1.y));
                uvs.Add(new Vector2(v2.x, v2.y));
                uvs.Add(new Vector2(v3.x, v3.y));
                uvs.Add(new Vector2(v4.x, v4.y));
            }
            else if (v1.y == v2.y && v1.y == v3.y && v1.y == v4.y)
            {
                uvs.Add(new Vector2(v1.x, v1.z));
                uvs.Add(new Vector2(v2.x, v2.z));
                uvs.Add(new Vector2(v3.x, v3.z));
                uvs.Add(new Vector2(v4.x, v4.z));
            }
            else if (v1.x == v2.x && v1.x == v3.x && v1.x == v4.x)
            {
                uvs.Add(new Vector2(v3.y, v3.z));
                uvs.Add(new Vector2(v4.y, v4.z));
                uvs.Add(new Vector2(v1.y, v1.z));
                uvs.Add(new Vector2(v2.y, v2.z));
            }
        }

        mesh.triangles = Indices;
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        mesh.UploadMeshData(false);

        GetComponent<BoxCollider>().center = size / 2;
        GetComponent<BoxCollider>().size = size;

        filter.mesh = mesh;
    }
}
