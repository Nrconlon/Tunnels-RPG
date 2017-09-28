using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mace : Weapon {

	public void Reset()
	{
		baseDamage = 50f;
		force = 40;
		currentDamage = baseDamage;
		isActivated = false;
		damageType = EDamageType.spiked;
	}	
}
