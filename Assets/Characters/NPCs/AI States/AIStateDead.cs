using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateDead : AIState
{
	public override void ChooseAction()
	{
	}

	public override Vector3 ChooseLookAtDirection()
	{
		return base.ChooseLookAtDirection();
	}

	public override Vector3 ChooseWalkingDestination()
	{
		return base.ChooseWalkingDestination();
	}

	public override void Died(GameObject instigator)
	{
	}

	public override void GotHit(GameObject instigator)
	{
	}

	public override void IShouldFlee()
	{
	}

	public override void LostSightOfTarget(GameObject target)
	{
	}

	public override void NewClosestObject(GameObject newObject)
	{
	}

	public override void NewMolatInSight(Molat newTarget)
	{
	}

	public override void NewMolatOutOfSight(Molat newTarget)
	{
	}

	public override void NewSpiderInSight(Spider spider)
	{
	}

	public override void NewSpiderOutOfSight(Spider spider)
	{
	}
}
