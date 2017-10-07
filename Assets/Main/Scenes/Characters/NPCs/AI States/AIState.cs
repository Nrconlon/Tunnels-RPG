using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIState : MonoBehaviour {

	protected MolatAIController m_molatAIController;
	protected Molat m_Molat;

	public void Initialize(AIState previousState)
	{
		this.m_molatAIController = previousState.m_molatAIController;
		this.m_Molat = previousState.m_Molat;
	}

	public void Initialize(MolatAIController molatAIController)
	{
		this.m_molatAIController = molatAIController;
		this.m_Molat = molatAIController.m_Molat;
	}

	public MolatAIController MolatAIController
	{
		get { return m_molatAIController; }
		set { m_molatAIController = value; }
	}

	public virtual Vector3 ChooseWalkingDestination()
	{
		return Vector3.zero;
	}
	public virtual Vector3 ChooseLookAtDirection()
	{
		return Vector3.zero;
	}
	public virtual void ChooseAction()
	{

	}
	public virtual void GotHit(GameObject instigator)
	{
		//play grunt and get knocked back.
		//If below health threshhold, change state to flee
	}
	public virtual void Died(GameObject instigator)
	{
		ChangeState(StateEnum.Dead);
	}

	public virtual void NoTargetAvailable()
	{
		ChangeState(StateEnum.Idle);
	}
	public virtual void NewTargetAquired(GameObject newObject)
	{

	}
	public virtual void FirstTargetAquired(GameObject newObject)
	{
		ChangeState(m_molatAIController.PreferedState);
	}

	public virtual void IShouldFlee()
	{
		ChangeState(StateEnum.Fleeing);
	}

	protected void ChangeState(StateEnum newState)
	{
		ResetAIStateInfo();
		m_molatAIController.currentStateEnum = newState;
		m_molatAIController.SetState(m_molatAIController.CreateState(newState), this);
	}

	private void ResetAIStateInfo()
	{
		m_Molat.TailSprint(false, Vector3.zero);
	}

	protected StateEnum DecideNextState()
	{
		////I'm done with my last state  search?  or chill, fight?.
		if (m_molatAIController.mList_EnemyMolats.Count > 0)
		{
			return m_molatAIController.PreferedState;
		}
		return StateEnum.Idle;
	}
	public bool IShouldFleeCheck()
	{
		if (m_molatAIController.mList_EnemySpiders.Count > 0)
		{
			return true;
		}
		else if (m_Molat.HealthAsPercentage < m_molatAIController.HealthThreshholdPercent)
		{
			return true;
		}
		else if (m_molatAIController.PreferedState == StateEnum.Fleeing && m_molatAIController.mList_EnemyMolats.Count > 0)
		{
			return true;
		}
		else if (m_molatAIController.mList_EnemyMolats.Count > m_molatAIController.MaxEnemiesBeforeFlee)
		{
			return true;
		}
		return false;
	}
}
