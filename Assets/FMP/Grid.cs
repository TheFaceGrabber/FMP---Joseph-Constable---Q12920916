using UnityEngine;

public static class Grid
{
    public static float Snap(float f, float GridSize)
    {
        return GridSize * Mathf.Floor(f / GridSize);
    }

    public static Vector2 Snap(Vector2 f, float GridSize)
    {
        return new Vector2(Snap(f.x, GridSize), Snap(f.y, GridSize));
    }

    public static Vector3 Snap(Vector3 f, float GridSize)
    {
        return new Vector3(Snap(f.x, GridSize), Snap(f.y, GridSize), Snap(f.z, GridSize));
    }

    public static Vector4 Snap(Vector4 f, float GridSize)
    {
        return new Vector4(Snap(f.x, GridSize), Snap(f.y, GridSize), Snap(f.z, GridSize), Snap(f.w, GridSize));
    }

    public static Vector3 SnapSize(Vector3 f, float GridSize)
    {
        return new Vector3(Mathf.Ceil(f.x / GridSize) * GridSize, Mathf.Ceil(f.y / GridSize) * GridSize, Mathf.Ceil(f.z / GridSize) * GridSize);
    }
}
