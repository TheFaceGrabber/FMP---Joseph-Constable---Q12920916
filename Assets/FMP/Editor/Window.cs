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
		Debug.Log("Resized to: " + lastSize);
	}

	void Resize()
	{
		DestroyRenderTexture();
		CreateRenderTexture();

        gridPlane.transform.localScale = new Vector3(position.width / position.height, 1, 1);
	}

	void Awake()
	{
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
        gridMaterial = gridPlane.GetComponent<MeshRenderer>().material;

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

    void OnGUI()
	{	//Check for window resizing and update rendertexture accordingly.
		if(position.width != lastSize.x || position.height != lastSize.y)
		{
			Resize();
		}

		//Get Input
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
		{
			switch(currentCamDirection)
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

		gridPlane.SetActive(true);

		gridMaterial.SetFloat("_GridSize", Grid.GridSize);
		viewCamera.Render();
		gridPlane.SetActive(false);
		GUI.DrawTexture(new Rect(0, 0, position.width, position.height), renderTexture);
	}
}
