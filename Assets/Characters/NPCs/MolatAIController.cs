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
	[SerializeField] float chaseRadius = 6f;

	[Header("Animation Settings")]
	[SerializeField] private CurrentState m_CurrentState;

	Molat molat;


	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		molat = GetComponent<Molat>();
		target = GameObject.FindGameObjectWithTag("Player").transform;
		agent.updateRotation = false;
		agent.updatePosition = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (target != null && target.position != Vector3.zero)
		{
			// && agent.remainingDistance > agent.stoppingDistance
			
			float distanceToTarget = Vector3.Distance(target.position, transform.position);
			if (distanceToTarget <= attackRadius)
			{
				agent.SetDestination(target.position);
				molat.targetDirection = agent.desiredVelocity.normalized;
				molat.lookAtDirection = agent.desiredVelocity.normalized;
			}
			else
			{
				agent.SetDestination(transform.position);
				molat.targetDirection = Vector3.zero;
				molat.lookAtDirection = Vector3.zero;
			}
		}
		else
		{
			molat.targetDirection = Vector3.zero;
			molat.lookAtDirection = Vector3.zero;
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
	
}
