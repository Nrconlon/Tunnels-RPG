using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class PlayerStaminaBar : MonoBehaviour
{

	RawImage staminaBarRawImage;
	Molat player;

	// Use this for initialization
	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Molat>();
		staminaBarRawImage = GetComponent<RawImage>();
	}

	// Update is called once per frame
	void Update()
	{
		if (player)
		{
			float xValue = -(player.staminaAsPercentage / 2f) - 0.5f;
			staminaBarRawImage.uvRect = new Rect(xValue, 0f, 0.5f, 1f);

		}

	}
}
