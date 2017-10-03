using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

public class MouseAndRaycast : MonoBehaviour
{
	// INSPECTOR PROPERTIES RENDERED BY CUSTOM EDITOR SCRIPT
	[SerializeField] Texture2D spawnCursor = null;
	[SerializeField] Texture2D unknownCursor = null;
	[SerializeField] Texture2D selectCursor = null;

	[SerializeField] const int spawnLayer = 0;
	[SerializeField] const int uILayer = 5;
	[SerializeField] const int itemLayer = 9;
	[SerializeField] const int livingCreatureLayer = 11;

	[SerializeField] Vector2 cursorHotspot = Vector2.zero;

	[HideInInspector] public bool lookingForCharacter = false;
	[HideInInspector] public bool lookingForItem = false;

	float maxRaycastDepth = 100f; // Hard coded value
	int topPriorityLayerLastFrame = -1; // So get ? from start with Default layer terrain

	private UserArenaController userArenaController;
	private Camera myCamera;
	private RaycastHit? priorityHit = null;
	public bool inGame = false;

	public delegate void ClickedOnMapDelegate(RaycastHit? raycastHit); // declare new delegate type
	public event ClickedOnMapDelegate ClickedOnMapDel; // instantiate an observer set


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
			priorityHit = null;
			return;
		}

		// Raycast to max depth, every frame as things can move under mouse
		Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] raycastHits = Physics.RaycastAll (ray, maxRaycastDepth);
		if(!lookingForCharacter)
		{
			priorityHit = FindLayerHit(raycastHits,spawnLayer);
		}
		else
		{
			priorityHit = FindLayerHit(raycastHits, livingCreatureLayer);
			if(!priorityHit.HasValue && lookingForItem)
			{
				priorityHit = FindLayerHit(raycastHits, itemLayer);
			}
		}
        
        if (!priorityHit.HasValue) // if hit no priority object
		{
			CheckIfLayerChange (0); // broadcast default layer
			
			return;
		}

		if (Input.GetMouseButtonDown(0) && !inGame)
		{
			if (priorityHit != null)
			{
				if(ClickedOnMapDel != null)
				{
					ClickedOnMapDel(priorityHit);
				}
			}
		}
		var layerHit = priorityHit.Value.collider.gameObject.layer;
		CheckIfLayerChange(layerHit);
	}

	public RaycastHit? GetPriorityHit()
	{
		return priorityHit;
	}

	void CheckIfLayerChange(int newLayer)
	{
		if (newLayer != topPriorityLayerLastFrame && !inGame)
		{
			topPriorityLayerLastFrame = newLayer;
			
			switch (newLayer)
			{
				case uILayer:
					if (!selectCursor)
					{
						cursorHotspot = Vector2.zero;
					}
					Cursor.SetCursor(selectCursor, cursorHotspot, CursorMode.Auto);
					break;
				case spawnLayer:
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

	RaycastHit? FindLayerHit(RaycastHit[] raycastHits, int layer)
	{
		// Step through layers in order of priority looking for a gameobject with that layer

		foreach (RaycastHit hit in raycastHits)
		{
			if (hit.collider.gameObject.layer == layer)
			{
				return hit; // stop looking
			}
		}
		return null;
	}

	public void SpawnAIActivated()
	{

	}
	public void SpawnSelfActivated()
	{

	}
	public void TakeControlActivated()
	{

	}
	public void DeleteCharacter()
	{

	}
}