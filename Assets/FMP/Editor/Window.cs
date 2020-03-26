using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

public class Window : EditorWindow
{
    enum cameraDirection
    {
        Top,
        Left,
        Right,
        Front,
        Back
    }

    enum TransformationOperation
    {
        Move,Resize,Rotate
    }


    /*Window manipulation*/
	bool wireFrame;

	/*Rendering*/
    RenderTexture renderTexture;
    RenderTexture selectionRenderTexture;
    Vector2 lastSize;
	Camera viewCamera;
	cameraDirection currentCamDirection;

    private GameObject gridPlane;
    private Material gridMaterial;
    private Shader replacementShader;
    private Material effectMaterial;

    private bool isLeftClickDown;
    private bool wasMouseDragged;
    private Vector3 mouseWorldPos;
    private Vector3 mouseScreenPos;
    private Vector3 mouseDragStartScreenPos;
    private Vector3 mouseDragDelta;
    private Vector3 lastMouseDragStartScreenPos;
    private TransformationOperation transformationOperation;

    [MenuItem("Tools/FMP/Window")]
    static void Open()
	{
		Window win = (Window)EditorWindow.GetWindow(typeof(Window));
		win.Show();
	}

	void DestroyRenderTexture()
	{
        if(renderTexture) //Compatibility reasons
		    renderTexture.Release();
	}

	void CreateRenderTexture()
	{
		renderTexture = new RenderTexture((int)position.width, (int)position.height, 0, RenderTextureFormat.ARGB32);
        selectionRenderTexture = new RenderTexture((int)position.width, (int)position.height, 0, RenderTextureFormat.ARGB32);
        lastSize = new Vector2(position.width, position.height);
	}

	void Resize()
	{
		DestroyRenderTexture();
		CreateRenderTexture();

	}

	void Awake()
    {
        Grid.SetGridSize(1);
        wantsMouseMove = true;
		GameObject cam = new GameObject("cam");
		cam.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontSave;
		viewCamera = cam.AddComponent<Camera>();
        viewCamera.farClipPlane = 5000;
        viewCamera.backgroundColor = Color.black;
		viewCamera.clearFlags = CameraClearFlags.SolidColor;
		viewCamera.orthographic = true;

        gridPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        gridPlane.transform.parent = viewCamera.transform;
        gridPlane.transform.localPosition = new Vector3(0, 0, 1000);
        gridPlane.transform.localEulerAngles = new Vector3(-90,0,0);
		gridPlane.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontSave;
		gridPlane.GetComponent<MeshRenderer>().material = Resources.Load<Material>("GridMaterial");
        gridPlane.SetActive(false);
        gridMaterial = gridPlane.GetComponent<MeshRenderer>().sharedMaterial;

        replacementShader = Shader.Find("Hidden/FMP/SelectedObject");
        effectMaterial = new Material(Shader.Find("Hidden/FMP/SelectionEffect"));


        UpdateViewDirection();

		DestroyRenderTexture();
		CreateRenderTexture();
	}

    void OnDestroy()
	{
		DestroyRenderTexture();

		if(viewCamera)
		{
			DestroyImmediate(viewCamera.gameObject);
		}

		if(gridPlane)
			DestroyImmediate(gridPlane);
	}

	void UpdateViewDirection()
	{
        switch (currentCamDirection)
        {
            case cameraDirection.Top:
                viewCamera.transform.position = new Vector3(0, 1000, 0);
                viewCamera.transform.eulerAngles = new Vector3(90, 0, 0);
                gridMaterial.SetInt("_DrawVertically", 0);
                break;
            case cameraDirection.Left:
                viewCamera.transform.position = new Vector3(1000, 0, 0);
                viewCamera.transform.eulerAngles = new Vector3(0, -90, 0);
                gridMaterial.SetInt("_DrawVertically", 1);
				break;
            case cameraDirection.Right:
                viewCamera.transform.position = new Vector3(-1000, 0, 0);
                viewCamera.transform.eulerAngles = new Vector3(0, 90, 0);
                gridMaterial.SetInt("_DrawVertically", 1);
				break;
            case cameraDirection.Front:
                viewCamera.transform.position = new Vector3(0, 0, -1000);
                viewCamera.transform.eulerAngles = new Vector3(0, 0, 0);
                gridMaterial.SetInt("_DrawVertically", 2);
				break;
            case cameraDirection.Back:
                viewCamera.transform.position = new Vector3(0, 0, 1000);
                viewCamera.transform.eulerAngles = new Vector3(0, 180, 0);
                gridMaterial.SetInt("_DrawVertically", 2);
				break;
        }
    }

    Vector3 GetMoveDirection(Vector2 delta)
    {
        switch (currentCamDirection)
        {
            case cameraDirection.Top:
                return new Vector3(-Event.current.delta.x, 0, Event.current.delta.y) * 0.01f;
            case cameraDirection.Left:
                return new Vector3(0, Event.current.delta.y, -Event.current.delta.x) * 0.01f;
			case cameraDirection.Right:
                return new Vector3(0, Event.current.delta.y, Event.current.delta.x) * 0.01f;
			case cameraDirection.Front:
                return new Vector3(-Event.current.delta.x, Event.current.delta.y, 0) * 0.01f;
			case cameraDirection.Back:
                return new Vector3(Event.current.delta.x, Event.current.delta.y, 0) * 0.01f;
			default:
				return Vector3.zero;
        }
    }

    Vector3 ConvertToCameraRelative(Vector3 pos)
    {
        switch (currentCamDirection)
        {
            case cameraDirection.Top:
                return new Vector3(pos.x, 0, pos.z);
            case cameraDirection.Left:
                return new Vector3(0, pos.y, pos.z);
            case cameraDirection.Right:
                return new Vector3(0, pos.y, pos.z);
            case cameraDirection.Front:
                return new Vector3(pos.x, pos.y, 0);
            case cameraDirection.Back:
                return new Vector3(pos.x, pos.y, 0);
            default:
                return pos;
        }
    }

    Vector3 ConvertToCameraRelative(Vector2 pos)
    {
        switch (currentCamDirection)
        {
            case cameraDirection.Top:
                return new Vector3(pos.x, 0, pos.y);
            case cameraDirection.Left:
                return new Vector3(0, pos.y, pos.x);
            case cameraDirection.Right:
                return new Vector3(0, pos.y, pos.x);
            case cameraDirection.Front:
                return new Vector3(pos.x, pos.y, 0);
            case cameraDirection.Back:
                return new Vector3(pos.x, pos.y, 0);
            default:
                return pos;
        }
    }

    [MenuItem("Tools/FMP/Export OBJ")]
    static void ExportToObj()
    {
        StringBuilder str = new StringBuilder();
        var meshes = FindObjectsOfType<ProceduralCube>();
        int index = 0;
        int vertCount = 0;
        foreach (var cube in meshes)
        {
            var mesh = cube.GetComponent<MeshFilter>().sharedMesh;

            str.Append("#\n#object box\n#\n\n");
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                var v = cube.transform.TransformPoint(mesh.vertices[i]);
                str.Append($"v {v.x} {v.y} {-v.z}\n");
            }
            str.Append("\n"); 
            for (int i = 0; i < mesh.normals.Length; i++)
            {
                var v = cube.transform.localRotation * mesh.normals[i];
                str.Append($"vn {v.x} {v.y} {-v.z}\n");
            }
            str.Append("\n");
            for (int i = 0; i < mesh.uv.Length; i++)
            {
                var v = mesh.uv[i];
                str.Append($"vt {v.x} {v.y}\n");
            }
            str.Append("\n");

            str.Append($"o box{index}\n");
            str.Append($"g box{index}\n");

            for (int i = 0; i < mesh.triangles.Length; i+=3)
            {
                var first = mesh.triangles[i] + 1 + vertCount;
                var second = mesh.triangles[i + 1] + 1 + vertCount;
                var third = mesh.triangles[i + 2] + 1 + vertCount;
                str.Append($"f {third}/{third}/{third} {second}/{second}/{second} {first}/{first}/{first}\n");
            }
            str.Append("\n");
            vertCount += mesh.vertices.Length;
            index++;
        }

        string loc = EditorUtility.SaveFilePanel("Please select where to save the obj file.", "", "myScene", "obj");

        File.WriteAllText(loc, str.ToString());
    }

    void Update()
    {
        mouseWorldPos = (ConvertToCameraRelative(viewCamera.ScreenToWorldPoint(mouseScreenPos)));
        var snapped = Grid.Snap(mouseWorldPos);
        if (wasMouseDragged)
        {
            var dragPos = Grid.Snap(ConvertToCameraRelative(viewCamera.ScreenToWorldPoint(mouseDragStartScreenPos)));
            if (Selection.activeGameObject != null)
            {
                var cube = Selection.activeGameObject.GetComponent<ProceduralCube>();
                if (cube)
                {
                    if (transformationOperation == TransformationOperation.Move)
                    {
                        var size = cube.EndPosition - cube.transform.position;

                        Vector3 trans = Grid.Snap(ConvertToCameraRelative(viewCamera.ScreenToWorldPoint(mouseDragStartScreenPos + mouseDragDelta))) - dragPos;
                        lastMouseDragStartScreenPos = mouseDragStartScreenPos;
                        mouseDragStartScreenPos = mouseScreenPos;

                        if (mouseDragStartScreenPos == lastMouseDragStartScreenPos)
                            trans = Vector3.zero;

                        Selection.activeGameObject.transform.position += trans;
                        cube.EndPosition = cube.transform.position + size;
                    }
                    else if (transformationOperation == TransformationOperation.Resize)
                    {
                        var endPos = cube.EndPosition;
                        var size = cube.EndPosition - cube.transform.position;

                        Vector3 trans = Grid.Snap(ConvertToCameraRelative(viewCamera.ScreenToWorldPoint(mouseDragStartScreenPos + mouseDragDelta))) - dragPos;
                        lastMouseDragStartScreenPos = mouseDragStartScreenPos;
                        mouseDragStartScreenPos = mouseScreenPos;

                        if (mouseDragStartScreenPos == lastMouseDragStartScreenPos)
                            trans = Vector3.zero;

                        if (dragPos.x > cube.transform.position.x)
                        {
                            cube.EndPosition += new Vector3(trans.x, 0, 0);
                        }
                        else if (dragPos.x < cube.transform.position.x)
                        {
                            cube.transform.position += new Vector3(trans.x, 0, 0);
                        }

                        if (dragPos.z > cube.transform.position.z)
                        {
                            cube.EndPosition += new Vector3(0, 0, trans.z);
                        }
                        else if (dragPos.z < cube.transform.position.z)
                        {
                            cube.transform.position += new Vector3(0, 0, trans.z);
                        }

                        if (dragPos.y > cube.transform.position.y)
                        {
                            cube.EndPosition += new Vector3(0, trans.y, 0);
                        }
                        else if (dragPos.y < cube.transform.position.y)
                        {
                            cube.transform.position += new Vector3(0, trans.y, 0);
                        }
                        cube.UpdateMesh();
                    }
                }
            }
        }
    }

    void OnGUI()
	{
        //Check for window resizing and update rendertexture accordingly.
		if(position.width != lastSize.x || position.height != lastSize.y)
		{
			Resize();
        }

        foreach (var cube in FindObjectsOfType<ProceduralCube>())
        {
            int newVal = 0;
            if (Selection.gameObjects.Contains(cube.gameObject))
                newVal = 1;

            cube.GetComponent<MeshRenderer>().sharedMaterial.SetInt("_selected", newVal);
            Repaint();
        }

        //Get Input
        mouseScreenPos = Event.current.mousePosition;
        mouseScreenPos.y = viewCamera.pixelHeight - Event.current.mousePosition.y;

        if (Event.current.type == EventType.KeyDown)
		{
            if (Event.current.keyCode == KeyCode.Tab)
            {
                switch (currentCamDirection)
                {
                    case cameraDirection.Top:
                        currentCamDirection = cameraDirection.Left;
                        break;
                    case cameraDirection.Left:
                        currentCamDirection = cameraDirection.Right;
                        break;
                    case cameraDirection.Right:
                        currentCamDirection = cameraDirection.Front;
                        break;
                    case cameraDirection.Front:
                        currentCamDirection = cameraDirection.Back;
                        break;
                    case cameraDirection.Back:
                        currentCamDirection = cameraDirection.Top;
                        break;
                }

                UpdateViewDirection();
                ShowNotification(new GUIContent(currentCamDirection.ToString()));
            }
            else if (Event.current.keyCode == KeyCode.Equals)
            {
                if (Grid.GridSize < 64)
                {
                    Grid.SetGridSize(Grid.GridSize * 2);
                }

                ShowNotification(new GUIContent(Grid.GridSize.ToString()));
                Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Minus)
            {
                if (Grid.GridSize > 0.0625)
                {
                    Grid.SetGridSize(Grid.GridSize / 2);
                }

                ShowNotification(new GUIContent(Grid.GridSize.ToString()));
                Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Delete)
            {
                foreach (var gameObject in Selection.gameObjects)
                {
                    DestroyImmediate(gameObject);
                }
                Repaint();
            }
            else if (Event.current.keyCode == KeyCode.W)
                wireFrame = !wireFrame;
        } 

		if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
        {
            viewCamera.transform.position += GetMoveDirection(Event.current.delta) * (viewCamera.orthographicSize / 5);
			Repaint();
        }

        if (Event.current.type == EventType.MouseDrag)
        {
            if (isLeftClickDown)
            {
                mouseDragDelta = mouseScreenPos - mouseDragStartScreenPos;
                wasMouseDragged = true;
            }
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            var ray = viewCamera.ScreenPointToRay(mouseScreenPos);
            bool didClickOnObject = false;
            if (Physics.Raycast(ray, out RaycastHit hit, Single.PositiveInfinity))
            {
                if (hit.transform.gameObject.GetComponent<ProceduralCube>())
                {
                   if(Event.current.shift)
                        Selection.activeGameObject = hit.transform.gameObject;

                   if(hit.transform.gameObject == Selection.activeGameObject)
                    didClickOnObject = true;
                }
            }

            if (didClickOnObject)
                transformationOperation = TransformationOperation.Move;
            else
                transformationOperation = TransformationOperation.Resize;

            mouseDragStartScreenPos = mouseScreenPos;

            isLeftClickDown = true;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            var ray = viewCamera.ScreenPointToRay(mouseScreenPos);
            bool raycast = Physics.Raycast(ray, out RaycastHit hit, Single.PositiveInfinity);

            if (Selection.gameObjects.Length == 0 && !raycast)
                Create();
            else
            {
                if (!raycast && !wasMouseDragged)
                {
                    Selection.activeGameObject = null;
                }
            }

            void Create()
            {
                var pos = ConvertToCameraRelative(viewCamera.ScreenToWorldPoint(mouseScreenPos));

                if (ProceduralCube.Create(pos, Grid.GridSize * Vector3.one) != null)
                    Repaint();
            }

            mouseDragStartScreenPos = Vector3.zero;
            isLeftClickDown = false;
            wasMouseDragged = false;
        }

        if (Event.current.type == EventType.ScrollWheel)
        {
            var d = Event.current.delta.y;
            viewCamera.orthographicSize += d / 5;
            viewCamera.orthographicSize = Mathf.Clamp(viewCamera.orthographicSize, .5f, 100);

			Repaint();
		}

        if (Event.current.type == EventType.Repaint)
        {
            gridPlane.transform.localScale = new Vector3(position.width / position.height, 1, 1);
			gridPlane.transform.localScale *= (viewCamera.orthographicSize / 5f);

            gridMaterial.SetFloat("_GridSize", Grid.GridSize);

            var clearFlags = viewCamera.clearFlags;

            viewCamera.targetTexture = renderTexture;

            if(wireFrame)
                GL.wireframe = true;
            else
                gridPlane.SetActive(true);

            viewCamera.Render();
            gridPlane.SetActive(false);

            RenderTexture rt = RenderTexture.active;

            RenderTexture.active = selectionRenderTexture;
            GL.Clear(true, true, Color.clear);

            RenderTexture.active = rt;

            viewCamera.clearFlags = CameraClearFlags.Depth;
            viewCamera.SetReplacementShader(replacementShader, "RenderType");
            viewCamera.targetTexture = selectionRenderTexture;
            GL.wireframe = false;
            viewCamera.Render();
            viewCamera.SetReplacementShader(null, "");
            viewCamera.clearFlags = clearFlags;

            viewCamera.targetTexture = renderTexture;
        }

        //Selection.activeObject = renderTexture;
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), renderTexture);
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), selectionRenderTexture);
    }
}
