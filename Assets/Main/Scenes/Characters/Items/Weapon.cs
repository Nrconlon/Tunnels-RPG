﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
	public float baseDamage;
	public float durabilityLossPerHit;
	public float force;
	public float throwingDamage;

	[HideInInspector] public float currentForce;
	private bool beingThrown = false;
	[HideInInspector] public float currentDamage;

	Collider myCollider;

	private List<GameObject> targetsHit = new List<GameObject>();

	bool blocked = false;
	private void Update()
	{
		if(beingThrown)
		{
			Rigidbody myRigidbody = GetComponent<Rigidbody>();
			if (myRigidbody)
			{
				float speed = myRigidbody.velocity.magnitude;
				if (speed < 5.0f)
				{
					StopBeingThrown();
				}
			}
		}
	}
	private void Start()
	{
		currentDurability = durability;
		currentForce = force;
		currentDamage = baseDamage;
		//this is the trigger collider
		myCollider = GetComponent<Collider>();
		myCollider.enabled = false;
	}

	protected void OnTriggerEnter(Collider collider)
	{
		//if I have an instigator, im activate, and im not blocked
		if (instigator && isActivated && !blocked)
		{
			GameObject hitObject = collider.gameObject;
			//if the root is not me, and I havent hit this root
			if (hitObject != instigator && !targetsHit.Contains(hitObject))
			{
				Molat targetMolat = hitObject.transform.root.GetComponent<Molat>();
				if(targetMolat && !targetMolat.IsDead)
				{
					Component damageableComponent = hitObject.GetComponent(typeof(IDamageable));
					Shield shield = hitObject.GetComponentInChildren<Shield>();
					if (shield)
					{
						if (shield.isActivated)
						{
							Vector3 pointerFromYouToMe = (instigator.transform.position - hitObject.transform.position).normalized;
							float angleBetweenUs = Vector3.Angle(shield.lookAtDirection, pointerFromYouToMe);
							if (angleBetweenUs < shield.maxAngleBlock)
							{
								instigator.GetComponent<Molat>().GotBlocked();
								shield.BlockAHit(force);
								blocked = true;
								HitRegistered(hitObject);
							}

						}
					}
					if (damageableComponent && !blocked)
					{
						(damageableComponent as IDamageable).TakeDamage(currentDamage, currentForce, instigator.transform.forward, instigator);
						HitRegistered(hitObject);
					}
				}
			}
		}
	}
	private void HitRegistered(GameObject hitObject)
	{
		targetsHit.Add(hitObject);
		HandleDurabilityLoss(durabilityLossPerHit);
		if(beingThrown)
		{
			beingThrown = false;
			Deactivate();
		}
	}


	public void ClearHitList()
	{
		targetsHit.Clear();
	}

	public override void Activate()
	{
		blocked = false;
		base.Activate();
		if (myCollider)
		{
			myCollider.enabled = true;
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (myCollider)
		{
			myCollider.enabled = false;
		}
		ClearHitList();
	}
	protected override void PlayBreakSound()
	{
		if(molatSounds)
		{
			molatSounds.MaceBreakSoundEffect();
		}
	}

	public void BeingThrown(float damage)
	{
		beingThrown = true;
		currentDamage = throwingDamage;
		Activate();
	}

	void StopBeingThrown()
	{
		beingThrown = false;
		Deactivate();
	}
}
