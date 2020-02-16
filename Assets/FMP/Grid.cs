using UnityEngine;

public static class Grid
{
    public static float GridSize { get; private set; } = 1;

    public static void SetGridSize(float size)
    {
        GridSize = size;
    }

    public static float Snap(float f)
    {
        return Mathf.Round(f);
    }

    public static Vector2 Snap(Vector2 f)
    {
        return new Vector2(Mathf.Round(f.x), Mathf.Round(f.y));
    }

    public static Vector3 Snap(Vector3 f)
    {
        return new Vector3(Mathf.Round(f.x), Mathf.Round(f.y), Mathf.Round(f.z));
    }

    public static Vector4 Snap(Vector4 f)
    {
        return new Vector4(Mathf.Round(f.x), Mathf.Round(f.y), Mathf.Round(f.z), Mathf.Round(f.w));
    }
}
