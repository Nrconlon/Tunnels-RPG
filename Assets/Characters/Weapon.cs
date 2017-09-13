using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

	public float baseDamage = 10f;
	public float force = 250;
	public float currentForce = 0;
	public float currentDamage = 0;
	public GameObject instigator;
	public bool isActivated;
	public EDamageType damageType;
	private List<GameObject> targetsHit = new List<GameObject>();
	private bool wasActivated;

	protected void OnTriggerEnter(Collider collider)
	{
		GameObject hitObject = collider.gameObject;
		if (hitObject != instigator && isActivated && !targetsHit.Contains(hitObject))
		{
			Component damageableComponent = hitObject.GetComponent(typeof(IDamageable));
			if (damageableComponent)
			{
				targetsHit.Add(hitObject);
				(damageableComponent as IDamageable).TakeDamage(currentDamage, currentForce, damageType, instigator);
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
