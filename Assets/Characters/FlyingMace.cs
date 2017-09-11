using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingMace : MonoBehaviour {

	public float projectileSpeed = 10f;
	public float damageCaused = 10f;
	private void OnTriggerEnter(Collider collider)
	{
		Component damageableComponent = collider.gameObject.GetComponent(typeof(IDamageable));
		if(damageableComponent)
		{
			(damageableComponent as IDamageable).TakeDamage(damageCaused);
		}
	}
}
