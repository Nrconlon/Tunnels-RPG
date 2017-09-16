using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Molat))]
public class MolatAIController : MonoBehaviour {
	[Header("NavMesh Settings / Target")]
	 NavMeshAgent agent;
	public Transform target;
	[SerializeField] float attackRadius = 3f;
	[SerializeField] float chaseRadius = 10f;

	[Header("Animation Settings")]
	[SerializeField] private CurrentState m_CurrentState;

	Molat m_Molat;
	List<Molat> mList_EnemyMolats = new List<Molat>();
	bool isAttacking = false;


	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		m_Molat = GetComponent<Molat>();
		target = GameObject.FindGameObjectWithTag("Player").transform;
		agent.updateRotation = false;
		agent.updatePosition = true;
		addTarget(target.gameObject);
		//TODO on raycasting vision, add targets;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(m_Molat.IsDead)
		{
			agent.enabled = false;
			SetTarget(transform);
		}
		else
		{

			if (target != null && target.position != Vector3.zero)
			{
				// && agent.remainingDistance > agent.stoppingDistance

				float distanceToTarget = Vector3.Distance(target.position, transform.position);

				if (distanceToTarget <= attackRadius)
				{
					isAttacking = true;

				}
				else
				{
					isAttacking = false;
				}

				if (distanceToTarget <= chaseRadius)
				{
					agent.SetDestination(target.position);
					m_Molat.targetDirection = agent.desiredVelocity.normalized;
					m_Molat.lookAtDirection = (target.position - transform.position).normalized;
				}
				else
				{
					agent.SetDestination(transform.position);
					m_Molat.targetDirection = Vector3.zero;
					m_Molat.lookAtDirection = Vector3.zero;
				}
			}
			else
			{
				m_Molat.targetDirection = Vector3.zero;
				m_Molat.lookAtDirection = Vector3.zero;
			}

			if (!m_Molat.isWeaponEquiped)
			{
				m_Molat.ToggleEquipWeapon();
			}

			m_Molat.Attack(isAttacking);
		}
	}

	void addTarget(GameObject incomingTarget)
	{
		Molat targetMolat = incomingTarget.GetComponent<Molat>();
		if(targetMolat)
		{
			mList_EnemyMolats.Add(targetMolat);
			targetMolat.actionExpressed += expressAction;
		}
		//TODO check if spider, add to spider list
		
	}
	void removeTarget(GameObject incomingTarget)
	{
		Molat targetMolat = incomingTarget.GetComponent<Molat>();
		if(mList_EnemyMolats != null)
		{
			mList_EnemyMolats.Remove(targetMolat);
		}
		targetMolat.actionExpressed -= expressAction;
	}

	void expressAction(Action action, GameObject instigator)
	{
		if (action == Action.Attack)
		{
			m_Molat.Jump(true, -m_Molat.lookAtDirection);
		}
	}

	public void SetTarget(Transform target)
	{
		this.target = target;
	}


	enum CurrentState
	{
		Idle,
		Walking,
		Running
	}

	

	private void OnDrawGizmos()
	{
		//Draw attack speher
		Gizmos.color = new Color(255f, 0f, 0, .5f);
		Gizmos.DrawWireSphere(transform.position, attackRadius);

		//Draw chase speher
		Gizmos.color = new Color(0f, 0f, 255f, .5f);
		Gizmos.DrawWireSphere(transform.position, chaseRadius);
	}



}
