using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingStation : MonoBehaviour {
	[SerializeField] float healingAmount = 20f;
	[SerializeField] float amountToShrink = 0.15f;

	public delegate void DestroyMeDelegate(HealingStation destroyMeStation);
	public event DestroyMeDelegate DestroyMeDel;

	private HashSet<Molat> myMolats = new HashSet<Molat>();
	private void OnTriggerEnter(Collider other)
	{
		Molat molat = other.gameObject.GetComponent<Molat>();
		if(molat && !molat.IsDead)
		{ 
			myMolats.Add(molat);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Molat molat = other.gameObject.GetComponent<Molat>();
		if (molat)
		{
			myMolats.Remove(molat);
		}
	}

	private void Update()
	{
		List<Molat> deadMolats = new List<Molat>();
		HashSet<Molat> currentMolats = myMolats;
		foreach (Molat molat in currentMolats)
		{
			if(molat.IsDead)
			{
				deadMolats.Add(molat);
			}
			else
			{
				if (molat.Heal(healingAmount * Time.deltaTime))
				{
					molat.isHealing = true;
					Shrink(Time.deltaTime);
				}
				else
				{
					molat.isHealing = false;
				}
			}
		}
		foreach (Molat molat in deadMolats)
		{
			myMolats.Remove(molat);
		}
	}

	private void Shrink(float deltaTime)
	{
		if(transform.localScale.x <= (deltaTime * amountToShrink))
		{
			myMolats.Clear();
			DestroyMeDel(this);

		}
		else
		{
			float x = transform.localScale.x - (amountToShrink * deltaTime);
			float z = transform.localScale.z;
			float y = transform.localScale.y - (amountToShrink * deltaTime);
			transform.localScale = new Vector3(x, y, z);
		}
	}

}
