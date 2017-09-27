using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraRaycaster))]
public class CursorAffordance : MonoBehaviour {

    [SerializeField] Texture2D spawnCursor = null;
    [SerializeField] Texture2D unknownCursor = null;
	[SerializeField] Texture2D selectCursor = null;

	[SerializeField] const int spawnNumber = 0;
	[SerializeField] const int uINumber = 5;

	[SerializeField] Vector2 cursorHotspot = Vector2.zero;

	CameraRaycaster cameraRaycaster;

	// Use this for initialization
	void Start () {
        cameraRaycaster = GetComponent<CameraRaycaster>();
        cameraRaycaster.NotifyLayerChangeObservers += OnLayerChanged; // registering
	}

    void OnLayerChanged(int newLayer) {
        switch (newLayer)
        {
		case uINumber:
			if(!selectCursor)
			{
				cursorHotspot = Vector2.zero;
			}
            Cursor.SetCursor(selectCursor, cursorHotspot, CursorMode.Auto);
            break;
        case spawnNumber:
			if(!spawnCursor)
			{
				cursorHotspot = Vector2.zero;
			}
            Cursor.SetCursor(spawnCursor, cursorHotspot, CursorMode.Auto);
            break;
        default:
			if (!unknownCursor)
			{
				cursorHotspot = Vector2.zero;
			}
			Cursor.SetCursor(unknownCursor, cursorHotspot, CursorMode.Auto);
            return;
        }
    }
	// TODO consider adding layers and curser targets
    // TODO consider de-registering OnLayerChanged on leaving all game scenes
}
