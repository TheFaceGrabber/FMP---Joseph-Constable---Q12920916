using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralCubeCreationTest : MonoBehaviour
{
    public bool Create;

    void Update()
    {
        if (Create)
        {
            ProceduralCube.Create();
            Create = false;
        }
    }
}
