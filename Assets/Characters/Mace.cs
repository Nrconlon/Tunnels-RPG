using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mace : Weapon {

	public void Reset()
	{
		baseDamage = 50f;
		force = 250;
		currentDamage = baseDamage;
		isActivated = false;
		damageType = EDamageType.spiked;
	}	
}
