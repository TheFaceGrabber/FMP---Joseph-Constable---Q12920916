using UnityEngine;
using UnityEditor;

public class Window : EditorWindow
{
enum cameraDirection{
	Top,
	Left,
	Right,
	Front,
	Back
}

	RenderTexture renderTexture;
	bool showGrid;
	Vector2 mousePos;
	Vector2 mouseDragStartPos;
	Vector2 lastSize;
	Camera viewCamera;
	cameraDirection currentCamDirection;

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
	}

	void Awake()
	{
		GameObject cam = new GameObject("cam");
		cam.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontSave;
		viewCamera = cam.AddComponent<Camera>();
		viewCamera.clearFlags = CameraClearFlags.SolidColor;
		viewCamera.orthographic = true;

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
	}

	void UpdateViewDirection()
	{
		switch(currentCamDirection)
			{
				case cameraDirection.Top:
					viewCamera.transform.position = new Vector3(0,1000,0);
					viewCamera.transform.eulerAngles = new Vector3(90,0,0);
				break;
				case cameraDirection.Left:
					viewCamera.transform.position = new Vector3(1000,0,0);
					viewCamera.transform.eulerAngles = new Vector3(0,-90,0);
				break;
				case cameraDirection.Right:
					viewCamera.transform.position = new Vector3(-1000,0,0);
					viewCamera.transform.eulerAngles = new Vector3(0,90,0);
				break;
				case cameraDirection.Front:
					viewCamera.transform.position = new Vector3(0,0,-1000);
					viewCamera.transform.eulerAngles = new Vector3(0,0,0);
				break;
				case cameraDirection.Back:
					viewCamera.transform.position = new Vector3(0,0,1000);
					viewCamera.transform.eulerAngles = new Vector3(0,180,0);
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


		GL.wireframe = true;
		viewCamera.Render();
		GL.wireframe = false;
		GUI.DrawTexture(new Rect(0, 0, position.width, position.height), renderTexture);
	}
}
