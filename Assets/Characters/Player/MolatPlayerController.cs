﻿using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;




[RequireComponent(typeof(Molat))]
public class MolatPlayerController : MonoBehaviour {

	private Molat m_molat;  // A reference to the molat on the object
	private Vector3 m_CamForward;  // The current forward direction of the camera
	private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

	// Use this for initialization
	void Start () {
		m_molat = GetComponent<Molat>();
	}


	void Update()
	{
		Vector3 m_CamForward = Camera.main.transform.forward;
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
		Vector3 right = new Vector3(m_CamForward.z, 0, -m_CamForward.x);

		Vector3 targetDirection = (hor * right) + (ver * m_CamForward);
		targetDirection = targetDirection.normalized;
		m_molat.targetDirection = targetDirection;
		m_molat.lookAtDirection = m_CamForward;

		m_molat.TailSprint(Input.GetButton("Fire3"), targetDirection);
		m_molat.Jump(Input.GetButton("Jump"), targetDirection);

		if (Input.GetButton("Fire3"))
		{
			m_molat.ToggleEquipWeapon();
		}

		if (Input.GetButton("Mouse X"))
		{
			m_molat.Attack();
		}

	}
}
