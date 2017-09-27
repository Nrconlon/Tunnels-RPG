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
		if(m_molatAIController.CurrentTarget)
		{
			Vector3 correctDistanceFromTarget = (transform.position - m_molatAIController.CurrentTarget.transform.position).normalized * m_molatAIController.PrefDistFromTarget;
			return m_molatAIController.CurrentTarget.transform.position + correctDistanceFromTarget;
		}
		else
		{
			return base.ChooseWalkingDestination();
		}
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

	public override void ChooseAction()
	{
		if (!m_Molat.isWeaponEquiped)
		{
			m_Molat.ToggleEquipWeapon();
		}
		if(m_molatAIController.CurrentTarget !=  null)
		{
			if(Vector3.Distance(m_molatAIController.CurrentTarget.transform.position,transform.position) < m_molatAIController.AttackRadius)
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
}
