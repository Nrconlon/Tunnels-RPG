using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateIdle : AIState
{
	public override void NewMolatInSight(Molat newTarget)
	{
		base.NewMolatInSight(newTarget);
		m_molatAIController.SetState(gameObject.AddComponent<AIStateFighting>(), this);
	}

	public override void NewMolatOutOfSight(Molat newTarget)
	{
		base.NewMolatOutOfSight(newTarget);
	}
}

