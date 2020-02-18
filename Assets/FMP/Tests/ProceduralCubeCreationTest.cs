using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralCubeCreationTest : MonoBehaviour
{
    public bool Create;
    public float GridSize = 1;

    void Update()
    {
        if (Create)
        {
            ProceduralCube.Create();
            Create = false;
        }

        Grid.SetGridSize(GridSize);
    }
}
