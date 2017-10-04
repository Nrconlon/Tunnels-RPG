using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateIdle : AIState
{
	bool lookTooLong = false;
	bool lookTooLongToggle = false;
	float lookTooLongWalkDistance = 1f;
	float lookTooLongTime = 3f;
	Vector3 lookTooLongDestination = default(Vector3);
	Item targetItem = null;
	public override void ChooseAction()
	{
		if(targetItem)
		{
			float distance = Vector3.Distance(transform.position, targetItem.transform.position);
			if(distance <= m_Molat.MaxPickupDistance)
			{
				m_Molat.TryToPickUpItem(targetItem);
			}
		}
	}

	public override Vector3 ChooseWalkingDestination()
	{
		if(!m_Molat.MyWeapon)
		{
			targetItem = m_Molat.playerItemFinder.GetClosestItem<Weapon>();
		}
		else if (!m_Molat.MyShield)
		{
			targetItem = m_Molat.playerItemFinder.GetClosestItem<Shield>();
		}
		else
		{
			targetItem = null;
		}


		if(targetItem)
		{
			return targetItem.transform.position;
		}
		else if(m_Molat.HealthAsPercentage < 1f)
		{
			HealingStation closestStation = m_molatAIController.healingStationController.GetClosestStation(transform.position);
			if(closestStation)
			{
				return closestStation.transform.position;
			}
		}

		//Havent found a task.  default to turn after a certain time.

		if (!lookTooLong)
		{
			lookTooLong = true;
			StartCoroutine(LookTooLongTimer(lookTooLongTime));
		}

		if (lookTooLongDestination != default(Vector3))
		{
			return lookTooLongDestination;
			
		}
		

		
		return base.ChooseWalkingDestination();
	}

	private IEnumerator LookTooLongTimer(float duration)
	{
		yield return new WaitForSeconds(duration);
		lookTooLongDestination = (-m_Molat.transform.forward * lookTooLongWalkDistance) + transform.position;
		lookTooLong = false;

	}

	public override void NoTargetAvailable()
	{
		//comes to idle, overwrite ebcause we are here
	}
}

