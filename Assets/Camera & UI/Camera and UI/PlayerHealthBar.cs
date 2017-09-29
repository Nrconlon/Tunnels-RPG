using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class PlayerHealthBar : MonoBehaviour
{

    RawImage healthBarRawImage;
    Molat player;

    // Use this for initialization
    void Start()
    {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Molat>();
        healthBarRawImage = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
		if (player)
		{
			float xValue = -(player.HealthAsPercentage / 2f) - 0.5f;
			healthBarRawImage.uvRect = new Rect(xValue, 0f, 0.5f, 1f);

		}

	}
}
