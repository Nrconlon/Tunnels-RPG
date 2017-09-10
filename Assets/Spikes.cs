using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {

	[SerializeField] float damageCaused = 10f;
	private void OnTriggerEnter(Collider collider)
	{
		Component damageableComponent = collider.gameObject.GetComponent(typeof(IDamageable));
		if(damageableComponent)
		{
			(damageableComponent as IDamageable).TakeDamage(damageCaused);
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
