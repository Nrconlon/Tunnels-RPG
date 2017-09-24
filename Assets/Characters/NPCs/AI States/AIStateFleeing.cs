using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateFleeing : AIState
{
	private float fleeDistance = 1.0f;
	private float fleeLockTimer = 0f;
	int waypointCount = 0;

	public override void ChooseAction()
	{
		if (fleeLockTimer <Time.time)
		{
			fleeLockTimer = Time.time + m_molatAIController.fleeDuration;
			if (!IShouldFleeCheck())
			{
				ChangeState(DecideNextState());
			}
		}
		if (m_Molat.isWeaponEquiped)
		{
			m_Molat.ToggleEquipWeapon();
		}

	}

	public override Vector3 ChooseWalkingDestination()
	{
		Vector3 newDestination;
		if(m_molatAIController.waypoints.Count > waypointCount)
		{
			newDestination = m_molatAIController.waypoints[waypointCount].position;
			if(Vector3.Distance(transform.position, m_molatAIController.waypoints[waypointCount].position) < 2f)
			{
				waypointCount++;
			}
		}
		else
		{
			//Dynamic fleeing
			GameObject target = m_molatAIController.currentTarget;
			Vector3 directionAwayFromTarget = (transform.position - target.transform.position);
			Vector3 sourcePosition = directionAwayFromTarget + transform.position;
			NavMeshHit navHit;
			if (NavMesh.SamplePosition(sourcePosition, out navHit, fleeDistance, NavMesh.AllAreas))
			{
				newDestination = navHit.position;
			}
			else
			{
				newDestination = Vector3.zero; // transform.position;
			}
		}

		return newDestination;


	}

	public override void GotHit(GameObject instigator)
	{
		//wobble
	}

	public override void LostSightOfTarget(GameObject target)
	{
		if(m_molatAIController.mList_EnemySpiders.Count > 0)
		{
			m_molatAIController.SetTarget(getClosestSpiderObject());
		}
		else if(m_molatAIController.closestTarget != null)
		{
			m_molatAIController.SetTarget(m_molatAIController.closestTarget);
		}
		else
		{
			m_molatAIController.SetTarget(null);
		}
	}

	public override void NewClosestObject(GameObject newObject)
	{
		if (m_molatAIController.mList_EnemySpiders.Count > 0)
		{
			Spider spider = newObject.GetComponent<Spider>();
			if(spider)
			{
				m_molatAIController.SetTarget(newObject);
			}
		}
		else
		{
			m_molatAIController.SetTarget(newObject);
		}
	}

	public override void NewMolatOutOfSight(Molat newTarget)
	{
		//Overwrite, we dont want to go to default state through here, we do it in action flee check
	}

	public override void NewSpiderInSight(Spider spider)
	{
		m_molatAIController.SetTarget(m_molatAIController.closestTarget);
	}

	public override void NewSpiderOutOfSight(Spider spider)
	{
		base.NewSpiderOutOfSight(spider);
	}
	public override void IShouldFlee()
	{
		//do nothing because we are fleeing.
	}

	private GameObject getClosestSpiderObject()
	{
		Spider closestSpider = null;
		float closestDistance = float.PositiveInfinity;
		foreach (Spider entry in m_molatAIController.mList_EnemySpiders)
		{
			float distance = Vector3.Distance(entry.transform.position, transform.position);
			if(distance < closestDistance)
			{
				closestDistance = distance;
				closestSpider = entry;
			}
		}
		if (closestSpider)
		{
			return closestSpider.gameObject;
		}
		else return null;
	}

}
