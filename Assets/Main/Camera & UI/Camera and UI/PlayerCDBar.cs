using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCDBar : MonoBehaviour
{

	public Molat player;
	RectTransform rectTransform;

	// Use this for initialization
	void Start()
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		if (playerObject)
		{
			player = playerObject.GetComponent<Molat>();
		}
		rectTransform = GetComponent<RectTransform>();
	}

	// Update is called once per frame
	void Update()
	{
		if (player)
		{
			rectTransform.localScale = new Vector3(player.CurrentCooldown, 1, 1);

		}
		else
		{
			rectTransform.localScale = Vector3.zero;
		}

	}
}
