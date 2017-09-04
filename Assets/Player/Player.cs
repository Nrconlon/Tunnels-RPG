using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

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
