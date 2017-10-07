using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerItemFinder : MonoBehaviour
{
	private void Start()
	{
		Molat myMolat = GetComponentInParent<Molat>();
		myMolat.PickUpOrDropDel += PickUpOrDrop;
	}
	List<Item> itemList = new List<Item>();
	void OnTriggerEnter(Collider collider)
	{
		Molat molat = collider.gameObject.GetComponent<Molat>();
		if(molat)
		{
			molat.PickUpOrDropDel += PickUpOrDrop;
		}
		else
		{
			Item item = collider.gameObject.GetComponent<Item>();
			if (item)
			{
				AddItem(item);
			}
		}
	}

	private void ItemDestroyed(Item item)
	{
		RemoveItem(item);
	}
	private void RemoveItem(Item item)
	{
		if (item.GetComponent<Rigidbody>())
		{
			item.ItemDestroyDel -= ItemDestroyed;
			itemList.Remove(item);
		}
	}
	private void AddItem(Item item)
	{
		if(item.GetComponent<Rigidbody>() && !itemList.Contains(item))
		{
			item.ItemDestroyDel += ItemDestroyed;
			itemList.Add(item);
		}
	}

	void OnTriggerExit(Collider collider)
	{
		Molat molat = collider.gameObject.GetComponent<Molat>();
		if (molat)
		{
			molat.PickUpOrDropDel -= PickUpOrDrop;
		}
		else
		{
			Item item = collider.gameObject.GetComponent<Item>();
			if (item)
			{
				RemoveItem(item);
			}
		}

	}
	void PickUpOrDrop(Item item, bool pickUp)
	{
		if(pickUp)
		{
			RemoveItem(item);
		}
		else
		{
			AddItem(item);
		}
	}



	public Item GetClosestItem(float maxDistance)
	{
		return GetClosestItem<Item>(maxDistance);
	}
	public Item GetClosestItem()
	{
		return GetClosestItem<Item>(float.PositiveInfinity);
	}
	public T GetClosestItem<T>()
	{
		return GetClosestItem<T>(float.PositiveInfinity);
	}


	/// <summary>
	/// T must be a subclass of Item.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="maxDistance"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	public T GetClosestItem<T>(float maxDistance)
	{
		Item closestItem = null;
		float closestDistance = float.PositiveInfinity;
		foreach (Item item in itemList)
		{
			if(!item.CanBePickedUp)
			{
				continue;
			}
			if (item is T)
			{
				float currentDistance = Vector3.Distance(transform.position, item.transform.position);
				if (currentDistance < closestDistance && currentDistance < maxDistance)
				{
					closestDistance = currentDistance;
					closestItem = item;
				}
			}
		}
		if(closestItem != null)
		{
			return closestItem.GetComponent<T>();
		}
		else
		{
			return default(T);
		}
		
	}

}

