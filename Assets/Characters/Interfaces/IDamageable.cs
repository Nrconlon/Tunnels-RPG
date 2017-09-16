using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
	void TakeDamage(float damage, float force, Vector3 direction, EDamageType type, GameObject instigator);
}

public enum EDamageType { blunt, spiked, sharp };