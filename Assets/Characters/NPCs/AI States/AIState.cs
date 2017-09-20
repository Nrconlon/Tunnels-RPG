using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

	protected MolatAIController m_molatAIController;
	
	public void Initialize(AIState previousState)
	{
		this.m_molatAIController = previousState.m_molatAIController;
	}

	public void Initialize(MolatAIController molatAIController)
	{
		this.m_molatAIController = molatAIController;
	}

	public MolatAIController MolatAIController
	{
		get { return m_molatAIController; }
		set { m_molatAIController = value; }
	}

	public virtual void NewMolatInSight(Molat newTarget)
	{

	}
	public virtual void NewMolatOutOfSight(Molat newTarget)
	{

	}
	public virtual void NewTargetInSight(Spider newTarget)
	{

	}
	public virtual void LostSightOfTarget(Molat oldarget)
	{

	}
	public virtual void GotHit(GameObject instigator)
	{

	}

	public virtual void ChooseLocation()
	{

	}

	public virtual void ChooseAction()
	{

	}
	public virtual void InAttackRange()
	{

	}
	public virtual void NewClosestTarget()
	{

	}
	public virtual void NewSpiderInSight(Spider spider)
	{

	}
	public virtual void NewSpiderOutOfSight(Spider spider)
	{

	}

	protected void ChangeState(AIState newState)
	{
		m_molatAIController.AIState = newState;
	}
}

//new ParameterOverride("car", "new car").OnType<NewCar>()