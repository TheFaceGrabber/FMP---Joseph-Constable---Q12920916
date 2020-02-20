using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralCubeCreationTest : MonoBehaviour
{
    public bool Create;
    public float GridSize = 1;
    public Vector3 CubeSize;

    void Update()
    {
        if (Create)
        {
            ProceduralCube.Create(Vector3.zero, CubeSize);
            Create = false;
        }

        Grid.SetGridSize(GridSize);
    }
}
