using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Enemy : MonoBehaviour {

	[SerializeField]  float maxHealthPoints = 100f;

	float currentHealthPoints = 1;

	private void Start()
	{
		currentHealthPoints = maxHealthPoints;
	}
	public float healthAsPercentage
	{
		get
		{
			return currentHealthPoints / maxHealthPoints;
		}

	}

}
