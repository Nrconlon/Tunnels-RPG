using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	GameObject player;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");  //Player is the tag we gave to player in unity
	}
	
	// LateUpdate is called once per frame, after everything else is calculated (including character position)
	void LateUpdate () {
		transform.position = player.transform.position;
	}
}
