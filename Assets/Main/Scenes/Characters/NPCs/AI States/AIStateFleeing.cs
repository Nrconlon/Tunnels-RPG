using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateFleeing : AIState
{
	private float fleeDistance = 1.0f;
	private float fleeLockTimer = 0f;
	int waypointCount = 0;
	private void Start()
	{
		m_Molat.MolatSounds.FleeingSoundEffect();
	}

	public override void ChooseAction()
	{
		if (fleeLockTimer <Time.time)
		{
			m_Molat.TailSprint(true, m_Molat.targetDirection);
			fleeLockTimer = Time.time + m_molatAIController.FleeDuration;
			if (!IShouldFleeCheck())
			{
				ChangeState(DecideNextState());
			}
		}
		if (m_Molat.IsWeaponEquiped)
		{
			m_Molat.ToggleEquipWeapon();
		}
	}

	public override Vector3 ChooseWalkingDestination()
	{
		var waypoints = m_molatAIController.Waypoints;
		if (waypoints.Count > waypointCount)
		{
			if(Vector3.Distance(transform.position, waypoints[waypointCount].position) < 2f)
			{
				waypointCount++;
			}
			return waypoints[waypointCount].position;
		}
		else if (m_Molat.HealthAsPercentage < 1f)
		{
			if(m_molatAIController.healingStationController)
			{
				HealingStation closestStation = m_molatAIController.healingStationController.GetClosestStation(transform.position);
				if (closestStation)
				{
					return closestStation.transform.position;
				}
			}
		}

		//Dynamic fleeing
		GameObject target = m_molatAIController.CurrentTarget;
		if(target)
		{
			Vector3 directionAwayFromTarget = (transform.position - target.transform.position);
			Vector3 sourcePosition = directionAwayFromTarget + transform.position;
			NavMeshHit navHit;
			if (NavMesh.SamplePosition(sourcePosition, out navHit, fleeDistance, NavMesh.AllAreas))
			{
				return navHit.position;
			}
		}
		return base.ChooseWalkingDestination();
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
