using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {

	public float baseDamage = 10f;
	public float force = 250;
	[HideInInspector] public float currentForce = 0;
	[HideInInspector] public float currentDamage = 0;
	[HideInInspector] public GameObject instigator;
	[HideInInspector] public bool isActivated;
	[HideInInspector] public EDamageType damageType;
	private List<GameObject> targetsHit = new List<GameObject>();

	protected void OnTriggerEnter(Collider collider)
	{
		if(instigator)
		{
			GameObject hitObject = collider.gameObject;
			if (hitObject != instigator && isActivated && !targetsHit.Contains(hitObject))
			{
				Component damageableComponent = hitObject.GetComponent(typeof(IDamageable));
				if (damageableComponent)
				{
					targetsHit.Add(hitObject);
					(damageableComponent as IDamageable).TakeDamage(currentDamage, currentForce, instigator.transform.forward, damageType, instigator);
				}
			}
		}
	}

	public void clearHitList()
	{
		targetsHit.Clear();
	}

	public void Activate()
	{
		isActivated = true;
	}

	public void Deactivate()
	{
		isActivated = false;
		clearHitList();
	}

}
