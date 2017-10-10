using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
using System;

public class MouseAndRaycast : MonoBehaviour
{
	// INSPECTOR PROPERTIES RENDERED BY CUSTOM EDITOR SCRIPT
	[SerializeField] Texture2D spawnCursor = null;
	[SerializeField] Texture2D maceCursor = null;
	[SerializeField] Texture2D shieldCursor = null;
	[SerializeField] Texture2D healCursor = null;
	[SerializeField] Texture2D circleCursor = null;
	[SerializeField] Texture2D deleteCursor = null;
	[SerializeField] Texture2D defaultCursor = null;

	[SerializeField] const int spawnLayer = 0;
	[SerializeField] const int uILayer = 5;
	[SerializeField] const int itemLayer = 9;
	[SerializeField] const int livingCreatureLayer = 11;

	[SerializeField] Vector2 cursorDefaultSpot = Vector2.zero;
	[SerializeField] Vector2 cursorNewSpot = new Vector2(100, 100);

	[HideInInspector] public bool lookingForObject = false;
	[HideInInspector] public bool lookingForItem = false;

	UIButtonSelection selectedButton = UIButtonSelection.None;

	float maxRaycastDepth = 100f; // Hard coded value
	int layerLastFrame = -1; // So get ? from start with Default layer terrain

	private Camera myCamera;
	private RaycastHit? priorityHit = null;
	[HideInInspector] public bool inGame = false;

	public delegate void ClickedOnMapDelegate(RaycastHit? raycastHit); // declare new delegate type
	public event ClickedOnMapDelegate ClickedOnMapDel; // instantiate an observer set


	private void Start()
	{
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
		else
		{
			CheckIfLayerChange(0);
			Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			Physics.Raycast(ray, out raycastHit, maxRaycastDepth);
			priorityHit = raycastHit;

			if (Input.GetMouseButtonDown(0) && !inGame)
			{
				if (priorityHit != null)
				{
					if (ClickedOnMapDel != null)
					{
						ClickedOnMapDel(priorityHit);
					}
				}
			}
		}

		// Raycast to max depth, every frame as things can move under mouse

		//if(!lookingForObject)
		//{
		//	priorityHit = FindLayerHit(raycastHits,spawnLayer);
		//}
		//else
		//{
		//	priorityHit = FindLayerHit(raycastHits, livingCreatureLayer);
		//	if(!priorityHit.HasValue && lookingForItem)
		//	{
		//		priorityHit = FindLayerHit(raycastHits, itemLayer);
		//	}
		//}
        
  //      if (!priorityHit.HasValue) // if hit no priority object
		//{
		//	CheckIfLayerChange (0); // broadcast default layer
			
		//	return;
		//}

		//var layerHit = priorityHit.Value.collider.gameObject.layer;
		//CheckIfLayerChange(layerHit);
	}

	public RaycastHit? GetPriorityHit()
	{
		return priorityHit;
	}

	void CheckIfLayerChange(int newLayer)
	{
		if (newLayer != layerLastFrame && !inGame)
		{
			layerLastFrame = newLayer;
			
			switch (newLayer)
			{
				case uILayer:
					SetCursor(UIButtonSelection.None);
					break;
				default:
					SetCursor(selectedButton);
					break;
			}
		}
	}

	internal void SwapPlayerMode(bool swapToInGame)
	{
		inGame = swapToInGame;
		SetCursor(UIButtonSelection.None);
	}

	RaycastHit? FindLayerHit(RaycastHit[] raycastHits, int layer)
	{

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

	internal void SelectedButton(UIButtonSelection buttonSelection)
	{
		selectedButton = buttonSelection;
		SetCursor(buttonSelection);

	}

	private void SetCursor(UIButtonSelection buttonSelection)
	{
		if(!inGame)
		{
			switch (buttonSelection)
			{
				case UIButtonSelection.Spawn:
					if (spawnCursor)
					{
						Cursor.SetCursor(spawnCursor, cursorNewSpot, CursorMode.Auto);
					}
					else
					{
						Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					}
					break;
				case UIButtonSelection.SpawnMace:
					if (spawnCursor)
					{
						Cursor.SetCursor(maceCursor, cursorNewSpot, CursorMode.Auto);
					}
					else
					{
						Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					}
					break;
				case UIButtonSelection.SpawnShield:
					if (spawnCursor)
					{
						Cursor.SetCursor(shieldCursor, cursorNewSpot, CursorMode.Auto);
					}
					else
					{
						Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					}
					break;
				case UIButtonSelection.SpawnHealing:
					if (spawnCursor)
					{
						Cursor.SetCursor(healCursor, cursorNewSpot, CursorMode.Auto);
					}
					else
					{
						Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					}
					break;
				case UIButtonSelection.SpawnMe:
					if (spawnCursor)
					{
						Cursor.SetCursor(circleCursor, cursorNewSpot, CursorMode.Auto);
					}
					else
					{
						Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					}
					break;
				case UIButtonSelection.Destroy:
					if (spawnCursor)
					{
						Cursor.SetCursor(deleteCursor, cursorNewSpot, CursorMode.Auto);
					}
					else
					{
						Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					}
					break;
				case UIButtonSelection.None:
					Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
					break;
			}
		}
		else
		{
			Cursor.SetCursor(defaultCursor, cursorDefaultSpot, CursorMode.Auto);
		}
	}
}