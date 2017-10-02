using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

public class MouseAndRaycast : MonoBehaviour
{
	// INSPECTOR PROPERTIES RENDERED BY CUSTOM EDITOR SCRIPT
	[SerializeField] int[] layerPriorities;

	[SerializeField] Texture2D spawnCursor = null;
	[SerializeField] Texture2D unknownCursor = null;
	[SerializeField] Texture2D selectCursor = null;

	[SerializeField] const int spawnNumber = 0;
	[SerializeField] const int uINumber = 5;

	[SerializeField] Vector2 cursorHotspot = Vector2.zero;

	float maxRaycastDepth = 100f; // Hard coded value
	int topPriorityLayerLastFrame = -1; // So get ? from start with Default layer terrain

	private UserArenaController userArenaController;
	private Camera myCamera;


	private void Start()
	{
		userArenaController = GetComponent<UserArenaController>();
		myCamera = GetComponent<Camera>();
	}

	void Update()
	{
		// Check if pointer is over an interactable UI element
		if (EventSystem.current.IsPointerOverGameObject ())
		{
			CheckIfLayerChange (5);
			return;
		}

		// Raycast to max depth, every frame as things can move under mouse
		Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] raycastHits = Physics.RaycastAll (ray, maxRaycastDepth);

        RaycastHit? priorityHit = FindTopPriorityHit(raycastHits);
        if (!priorityHit.HasValue) // if hit no priority object
		{
			CheckIfLayerChange (0); // broadcast default layer
			
			return;
		}

		// Notify delegates of layer change
		var layerHit = priorityHit.Value.collider.gameObject.layer;
		CheckIfLayerChange(layerHit);
		// Notify delegates of highest priority game object under mouse when clicked
		if (Input.GetMouseButtonDown(0))
		{
			userArenaController.SpawnAICharacter(priorityHit.Value.point);
		}
	}

	void CheckIfLayerChange(int newLayer)
	{
		if (newLayer != topPriorityLayerLastFrame)
		{
			topPriorityLayerLastFrame = newLayer;
			
			switch (newLayer)
			{
				case uINumber:
					if (!selectCursor)
					{
						cursorHotspot = Vector2.zero;
					}
					Cursor.SetCursor(selectCursor, cursorHotspot, CursorMode.Auto);
					break;
				case spawnNumber:
					if (!spawnCursor)
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
					break;
			}
		}
	}

	RaycastHit? FindTopPriorityHit (RaycastHit[] raycastHits)
	{
		// Step through layers in order of priority looking for a gameobject with that layer
		foreach (int layer in layerPriorities)
		{
			foreach (RaycastHit hit in raycastHits)
			{
				if (hit.collider.gameObject.layer == layer)
				{
					return hit; // stop looking
				}
			}
		}
		return null; // because cannot use GameObject? nullable
	}
}