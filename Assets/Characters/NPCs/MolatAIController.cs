using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Molat))]
public class MolatAIController : MonoBehaviour {
	[Header("NavMesh Settings / Target")]
	 NavMeshAgent agent;
	[SerializeField] float attackRadius = 3f;
	[SerializeField] float visionRadius = 10f;
	[SerializeField] float maxVisionAngle = 90f;
	[SerializeField] Transform lookFrom;

	public StateEnum preferedState = StateEnum.Idle;
	public Action currentAction;

	Molat m_Molat;
	[HideInInspector] public Molat currentTarget;
	[HideInInspector] public List<Molat> mList_EnemyMolats = new List<Molat>();
	[HideInInspector] public List<Spider> mList_EnemySpiders = new List<Spider>();
	Dictionary<GameObject, bool> _objectsInRangeDictionary = new Dictionary<GameObject, bool>();
	private Vector3 destination;
	private Vector3 lookAtDirection;
	bool isAttacking = false;
	bool isSprinting = false;
	bool isJumping = false;
	private AIState _AIState;



	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		m_Molat = GetComponent<Molat>();
		agent.updateRotation = false;
		agent.updatePosition = true;
		_AIState = CreatePreferedState();
		_AIState.Initialize(this);
		//TODO on raycasting vision, add targets;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (m_Molat.IsDead)
		{
			agent.enabled = false;
			SetTarget(null);
		}
		else
		{
			HandleRayCastVision();
			ChooseWalkingDirection();
			ChooseLookAtDirection();
			ChooseAction();
			// && agent.remainingDistance > agent.stoppingDistance

			//float distanceToTarget = Vector3.Distance(target.position, transform.position);

			//if (distanceToTarget <= attackRadius)
			//{
			//	isAttacking = true;

			//}
			//else
			//{
			//	isAttacking = false;
			//}

			//if (distanceToTarget <= visionRadius)
			//{
			//	agent.SetDestination(target.position);
			//	m_Molat.targetDirection = agent.desiredVelocity.normalized;
			//	m_Molat.lookAtDirection = (target.position - transform.position).normalized;
			//	m_Molat.TailSprint(isSprinting, m_Molat.lookAtDirection);
			//}
			//else
			//{
			//	agent.SetDestination(transform.position);
			//	m_Molat.targetDirection = Vector3.zero;
			//	m_Molat.lookAtDirection = Vector3.zero;
			//}

			//if (!m_Molat.isWeaponEquiped)
			//{
			//	m_Molat.ToggleEquipWeapon();
			//}

			//m_Molat.Attack(isAttacking);
			//m_Molat.Jump(isJumping, Vector3.zero);
		}
	}


	void expressAction(Action action, GameObject instigator)
	{
		if (action == Action.Attack)
		{
			m_Molat.TailSprint(true, -m_Molat.lookAtDirection);
		}
		else if (action == Action.Die)
		{
			//TODO watched enemy has died.
			currentTarget = null;
			isJumping = true;
			isAttacking = false;
		}
	}

	public void SetTarget(Molat newTarget)
	{
		this.currentTarget = newTarget;
	}

	public AIState AIState
	{
		get { return _AIState; }
		set { _AIState = value; }
	}

	//Called from collision Capsule
	public void GameObjectInRange(GameObject newObject)
	{
		if(!_objectsInRangeDictionary.ContainsKey(newObject))
		{
			_objectsInRangeDictionary.Add(newObject, false);
		}

	}
	public void GameObjectOutOfRange(GameObject newObject)
	{
		_objectsInRangeDictionary.Remove(newObject);
		
	}

	private void HandleRayCastVision()
	{
		float shortestDistance = float.PositiveInfinity;
		GameObject closestObject;
		RaycastHit Hit;

		List<GameObject> setToTrueList = new List<GameObject>();
		List<GameObject> setToFalseList = new List<GameObject>();
		
		foreach (KeyValuePair<GameObject, bool> entry in _objectsInRangeDictionary)
		{
			if (entry.Key == null)
				continue;
			Vector3 direction = ((entry.Key.transform.position + Vector3.up) - lookFrom.position);
			float visionAngle = Vector3.Angle(transform.forward, direction);
			Physics.Raycast(lookFrom.position, direction, out Hit,1000f);
			if (Hit.transform.root.gameObject == entry.Key)
			{
				//raycast hit object
				if (entry.Value == true)
				{
					//if we have already seen it, just record the distance
					if (shortestDistance > Hit.distance)
					{
						shortestDistance = Hit.distance;
						closestObject = entry.Key;
					}
				}
				else if(visionAngle < maxVisionAngle)
				{
					//First time seeing target, make sure angle is acceptable
					print("newCreature In sight");
					NewCreatureInSight(entry.Key);
					setToTrueList.Add(entry.Key);
				}
			


			}
			else
			{
				//Didnt see target
				if (entry.Value == true)
				{
					print("lost sightt");
					NewCreatureOutOfSight(entry.Key);
					setToFalseList.Add(entry.Key);
				}
			}
		}
		foreach (GameObject entry in setToFalseList)
		{
			_objectsInRangeDictionary[entry] = false;
		}
		foreach (GameObject entry in setToTrueList)
		{
			_objectsInRangeDictionary[entry] = true;
		}


	}

	private void NewCreatureInSight(GameObject newCreature)
	{
		Molat molat = newCreature.GetComponentInChildren<Molat>();
		if (molat)
		{
			addMolat(molat);
		}
		else
		{
			Spider spider = newCreature.GetComponentInChildren<Spider>();
			if (spider) addSpider(spider);
		}
	}

	private void NewCreatureOutOfSight(GameObject newCreature)
	{
		Molat molat = newCreature.GetComponentInChildren<Molat>();
		if (molat)
		{
			removeMolat(molat);
		}
		else
		{
			Spider spider = newCreature.GetComponentInChildren<Spider>();
			if (spider) removeSpider(spider);
		}
	}

	void addMolat(Molat newMolat)
	{
		mList_EnemyMolats.Add(newMolat);
		newMolat.actionExpressed += expressAction;
		NewMolatInSight(newMolat);
	}
	void removeMolat(Molat newMolat)
	{
		if (mList_EnemyMolats != null)
		{
			mList_EnemyMolats.Remove(newMolat);
		}
		NewMolatOutOfSight(newMolat);
		newMolat.actionExpressed -= expressAction;
	}

	void addSpider(Spider newSpider)
	{
		mList_EnemySpiders.Add(newSpider);
		NewSpiderInSight(newSpider);
	}
	void removeSpider(Spider newSpider)
	{
		if (mList_EnemySpiders != null)
		{
			mList_EnemySpiders.Remove(newSpider);
		}
		NewSpiderOutOfSight(newSpider);
	}

	private void NewMolatInSight(Molat newTarget)
	{
		_AIState.NewMolatInSight(newTarget);
	}
	private void NewMolatOutOfSight(Molat newTarget)
	{
		_AIState.NewMolatOutOfSight(newTarget);
	}
	private void NewSpiderInSight(Spider spider)
	{
		_AIState.NewSpiderInSight(spider);
	}
	private void NewSpiderOutOfSight(Spider spider)
	{
		_AIState.NewSpiderOutOfSight(spider);
	}
	private void TargetInRange(Molat target)
	{
		_AIState.LostSightOfTarget(target);
	}
	//Called from Molat on damage
	private void GotHit(GameObject instigator)
	{
		_AIState.GotHit(instigator);
	}
	private void Died(GameObject instigator)
	{
		//_AIState.Died(instigator);
	}
	private void ChooseWalkingDirection()
	{
		//_AIState.ChooseWalkingDirection();
	}
	private void ChooseLookAtDirection()
	{
		//_AIState.ChooseLookAtDirection();
	}
	private void ChooseAction()
	{
		//_AIState.ChooseAction();
	}

	public void SetState(AIState newState, AIState previousState)
	{
		Destroy(AIState);
		AIState = newState;
		AIState.Initialize(previousState);
	}

	public AIState CreatePreferedState()
	{
		switch(preferedState)
		{
			case(StateEnum.Idle):
				return gameObject.AddComponent<AIStateIdle>();
			case (StateEnum.Chasing):
				return gameObject.AddComponent<AIStateChasing>();
			case (StateEnum.Dead):
				return gameObject.AddComponent<AIStateDead>();
			case (StateEnum.Fighting):
				return gameObject.AddComponent<AIStateFighting>();
			case (StateEnum.Fleeing):
				return gameObject.AddComponent<AIStateFleeing>();
			case (StateEnum.Hiding):
				return gameObject.AddComponent<AIStateHiding>();
			case (StateEnum.Patrolling):
				return gameObject.AddComponent<AIStatePatrolling>();
		}
		return gameObject.AddComponent<AIStateIdle>();

	}






	private void OnDrawGizmos()
	{
		//Draw attack speher
		Gizmos.color = new Color(255f, 0f, 0, .5f);
		Gizmos.DrawWireSphere(transform.position, attackRadius);

		//Draw chase speher
		Gizmos.color = new Color(0f, 0f, 255f, .5f);
		Gizmos.DrawWireSphere(transform.position, visionRadius);
	}



}

public enum StateEnum
{
	Idle,
	Fighting,
	Fleeing,
	Hiding,
	Chasing,
	Dead,
	Patrolling
}

public enum Team
{
	Bad,
	Good,
	NeutralBad,
	NeutralGood
}