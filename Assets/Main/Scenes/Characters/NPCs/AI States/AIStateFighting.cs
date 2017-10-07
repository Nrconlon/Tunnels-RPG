using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateFighting : AIState
{
	private bool targetInRange = false;
	private bool targetInRangePrevious = false;
	Item targetItem = null;
	List<Coroutine> breakableRoutines = new List<Coroutine>();
	bool inAction = false;
	Coroutine runningCoroutine;
	Vector3 fightingDestination = Vector3.zero;

	//on tick

	public override Vector3 ChooseWalkingDestination()
	{
		return fightingDestination;
		return base.ChooseWalkingDestination();
	}



	Item ItemCheck(float distance)
	{
		Item currentItem = null; ;
		if(!m_Molat.MyWeapon)
		{
			currentItem = m_Molat.playerItemFinder.GetClosestItem<Weapon>(distance);
			if(currentItem)
			{
				return currentItem;
			}
		}

		if(!m_Molat.MyShield)
		{
			currentItem = m_Molat.playerItemFinder.GetClosestItem<Shield>(distance);
			if (currentItem)
			{
				return currentItem;
			}
		}

		return null;
	}


	public override Vector3 ChooseLookAtDirection()
	{
		if (m_molatAIController.CurrentTarget)
		{
			return m_molatAIController.CurrentTarget.transform.position - transform.position;
		}
		else
		{
			return base.ChooseLookAtDirection();
		}

	}

	public override void GotHit(GameObject instigator)
	{
		//Reset intentions
		foreach (Coroutine routine in breakableRoutines)
		{
			StopCoroutine(routine);
		}
		targetItem = null;
		base.GotHit(instigator);
	}

	public override void ChooseAction()
	{
		//Putting choose destination into choose action  (poor designing, it always should have been done this way.)

		//Always weapon equiped
		if (!m_Molat.IsWeaponEquiped)
		{
			m_Molat.ToggleEquipWeapon();
		}

		//We are going to an item.  Only Cancel On Hit.  dont both with anything else
		if (targetItem)
		{
			if(targetItem.CanBePickedUp)
			{
				float distance = Vector3.Distance(transform.position, targetItem.transform.position);
				if (distance <= m_Molat.MaxPickupDistance)
				{
					m_Molat.TryToPickUpItem(targetItem);
					targetItem = null;
					return;
				}
				else
				{
					fightingDestination = targetItem.transform.position;
					return;

				}
			}
			else
			{
				targetItem = null;
				return;
			}
		}


		GameObject currentTarget = m_molatAIController.CurrentTarget;
		//We have Target
		if (m_molatAIController.CurrentTarget)
		{
			float distanceFromTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
			//We are not in Range
			if (distanceFromTarget > m_molatAIController.FightRadius)
			{
				targetItem = ItemCheck(distanceFromTarget);  //Check for an item closer than the target
				if (targetItem)
				{
					//Start going for Item.  
					fightingDestination = targetItem.transform.position;
					return;
				}
			}
			else
			{

				//we are in range, check if super close item.
				targetItem = ItemCheck(m_molatAIController.GrabItemWhileFightingDist);  //Check for an item closer than the target
				if (targetItem)
				{
					//Start going for Item.  
					fightingDestination = targetItem.transform.position;
					return;
				}

				Vector3 targetLookDirection = m_molatAIController.CurrentTarget.transform.forward;
				Vector3 directionTowoardsMe = transform.position - m_molatAIController.CurrentTarget.transform.position;
				float angleOfLooking = Vector3.Angle(targetLookDirection, directionTowoardsMe);
				//is target fleeing?
				if (angleOfLooking > 90)
				{
					m_Molat.TailSprint(true, m_Molat.targetDirection);
					inAction = true;
					StartCoroutine(ThrowMace(RandomDelay(3)));
					return;
				}
				else
				{
					m_Molat.StopTailSprint();
				}

				//Facing us


				targetInRange = true;
				if (targetInRangePrevious == false)
				{
					m_Molat.Jump(true, m_Molat.lookAtDirection);
				}
				m_Molat.Attack();
				targetInRangePrevious = targetInRange;
			}
			//we havent found an action
			Vector3 correctDistanceFromTarget = (transform.position - m_molatAIController.CurrentTarget.transform.position).normalized * m_molatAIController.PrefDistFromTarget;
			fightingDestination =  m_molatAIController.CurrentTarget.transform.position + correctDistanceFromTarget;
			return;
		}

		if(!inAction)
		{
			if (m_molatAIController.CurrentTarget)
			{
				if (Vector3.Distance(m_molatAIController.CurrentTarget.transform.position, transform.position) <= m_molatAIController.FightRadius)
				{
					
				}
			}
		}

	}

	private IEnumerator ThrowMace(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_Molat.ThrowWeapon();
		inAction = false;
	}

	float RandomDelay(float max)
	{
		return UnityEngine.Random.Range(0,max);
	}
	bool RandomChance(float chance)
	{
		float result = UnityEngine.Random.Range(0, 1);
		return result < chance;
	}

	public override void NewTargetAquired(GameObject newObject)
	{
		ResetFighting();
	}
	private void ResetFighting()
	{
		StopAllCoroutines();
		inAction = false;
		m_Molat.StopTailSprint();
	}
}
