using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

	public float durability;
	[HideInInspector] public float currentDurability;
	[HideInInspector] public bool isActivated = false;
	[HideInInspector] public GameObject instigator;
	[HideInInspector] public MolatSounds molatSounds;


	// Use this for initialization
	void Start () {
	}

	private void OnDestroy()
	{
		if (ItemDestroyDel != null)
		{
			ItemDestroyDel(gameObject);
		}
	}

	public delegate void OnItemDestroy(GameObject item); // declare new delegate type
	public event OnItemDestroy ItemDestroyDel; // instantiate an observer set

	protected void HandleDurabilityLoss(float durabilityLoss)
	{
		currentDurability = Mathf.Clamp(currentDurability - durabilityLoss, 0, durability);
		if (currentDurability <= 0)
		{
			PlayBreakSound();
			Destroy(gameObject);
		}
	}

	public virtual void Activate()
	{
		isActivated = true;
	}

	public virtual void Deactivate()
	{
		isActivated = false;
	}

	protected abstract void PlayBreakSound();


}
