using UnityEngine;

public static class Grid
{
    public static float GridSize { get; private set; }

    public static void SetGridSize(float size)
    {
        GridSize = size;
    }

    public static float Snap(float f)
    {
        return GridSize * Mathf.Floor(f / GridSize);
    }

    public static Vector2 Snap(Vector2 f)
    {
        return new Vector2(Snap(f.x), Snap(f.y));
    }

    public static Vector3 Snap(Vector3 f)
    {
        return new Vector3(Snap(f.x), Snap(f.y), Snap(f.z));
    }

    public static Vector4 Snap(Vector4 f)
    {
        return new Vector4(Snap(f.x), Snap(f.y), Snap(f.z), Snap(f.w));
    }

    public static Vector3 SnapSize(Vector3 f)
    {
        return new Vector3(Mathf.Ceil(f.x / GridSize) * GridSize, Mathf.Ceil(f.y / GridSize) * GridSize, Mathf.Ceil(f.z / GridSize) * GridSize);
    }
}
