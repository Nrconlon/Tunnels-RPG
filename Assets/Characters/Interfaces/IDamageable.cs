using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
	void TakeDamage(float damage, float force, EDamageType type, GameObject instigator);
}

public enum EDamageType { blunt, spiked, sharp };