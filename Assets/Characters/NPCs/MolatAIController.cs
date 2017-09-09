using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Molat))]
public class MolatAIController : MonoBehaviour {
	[Header("NavMesh Settings / Target")]
	[SerializeField] private NavMeshAgent m_NavMeshAgent;
	[SerializeField] private GameObject m_Target;

	[Header("Animation Settings")]
	[SerializeField] private Animator m_Animator;
	[SerializeField] private CurrentState m_CurrentState;

	Molat molat;


	// Use this for initialization
	void Start () {
		molat = GetComponent<Molat>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(m_Target)
		{
			m_NavMeshAgent.SetDestination(m_Target.transform.position);
		}

	}

	enum CurrentState
	{
		Idle,
		Walking,
		Running
	}
	
}
