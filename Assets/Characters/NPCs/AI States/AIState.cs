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

	public virtual void NewMolatInSight(Molat newTarget)
	{
		//If its the first/only dude
		if (m_molatAIController.mList_EnemyMolats.Count == 1)
		{
			m_molatAIController.currentTarget = newTarget.gameObject;
		}
	}
	public virtual void NewMolatOutOfSight(Molat newTarget)
	{
		//if there are no more molats
		if (m_molatAIController.mList_EnemyMolats.Count == 0)
		{
			m_molatAIController.currentTarget = newTarget.gameObject;
			ChangeState(DecideNextState());
		}
	}
	public virtual void NewSpiderInSight(Spider spider)
	{
		
	}
	public virtual void NewSpiderOutOfSight(Spider spider)
	{

	}
	public virtual void LostSightOfTarget(GameObject target)
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
	public virtual void NewClosestObject(GameObject newObject)
	{

	}
	public virtual void IShouldFlee()
	{
		ChangeState(StateEnum.Fleeing);
	}

	protected void ChangeState(StateEnum newState)
	{
		m_molatAIController.currentStateEnum = newState;
		m_molatAIController.SetState(m_molatAIController.CreateState(newState), this);
	}
	protected StateEnum DecideNextState()
	{
		////I'm done with my last state  search?  or chill, fight?.
		if (m_molatAIController.mList_EnemyMolats.Count > 0)
		{
			return m_molatAIController.preferedState;
		}
		return StateEnum.Idle;
	}
	public bool IShouldFleeCheck()
	{
		if (m_molatAIController.mList_EnemySpiders.Count > 0)
		{
			return true;
		}
		else if (m_Molat.healthAsPercentage < m_molatAIController.healthThreshholdPercent)
		{
			return true;
		}
		else if (m_molatAIController.preferedState == StateEnum.Fleeing && m_molatAIController.mList_EnemyMolats.Count > 0)
		{
			return true;
		}
		else if (m_molatAIController.mList_EnemyMolats.Count > m_molatAIController.maxEnemiesBeforeFlee)
		{
			return true;
		}
		return false;
	}
}
