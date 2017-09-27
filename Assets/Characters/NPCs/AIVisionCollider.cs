using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIVisionCollider : MonoBehaviour {
	MolatAIController m_MolatAIController;
	// Use this for initialization
	void Start() {
		m_MolatAIController = GetComponentInParent<MolatAIController>();
	}

	// Update is called once per frame
	void Update() {

	}

	void OnTriggerEnter(Collider collider)
	{
		m_MolatAIController.StartRenderingGameObject(collider.transform.root.gameObject);
	}

	void OnTriggerExit(Collider collider)
	{
		m_MolatAIController.StopRenderingGameObject(collider.transform.root.gameObject);
	}
}

