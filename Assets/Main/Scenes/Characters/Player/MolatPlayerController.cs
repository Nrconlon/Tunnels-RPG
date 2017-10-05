using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;




[RequireComponent(typeof(Molat))]
public class MolatPlayerController : MonoBehaviour {

	private Molat m_molat;  // A reference to the molat on the object
	private Vector3 m_CamForward;  // The current forward direction of the camera
	private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
	private CameraPlayer myCamera;
	bool etoggle = false;

	// Use this for initialization
	void Start () {
		m_molat = GetComponent<Molat>();
		myCamera = GetComponentInChildren<CameraPlayer>();
		Cursor.lockState = CursorLockMode.Locked;
	}


	void Update()
	{
		Vector3 m_CamForward = myCamera.direction;
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
		Vector3 right = new Vector3(m_CamForward.z, 0, -m_CamForward.x);

		Vector3 targetDirection = (hor * right) + (ver * m_CamForward);
		targetDirection = targetDirection.normalized;
		m_molat.targetDirection = targetDirection;
		m_molat.lookAtDirection = m_CamForward;

		m_molat.TailSprint(Input.GetButton("Fire3"), targetDirection);
		m_molat.Jump(Input.GetButton("Jump"), targetDirection);
		if(Input.GetMouseButtonDown(0))
		{
			m_molat.Attack();
		}
		if(Input.GetMouseButtonDown(1))
		{
			m_molat.Block(true);
		}
		else if(Input.GetMouseButtonUp(1))
		{
			m_molat.Block(false);
		}

		if (Input.GetKey("q"))
		{
			m_molat.ThrowWeapon();
		}

		if (Input.GetKey("f"))
		{
			m_molat.ToggleEquipWeapon();
		}
		if (Input.GetKey("e"))
		{
			if (!etoggle)
			{
				etoggle = true;
				m_molat.GrabNearestItem();
			}
		}
		else
		{
			etoggle = false;
		} 
			



	}
}
