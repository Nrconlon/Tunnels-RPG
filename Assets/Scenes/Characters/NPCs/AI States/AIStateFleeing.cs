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
			fleeLockTimer = Time.time + m_molatAIController.FleeDuration;
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
		var waypoints = m_molatAIController.Waypoints;
		if (waypoints.Count > waypointCount)
		{
			newDestination = waypoints[waypointCount].position;
			if(Vector3.Distance(transform.position, waypoints[waypointCount].position) < 2f)
			{
				waypointCount++;
			}
		}
		else
		{
			//Dynamic fleeing
			GameObject target = m_molatAIController.CurrentTarget;
			if(target)
			{
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

	public override void IShouldFlee()
	{
		//do nothing because we are fleeing.
	}
}
