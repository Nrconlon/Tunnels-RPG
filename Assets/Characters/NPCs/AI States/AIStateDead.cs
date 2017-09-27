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

	public override void FirstTargetAquired(GameObject newObject)
	{
	}

	public override void GotHit(GameObject instigator)
	{
	}

	public override void IShouldFlee()
	{
	}

	public override void NewTargetAquired(GameObject newObject)
	{
	}

	public override void NoTargetAvailable()
	{
	}
}
