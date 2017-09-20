using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateFighting : AIState
{
	public override void NewMolatInSight(Molat newTarget)
	{
		base.NewMolatInSight(newTarget);

	}

	public override void NewMolatOutOfSight(Molat newTarget)
	{

			m_molatAIController.SetState(m_molatAIController.CreatePreferedState(), this);

		
	}
}
