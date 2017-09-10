using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Enemy : MonoBehaviour {

	[SerializeField]  float maxHealthPoints = 100f;
	[SerializeField] float attackRadius = 4f;

	float currentHealthPoints = 1;
	AICharacterControl aiCharacterControl = null;
	GameObject player = null;

	private void Start()
	{
		currentHealthPoints = maxHealthPoints;
		aiCharacterControl = GetComponent<AICharacterControl>();
		player = GameObject.FindGameObjectWithTag("Player");
	}


	private void Update()
	{
		float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
		if (distanceToPlayer <= attackRadius)
		{
			aiCharacterControl.SetTarget(player.transform);
		}
		else
		{
			aiCharacterControl.SetTarget(transform);
		}
	}

	public float healthAsPercentage
	{
		get
		{
			return currentHealthPoints / maxHealthPoints;
		}

	}
}
