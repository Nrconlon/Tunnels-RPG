using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;




[RequireComponent(typeof(Molat))]
public class MolatPlayerController : MonoBehaviour {

	private Molat m_molat;  // A reference to the molat on the object
	private Transform m_Cam;// A reference to the main camera in the scenes transform
	private Vector3 m_CamForward;  // The current forward direction of the camera
	private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

	// Use this for initialization
	void Start () {
		m_molat = GetComponent<Molat>();
	}


	void Update()
	{
		Vector3 forward = Camera.main.transform.forward;
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
		Vector3 right = new Vector3(forward.z, 0, -forward.x);

		Vector3 targetDirection = (hor * right) + (ver * forward);
		targetDirection = targetDirection.normalized;
		m_molat.targetDirection = targetDirection;

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
