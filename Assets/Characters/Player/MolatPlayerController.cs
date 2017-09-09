using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[RequireComponent(typeof(Molat))]
public class MolatPlayerController : MonoBehaviour {

	Molat m_molat;

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
