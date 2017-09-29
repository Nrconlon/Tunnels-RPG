using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
	public float baseDamage;
	public float staminaCost;
	public float durability;
	public float durabilityLossPerHit;
	public float force;

	[HideInInspector] public float currentForce;
	[HideInInspector] public float currentDamage;
	[HideInInspector] public float currentDurability;
	[HideInInspector] public GameObject instigator;
	[HideInInspector] public bool isActivated = false;

	Collider myCollider;

	private List<GameObject> targetsHit = new List<GameObject>();

	public delegate void OnWeaponBreak(GameObject weapon); // declare new delegate type
	public event OnWeaponBreak WeaponBreakDel; // instantiate an observer set

	public delegate void OnWeaponDestroy(GameObject weapon); // declare new delegate type
	public event OnWeaponDestroy WeaponDestroyDel; // instantiate an observer set


	private void OnDestroy()
	{
		if(WeaponDestroyDel != null)
		{
			WeaponDestroyDel(gameObject);
		}
	}

	private void Start()
	{
		currentForce = force;
		currentDamage = baseDamage;
		currentDurability = durability;
		myCollider = GetComponent<Collider>();
		myCollider.enabled = false;
	}

	protected void OnTriggerEnter(Collider collider)
	{
		if (instigator && isActivated)
		{
			GameObject hitObject = collider.gameObject;
			if (hitObject != instigator && !targetsHit.Contains(hitObject))
			{
				Component damageableComponent = hitObject.GetComponent(typeof(IDamageable));
				if (damageableComponent)
				{
					HandleDurability();
					targetsHit.Add(hitObject);
					(damageableComponent as IDamageable).TakeDamage(currentDamage, currentForce, instigator.transform.forward, instigator);
				}
			}
		}
	}

	private void HandleDurability()
	{
		currentDurability = Mathf.Clamp(currentDurability - durabilityLossPerHit,0, durability);
		if (currentDurability <= 0 )
		{
			if(WeaponBreakDel != null)
			{
				WeaponBreakDel(gameObject);
			}
		}
	}

	public void ClearHitList()
	{
		targetsHit.Clear();
	}

	public void Activate()
	{
		isActivated = true;
		if(myCollider)
		{
			myCollider.enabled = true;
		}
		
	}

	public void Deactivate()
	{
		isActivated = false;
		if (myCollider)
		{
			myCollider.enabled = false;
		}
		ClearHitList();
	}
}
