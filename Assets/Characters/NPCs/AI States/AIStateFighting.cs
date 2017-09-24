using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateFighting : AIState
{
	private bool targetInRange = false;
	private bool targetInRangePrevious = false;

	//on tick

	public override Vector3 ChooseWalkingDestination()
	{
		if(m_molatAIController.currentTarget)
		{
			Vector3 correctDistanceFromTarget = (transform.position - m_molatAIController.currentTarget.transform.position).normalized * m_molatAIController.prefDistFromTarget;
			return m_molatAIController.currentTarget.transform.position + correctDistanceFromTarget;
		}
		else
		{
			return base.ChooseWalkingDestination();
		}
	}

	public override Vector3 ChooseLookAtDirection()
	{
		if (m_molatAIController.currentTarget)
		{
			return m_molatAIController.currentTarget.transform.position - transform.position;
		}
		else
		{
			return base.ChooseLookAtDirection();
		}

	}

	public override void ChooseAction()
	{
		if (!m_Molat.isWeaponEquiped)
		{
			m_Molat.ToggleEquipWeapon();
		}
		if(m_molatAIController.currentTarget !=  null)
		{
			if(Vector3.Distance(m_molatAIController.currentTarget.transform.position,transform.position) < m_molatAIController.attackRadius)
			{
				targetInRange = true;
				if(targetInRangePrevious == false)
				{
					m_Molat.Jump(true, m_Molat.lookAtDirection);
				}
				m_Molat.Attack(true);
				targetInRangePrevious = targetInRange;
			}
		}

	}

	public override void GotHit(GameObject instigator)
	{
		//Fight more defensively?  Back up to heal?  Block more?
		base.GotHit(instigator);
	}

	public override void LostSightOfTarget(GameObject target)
	{
		//Switch Targets
		if(m_molatAIController.closestTarget != null)
		{
			m_molatAIController.SetTarget(m_molatAIController.closestTarget);
		}
		else
		{
			m_molatAIController.SetTarget(null);
			ChangeState(DecideNextState());
		}
	}

	public override void NewClosestObject(GameObject newObject)
	{
		//Decide if I should switch targets
		m_molatAIController.SetTarget(newObject);
	}


}
