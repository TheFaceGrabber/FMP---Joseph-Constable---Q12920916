using System;
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


    /*Window manipulation*/
	bool showGrid;
	Vector2 mousePos;
	Vector2 mouseDragStartPos;

	/*Rendering*/
    RenderTexture renderTexture;
	Vector2 lastSize;
	Camera viewCamera;
	cameraDirection currentCamDirection;

    private GameObject gridPlane;
    private Material gridMaterial;

    [MenuItem("Tools/FMP/Window")]
    static void Open()
	{
		Window win = (Window)EditorWindow.GetWindow(typeof(Window));
		win.Show();
	}

	void DestroyRenderTexture()
	{
		renderTexture?.Release();
	}

	void CreateRenderTexture()
	{
		renderTexture = new RenderTexture((int)position.width, (int)position.height, 16);
		lastSize = new Vector2(position.width, position.height);
		viewCamera.targetTexture = renderTexture;
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
        gridMaterial = gridPlane.GetComponent<MeshRenderer>().sharedMaterial;

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

    void OnGUI()
	{
        //Check for window resizing and update rendertexture accordingly.
		if(position.width != lastSize.x || position.height != lastSize.y)
		{
			Resize();
		}

		//Get Input
		if(Event.current.type == EventType.KeyDown)
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
        } 

		if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
        {
            viewCamera.transform.position += GetMoveDirection(Event.current.delta) * (viewCamera.orthographicSize / 5);
			Repaint();
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            var mousePos = Event.current.mousePosition;
            mousePos.y = viewCamera.pixelHeight - Event.current.mousePosition.y;

            var pos = ConvertToCameraRelative(viewCamera.ScreenToWorldPoint(mousePos));

            if(ProceduralCube.Create(pos, Grid.GridSize * Vector3.one) != null)
                Repaint();
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

			gridPlane.SetActive(true);

            gridMaterial.SetFloat("_GridSize", Grid.GridSize);
            viewCamera.Render();
            gridPlane.SetActive(false);
		}

		GUI.DrawTexture(new Rect(0, 0, position.width, position.height), renderTexture);
	}
}
