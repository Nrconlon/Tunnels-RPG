using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateIdle : AIState
{
	public override void NewMolatInSight(Molat newTarget)
	{
		base.NewMolatInSight(newTarget);
		ChangeState(m_molatAIController.preferedState);
	}
}

