using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateFighting : AIState
{
	Item targetItem = null;
	bool inAction = false;
	bool enemyFleeing = false;
	bool madeFirstMove = false;
	Vector3 fightingDestination = Vector3.zero;
	bool reactive = false;
	Molat targetMolat;
	bool reactionDestinationChosen;
	bool pickedDirection = false;
	bool preferRight;
	float attackAfterJumpTime = 0.3f;

	//on tick

	public override Vector3 ChooseWalkingDestination()
	{
		if (!m_molatAIController.CurrentTarget)
		{
			ChangeState(StateEnum.Idle);
		}
		if ((reactive || m_Molat.CurrentCooldown > 0))
		{
			m_Molat.Block(true);
			Vector3 correctVectorFromTarget = (transform.position - m_molatAIController.CurrentTarget.transform.position).normalized * m_molatAIController.PrefDistFromTarget;
			Vector3 vectorFromTargetTurned;
			if (!pickedDirection)
			{
				preferRight = RandomChance(0.5f);
				pickedDirection = true;
			}

			if(preferRight)
			{
				vectorFromTargetTurned = Quaternion.Euler(0, -90, 0) * correctVectorFromTarget;
			}
			else
			{
				vectorFromTargetTurned = Quaternion.Euler(0, 90, 0) * correctVectorFromTarget;
			}
			return m_molatAIController.CurrentTarget.transform.position + vectorFromTargetTurned;
		}
		else
		{
			pickedDirection = false;
		}
		return fightingDestination;
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

		targetItem = null;
		base.GotHit(instigator);
	}

	public override void ChooseAction()
	{
		//Putting choose destination into choose action  (poor designing, it always should have been done this way.)
		//3 conditions (or states).  an item is a priority, the enemy is running, and fighting.
		//Return if you set the destination, otherwise let it go to the end to set dest to fighting position.

		//Always weapon equiped
		if (!m_Molat.IsWeaponEquiped)
		{
			m_Molat.ToggleEquipWeapon();
		}

		//We are going to an item.  Only Cancel On Hit.  dont both with anything else
		if (targetItem)
		{
			if (targetItem.CanBePickedUp)
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
		else if (enemyFleeing)
		{
			GameObject currentTarget = m_molatAIController.CurrentTarget;
			float distanceFromTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
			//ITEM CHECK
			targetItem = ItemCheck(distanceFromTarget + m_molatAIController.GrabItemWhileFightingDist);  //Check for an item closer than the target times 1.5f
			if (targetItem)
			{
				//Start going for Item.  
				fightingDestination = targetItem.transform.position;
				return;
			}
			Vector3 targetLookDirection = m_molatAIController.CurrentTarget.transform.forward;
			Vector3 directionTowoardsMe = transform.position - m_molatAIController.CurrentTarget.transform.position;
			float angleOfLooking = Vector3.Angle(targetLookDirection, directionTowoardsMe);
			m_Molat.Attack();
			//is target not fleeing anymore?
			if (angleOfLooking < 90)
			{
				m_Molat.StopTailSprint();
				enemyFleeing = false;
			}
			//no return so that we go to the end
		}
		else if (!inAction)
		{
			m_Molat.Jump(false, Vector3.zero);
			m_Molat.StopTailSprint();  //Incase sprinting, stop sprinting
			GameObject currentTarget = m_molatAIController.CurrentTarget;
			//We have Target
			if (m_molatAIController.CurrentTarget)
			{
				float distanceFromTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
				//ITEM CHECK
				if (distanceFromTarget > m_molatAIController.FightRadius)
				{
					targetItem = ItemCheck(distanceFromTarget + m_molatAIController.GrabItemWhileFightingDist);  //Check for an item closer than the target times 1.5f
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
				}

				//No Item  FLEE CHECK
				Vector3 targetLookDirection = m_molatAIController.CurrentTarget.transform.forward;
				Vector3 directionTowoardsMe = transform.position - m_molatAIController.CurrentTarget.transform.position;
				float angleOfLooking = Vector3.Angle(targetLookDirection, directionTowoardsMe);
				//is target fleeing?
				if (angleOfLooking > 90 && madeFirstMove)
				{
					enemyFleeing = true;
					m_Molat.TailSprint(true, m_Molat.targetDirection);
					inAction = true;
					StartCoroutine(ThrowMace(RandomDelay(3)));
					return;
				}


				//No item, not fleeing, and not in action.  
				//in Range?
				if (distanceFromTarget < m_molatAIController.FightRadius)
				{
					if (!madeFirstMove)
					{
						StartingAction();
						//First encounter.  1/4 power strike, 1/4 block, 1/4 normal attack, 1/4 defensive/reactive
						if (RandomChance(0.25f) && m_Molat.MyShield)
						{
							StartCoroutine(NormalBlock(RandomDelay(0.2f)));
						}
						else if (RandomChance(0.35f))
						{
							StartCoroutine(NormalAttack(RandomDelay(0.1f)));
						}
						else if (RandomChance(0.25f))
						{
							PowerStrike();
						}
						else
						{
							BecomeReactive(UnityEngine.Random.Range(0.5f, 2f));
						}
					}
					else
					{
						StartingAction();
						if (RandomChance(0.7f))
						{
							if (RandomChance(0.25f))
							{
								PowerStrike();
							}
							else
							{
								StartCoroutine(NormalAttack(RandomDelay(0.1f)));
							}
						}
						else if (RandomChance(0.25f) && m_Molat.MyShield)
						{
							StartCoroutine(NormalBlock(RandomDelay(0.2f)));
						}
						else
						{
							BecomeReactive(UnityEngine.Random.Range(0.5f, 2f));
						}
					}
				}
			}
		}
		//we havent found an action
		Vector3 correctDistanceFromTarget = (transform.position - m_molatAIController.CurrentTarget.transform.position).normalized * m_molatAIController.PrefDistFromTarget;
		fightingDestination = m_molatAIController.CurrentTarget.transform.position + correctDistanceFromTarget;
		return;
	}

	void AMolatExpressedAction(Action action, Molat Instigator)
	{
		if (m_molatAIController.CurrentTarget.GetComponent<Molat>() == targetMolat)
		{
			if (!inAction && (reactive || m_Molat.CurrentCooldown > 0))
			{
				switch(action)
				{
					case Action.PowerAttack:
						if(RandomChance(0.7f))
						{
							StartCoroutine(TailJumpBack(RandomDelay(0.2f)));
						}
						break;
					case Action.ThrowWeapon:
						if (RandomChance(0.7f))
						{
							StartCoroutine(TailJumpSide(RandomDelay(0.2f)));
						}
						break;
					case Action.Attack:
						if (RandomChance(0.8f))
						{
							StartCoroutine(TailJumpSide(RandomDelay(0.5f)));
						}
						else if(RandomChance(0.2f) && m_Molat.MyShield)
						{
							StartCoroutine(NormalBlock(RandomDelay(0.5f)));
						}
						break;
					case Action.GotBlocked:
						if (RandomChance(0.9f))
						{
							StartCoroutine(NormalAttack(RandomDelay(0.5f)));
						}
						break;
				}  
			}
		}
	}

	private IEnumerator TailJumpSide(float delay)
	{
		yield return new WaitForSeconds(delay);
		Vector3 jumpDirection;
		if(RandomChance(0.5f))
		{
			jumpDirection = Quaternion.Euler(0, 90, 0) * m_Molat.lookAtDirection;
		}
		else
		{
			jumpDirection = Quaternion.Euler(0, -90, 0) * m_Molat.lookAtDirection;
		}
		m_Molat.TailSprint(true, jumpDirection);
	}
	private IEnumerator TailJumpBack(float delay)
	{
		yield return new WaitForSeconds(delay);
		Vector3 jumpDirection = -m_Molat.lookAtDirection;
		m_Molat.TailSprint(true, jumpDirection);
	}

	private void BecomeReactive(float delay)
	{
		reactive = true;
		StartCoroutine(StopBecomeReactive(delay));
	}
	private IEnumerator StopBecomeReactive(float delay)
	{
		yield return new WaitForSeconds(delay);
		inAction = false;
		reactive = false;
	}

	private IEnumerator NormalBlock(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_Molat.Block(true);
		StartCoroutine(StopBlocking(RandomDelay(1.5f)));
	}
	private IEnumerator StopBlocking(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_Molat.Block(false);
		StartCoroutine(FinishedMove(0.3f));
	}

	void StartingAction()
	{
		inAction = true;
		madeFirstMove = true;
	}

	private IEnumerator ThrowMace(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_Molat.ThrowWeapon();
		inAction = false;
	}

	void PowerStrike()
	{
		m_Molat.Jump(true, m_Molat.lookAtDirection);
		StartCoroutine(NormalAttack(attackAfterJumpTime));
	}
	private IEnumerator NormalAttack(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_Molat.Attack();
		m_Molat.Jump(false, Vector3.zero);
		StartCoroutine(FinishedMove(0.5f));
	}

	private IEnumerator FinishedMove(float delay)
	{
		yield return new WaitForSeconds(delay);
		inAction = false;
	}

	float RandomDelay(float max)
	{
		float extraDelay;
		if(m_molatAIController.Skill == 1)
		{
			extraDelay = 0.2f;
		}
		else if (m_molatAIController.Skill == 2)
		{
			extraDelay = 0;
		}
		else
		{
			extraDelay = -0.1f;
		}
		return UnityEngine.Random.Range(0.1f + extraDelay, max + extraDelay);
	}
	bool RandomChance(float chance)
	{
		float result = UnityEngine.Random.Range(0f, 1f);
		return result < chance;
	}

	public override void NewTargetAquired(GameObject newObject)
	{
		if(targetMolat)
		{
			targetMolat.ActionExpressedDel -= AMolatExpressedAction;
		}
		targetMolat = newObject.GetComponent<Molat>();
		targetMolat.ActionExpressedDel += AMolatExpressedAction;
		ResetFighting();
	}
	private void ResetFighting()
	{
		madeFirstMove = false;
		StopAllCoroutines();
		inAction = false;
		m_Molat.Jump(false, Vector3.zero);
		m_Molat.StopTailSprint();
	}

	public override void ResetAIStateInfo()
	{
		StopAllCoroutines();
		base.ResetAIStateInfo();
	}

	public override void FirstTargetAquired(GameObject newObject)
	{
		if (targetMolat)
		{
			targetMolat.ActionExpressedDel -= AMolatExpressedAction;
		}
		targetMolat = newObject.GetComponent<Molat>();
		targetMolat.ActionExpressedDel += AMolatExpressedAction;
		base.FirstTargetAquired(newObject);
	}

	public override void Died(GameObject instigator)
	{
		if(targetMolat)
		{
			targetMolat.ActionExpressedDel -= AMolatExpressedAction;
		}
		base.Died(instigator);
	}
}
