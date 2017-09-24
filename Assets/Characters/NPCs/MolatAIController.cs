using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Molat))]
public class MolatAIController : MonoBehaviour {
	[Header("NavMesh Settings / Target")]
	 NavMeshAgent agent;
	
	[SerializeField] float visionRadius = 10f;
	[SerializeField] float maxVisionAngle = 90f;
	[SerializeField] Transform lookFrom;
	[SerializeField] float newDestDistance =2f;

	public StateEnum preferedState = StateEnum.Fighting;
	[HideInInspector] public StateEnum currentStateEnum;
	[HideInInspector] public Action currentAction;
	public Team myTeam;
	[HideInInspector] public Molat m_Molat;
	[HideInInspector] public GameObject currentTarget = null;
	[HideInInspector] public GameObject closestTarget = null;
	[HideInInspector] public List<Molat> mList_EnemyMolats = new List<Molat>();
	[HideInInspector] public List<Spider> mList_EnemySpiders = new List<Spider>();
	Dictionary<GameObject, bool> _objectsInRangeDictionary = new Dictionary<GameObject, bool>();
	[HideInInspector] public Vector3 destination;
	[HideInInspector] public  Vector3 lookAtDirection;
	bool isAttacking = false;
	bool isSprinting = false;
	bool isJumping = false;
	private AIState _AIState;

	[Header("Combat")]
	public float attackRadius = 3f;
	public float prefDistFromTarget = 1f;
	public int maxEnemiesBeforeFlee = 5;
	public float healthThreshholdPercent = 0.2f;


	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		m_Molat = GetComponent<Molat>();
		agent.updateRotation = false;
		agent.updatePosition = true;
		_AIState = CreateState(StateEnum.Idle);
		currentStateEnum = StateEnum.Idle;
		destination = transform.position;
		_AIState.Initialize(this);
		//TODO on raycasting vision, add targets;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (m_Molat.IsDead)
		{
			Died(m_Molat.mostRecentAttacker);
			agent.enabled = false;
			SetTarget(null);
		}
		else
		{
			HandleRayCastVision(); //gather info around me
			if(currentStateEnum != StateEnum.Fleeing && IShouldFleeCheck())
			{
				IShouldFlee();
			}
			destination = ChooseWalkingDestination();
			if (destination == Vector3.zero)
			{
				m_Molat.targetDirection = Vector3.zero;
				agent.SetDestination(transform.position);
			}
			else
			{
				agent.SetDestination(destination);
				m_Molat.targetDirection = agent.desiredVelocity.normalized;
			}
			lookAtDirection = ChooseLookAtDirection();
			
			if(lookAtDirection == Vector3.zero)
			{
				m_Molat.lookAtDirection = m_Molat.targetDirection;
			}
			else
			{
				m_Molat.lookAtDirection = lookAtDirection;
			}
			ChooseAction();
			//wGm_Molat.TailSprint(isSprinting, m_Molat.lookAtDirection);

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
		if(instigator = currentTarget)
		{
			if (action == Action.Attack)
			{
				//dodge some times?
				//m_Molat.TailSprint(true, -m_Molat.lookAtDirection);
			}
			else if (action == Action.Die)
			{
				currentTarget = closestTarget;
			}
		}
	}

	public void SetTarget(GameObject newTarget)
	{
		this.currentTarget = newTarget;
	}

	public AIState AIState
	{
		get { return _AIState; }
		set { _AIState = value; }
	}

	private bool ObjectIsDead(GameObject newObject)
	{
		Molat thisMolat = newObject.GetComponent<Molat>();
		if (thisMolat)
		{
			return thisMolat.IsDead;
		}
		Spider thisSpider = newObject.GetComponent<Spider>();
		if (thisSpider)
		{
			return thisSpider.IsDead;
		}
		return true;
	}

	//Called from collision Capsule
	public void GameObjectInRange(GameObject newObject)
	{
		if(!_objectsInRangeDictionary.ContainsKey(newObject) && !ObjectIsDead(newObject))
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
		GameObject closestObject = null;
		RaycastHit Hit;

		List<GameObject> setToTrueList = new List<GameObject>();
		List<GameObject> setToFalseList = new List<GameObject>();
		List<GameObject> removeFromList = new List<GameObject>();
		foreach (KeyValuePair<GameObject, bool> entry in _objectsInRangeDictionary)
		{
			if (entry.Key == null || ObjectIsDead(entry.Key))
			{
				removeFromList.Add(entry.Key);
				continue;
			}

			Vector3 direction = ((entry.Key.transform.position) - lookFrom.position);
			float visionAngle = Vector3.Angle(transform.forward, direction);
			bool gotAHit = false;
			for(int i = -1; i<2; i++)
			{
				Physics.Raycast(lookFrom.position, direction + new Vector3(0,i,0), out Hit, 10000f);
				if (Hit.transform.root.gameObject == entry.Key)
				{
					gotAHit = true;
					if (shortestDistance > Hit.distance)
					{
						shortestDistance = Hit.distance;
						closestObject = entry.Key;
					}
					//raycast hit object
					if (entry.Value == false && visionAngle < maxVisionAngle)
					{
						//First time seeing target, make sure angle is acceptable
						print("Gained sightt using i = " + i);
						NewCreatureInSight(entry.Key);
						setToTrueList.Add(entry.Key);
					}
					break;
				}
			}
			if(gotAHit == false)
			{
				//Didnt see target
				if (entry.Value == true)
				{
					NewCreatureOutOfSight(entry.Key);
					setToFalseList.Add(entry.Key);
				}
			}
		}
		//After sight dictionary cleanup
		foreach (GameObject entry in setToFalseList)
		{
			_objectsInRangeDictionary[entry] = false;
		}
		foreach (GameObject entry in setToTrueList)
		{
			_objectsInRangeDictionary[entry] = true;
		}
		foreach (GameObject entry in removeFromList)
		{
			_objectsInRangeDictionary.Remove(entry);
		}
		closestTarget = closestObject;
		if (closestObject)
		{
			NewClosestObject(closestObject);
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
		if(newCreature = currentTarget)
		{
			LostSightOfTarget(newCreature);
		}
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

	//AIState Notications

	//Every Tick
	private Vector3 ChooseWalkingDestination()
	{
		return _AIState.ChooseWalkingDestination();
	}
	private Vector3 ChooseLookAtDirection()
	{
		return _AIState.ChooseLookAtDirection();
	}
	private void ChooseAction()
	{
		_AIState.ChooseAction();
	}

	//Change in vision
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

	private void LostSightOfTarget(GameObject target)
	{
		_AIState.LostSightOfTarget(target);
	}

	//Other Events
	public void GotHit(GameObject instigator)
	{
		_AIState.GotHit(instigator);
	}
	public void Died(GameObject instigator)
	{
		_AIState.Died(instigator);
	}
	private void NewClosestObject(GameObject newObject)
	{
		_AIState.NewClosestObject(newObject);
	}
	private bool IShouldFleeCheck()
	{
		return _AIState.IShouldFleeCheck();
	}
	private void IShouldFlee()
	{
		_AIState.IShouldFlee();
	}



	public void SetState(AIState newState, AIState previousState)
	{
		Destroy(AIState);
		AIState = newState;
		AIState.Initialize(previousState);
	}

	public AIState CreateState(StateEnum stateEnum)
	{
		switch(stateEnum)
		{
			case(StateEnum.Idle):
				return gameObject.AddComponent<AIStateIdle>();
			case (StateEnum.Dead):
				print("Dead");
				return gameObject.AddComponent<AIStateDead>();
			case (StateEnum.Fighting):
				print("fighting");
				return gameObject.AddComponent<AIStateFighting>();
			case (StateEnum.Fleeing):
				print("Fleeing");
				return gameObject.AddComponent<AIStateFleeing>();
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