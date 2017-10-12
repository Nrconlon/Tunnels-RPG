using System;
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
	[SerializeField] float fleeDuration = 5f;

	[SerializeField] List<Transform> waypoints = new List<Transform>();

	[SerializeField] StateEnum preferedState = StateEnum.Fighting;
	[SerializeField] Team myTeam;

	[HideInInspector] public StateEnum currentStateEnum;
	[HideInInspector] public Action currentAction;
	[HideInInspector] public Molat m_Molat;
	[HideInInspector] private GameObject currentTarget = null;
	[HideInInspector] public List<Molat> mList_EnemyMolats = new List<Molat>();
	[HideInInspector] public List<Spider> mList_EnemySpiders = new List<Spider>();

	Dictionary<GameObject, EnemyPriority> _RenderedObjectsDictionary = new Dictionary<GameObject, EnemyPriority>();
	[HideInInspector] public Vector3 destination;
	[HideInInspector] public  Vector3 lookAtDirection;
	bool isDead = false;
	private AIState _AIState;
	[Header("Combat")]
	[Range(1, 3)][SerializeField] public float skill = 2f;
	[Range(1, 5)] [SerializeField] public float courage = 1;
	[SerializeField] float fightRadius = 3f;
	[SerializeField] float prefTrgDistWep = 0.8f;
	[SerializeField] float prefTrgDistUnrm = 0.5f;
	[SerializeField] float grabItemWhileFightingDist = 5f;
	float prefTrgDistCurr = 0;
	[SerializeField] int maxEnemiesBeforeFlee = 300;




	[HideInInspector] public HealingStationController healingStationController = null;


	private void OnDestroy()
	{
		m_Molat.ActionExpressedDel -= AMolatExpressedAction;
		m_Molat.IGotHitDel -= AMolatGotHit;
		foreach (Molat molat in mList_EnemyMolats)
		{
			molat.ActionExpressedDel -= AMolatExpressedAction;
			molat.DestroyedDel -= AMolatDestroyed;
		}
	}

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		m_Molat = GetComponent<Molat>();
		agent.updateRotation = false;
		agent.updatePosition = true;
		agent.baseOffset = -0.1f;


		_AIState = CreateState(StateEnum.Idle);
		currentStateEnum = StateEnum.Idle;
		destination = transform.position;
		_AIState.Initialize(this);

		//delegate
		m_Molat.ActionExpressedDel += AMolatExpressedAction;
		m_Molat.IGotHitDel += AMolatGotHit;
		//TODO on raycasting vision, add targets;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!isDead)
		{
			if(m_Molat.IsDead)
			{
				Died(this.gameObject);
			}
			HandleRayCastVision(); //gather info around me
			HandleFindTarget();
			HandlePreferedDistance();
			if(currentStateEnum != StateEnum.Fleeing && IShouldFleeCheck())
			{
				IShouldFlee();
			}
			destination = ChooseWalkingDestination();
			if(agent && agent.isActiveAndEnabled)
			{
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
		}
	}

	private void HandlePreferedDistance()
	{
		if (agent && agent.isActiveAndEnabled)
		{
			if (m_Molat.IsWeaponEquiped && m_Molat.MyWeapon)
			{
				prefTrgDistCurr = prefTrgDistWep;
				//agent.stoppingDistance = prefTrgDistWep;
			}
			else
			{
				prefTrgDistCurr = prefTrgDistUnrm;
				//agent.stoppingDistance = prefTrgDistUnrm;
			}
		}
	}

	//Getters
	public float FleeDuration { get { return fleeDuration; } }
	public List<Transform> Waypoints { get { return waypoints; } }
	public StateEnum PreferedState { get { return preferedState; } }
	public float FightRadius { get { return fightRadius; } }
	public float PrefDistFromTarget { get { return prefTrgDistCurr; } }
	public int MaxEnemiesBeforeFlee { get { return maxEnemiesBeforeFlee; } }
	public float Courage { get { return courage; } }
	public GameObject CurrentTarget { get { return currentTarget; } }
	public float GrabItemWhileFightingDist { get { return grabItemWhileFightingDist; } }
	public float Skill { get { return skill; } }
	


	//Delegate sign ups
	//for enemies and self (and maybe allies)
	void AMolatExpressedAction(Action action, Molat instigator)
	{
		if(instigator != null)
		{
			//if its me
			if (instigator == gameObject)
			{
				if (action == Action.Die)
				{
					Died(m_Molat.mostRecentAttacker);
				}
			}
			if (action == Action.Die)
			{
				StopRenderingGameObject(instigator.gameObject);
			}
		}
	}
	//for allies and self
	void AMolatGotHit(float damage, float percentOfHealth, GameObject attacker, GameObject victim)
	{
		//if its me
		if(victim == gameObject && _RenderedObjectsDictionary.ContainsKey(attacker))
		{
			EnemyPriority currentPriority = _RenderedObjectsDictionary[attacker];
			currentPriority.TheyHitMe(percentOfHealth);
			currentPriority.ISawYou();
			GotHit(attacker);
		}
	}


	public void SetTarget(GameObject newTarget)
	{
		currentTarget = newTarget;
	}

	public AIState AIState
	{
		get { return _AIState; }
		set { _AIState = value; }
	}

	private bool IsObjectDead(GameObject newObject)
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
	public void StartRenderingGameObject(GameObject newObject)
	{
		Molat renderingMolat = newObject.GetComponent<Molat>();
		if(renderingMolat && renderingMolat.CheckIfTeamate(m_Molat.teamNumber))
		{
			return;
		}
		if (!_RenderedObjectsDictionary.ContainsKey(newObject) && !IsObjectDead(newObject))
		{
			EnemyPriority newPriority = new EnemyPriority
			{
				MaxDistance = visionRadius,
				ScareTimer = FleeDuration
			};
			if(newObject.GetComponent<Spider>())
			{
				newPriority.IsSpider();
			}
			_RenderedObjectsDictionary.Add(newObject, newPriority);
		}

	}
	public void StopRenderingGameObject(GameObject newObject)
	{
		_RenderedObjectsDictionary.Remove(newObject);

	}

	private void HandleRayCastVision()
	{
		RaycastHit Hit = new RaycastHit();
		List<GameObject> removeFromList = new List<GameObject>();
		foreach (KeyValuePair<GameObject, EnemyPriority> entry in _RenderedObjectsDictionary)
		{
			if (entry.Key == null || IsObjectDead(entry.Key))
			{
				removeFromList.Add(entry.Key);
				continue;
			}

			EnemyPriority currentPriority = entry.Value;
			Vector3 direction = ((entry.Key.transform.position) - lookFrom.position);
			float visionAngle = Vector3.Angle(transform.forward, direction);
			bool gotAHit = false;
			if (visionAngle < maxVisionAngle)
			{
				for (float i = 0; i < 1.5f; i = i + 0.5f)
				{
					Physics.Raycast(lookFrom.position, direction + new Vector3(0, i, 0), out Hit, 10000f);
					if (Hit.transform && Hit.transform.gameObject == entry.Key)
					{
						gotAHit = true;
						if(currentPriority.Vision == 0)
						{
							NewCreatureInSight(entry.Key);
						}
						currentPriority.setDistance(Hit.distance);
						currentPriority.ISawYou();
						break;
					}
				}
			}
			if(gotAHit == false)
			{
				float lastVision = currentPriority.Vision;
				currentPriority.IDidNotSeeYou(Time.deltaTime);
				if(currentPriority.Vision == 0 && lastVision > 0)
				{
					NewCreatureOutOfSight(entry.Key);
				}
			}
		}
		foreach (GameObject entry in removeFromList)
		{
			StopRenderingGameObject(entry);
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
		newMolat.ActionExpressedDel += AMolatExpressedAction;
		newMolat.DestroyedDel += AMolatDestroyed;
	}

	private void AMolatDestroyed(Molat destroyedMolat)
	{
		removeMolat(destroyedMolat);
	}

	void removeMolat(Molat newMolat)
	{
		if (mList_EnemyMolats != null)
		{
			mList_EnemyMolats.Remove(newMolat);
		}
		newMolat.ActionExpressedDel -= AMolatExpressedAction;
		newMolat.DestroyedDel -= AMolatDestroyed;
	}

	void addSpider(Spider newSpider)
	{
		mList_EnemySpiders.Add(newSpider);
	}
	void removeSpider(Spider newSpider)
	{
		if (mList_EnemySpiders != null)
		{
			mList_EnemySpiders.Remove(newSpider);
		}
	}


	private void HandleFindTarget()
	{
		float hightestPriority = 0;
		GameObject highestPriorityTarget = null;
		foreach (KeyValuePair<GameObject, EnemyPriority> entry in _RenderedObjectsDictionary)
		{
			float currentPriority = entry.Value.getTotalPriority();
			if(currentPriority > hightestPriority)
			{
				hightestPriority = currentPriority;
				highestPriorityTarget = entry.Key;
			}
		}
		//if we didint find anything, and we werent already null
		if (hightestPriority == 0)
		{
			if (CurrentTarget != null)
			{
				SetTarget(null);
				NoTargetAvailable();
			}
		}
		else
		{
			//Make sure target is different
			if(highestPriorityTarget != currentTarget)
			{
				GameObject oldTarget = CurrentTarget;
				SetTarget(highestPriorityTarget);
				//set target before notifying the states jsut in case
				if (oldTarget == null)
				{
					FirstTargetAquired(CurrentTarget);
				}
				else
				{
					NewTargetAquired(CurrentTarget);
				}
			}			
		}
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
	private void NoTargetAvailable()
	{
		_AIState.NoTargetAvailable();
	}
	private void NewTargetAquired(GameObject newTarget)
	{
		_AIState.NewTargetAquired(newTarget);
	}
	private void FirstTargetAquired(GameObject newTarget)
	{
		_AIState.FirstTargetAquired(newTarget);
	}
	
	//Other Events
	public void GotHit(GameObject instigator)
	{
		_AIState.GotHit(instigator);
	}
	public void Died(GameObject instigator)
	{
		isDead = true;
		agent.enabled = false;
		SetTarget(null);
		_AIState.Died(instigator);
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
				return gameObject.AddComponent<AIStateDead>();
			case (StateEnum.Fighting):
				return gameObject.AddComponent<AIStateFighting>();
			case (StateEnum.Fleeing):
				return gameObject.AddComponent<AIStateFleeing>();
		}
		return gameObject.AddComponent<AIStateIdle>();
	}


	private void OnDrawGizmos()
	{
		//Draw attack speher
		Gizmos.color = new Color(255f, 0f, 0, .5f);
		Gizmos.DrawWireSphere(transform.position, fightRadius);

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
}

public enum Team
{
	Bad,
	Good,
	NeutralBad,
	NeutralGood
}