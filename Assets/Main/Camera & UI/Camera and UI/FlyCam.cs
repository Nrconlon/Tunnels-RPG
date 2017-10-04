using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour {

	[SerializeField] float mainSpeed = 2000; //regular speed
	[SerializeField] float shiftAdd = 2000; //multiplied by how long shift is held.  Basically running
	[SerializeField] float maxVelocityChange = 10.0f;
	[SerializeField] float mouseSensitivity = 300f;
	[SerializeField] float mouseScrollHeight = 15;
	[SerializeField] float minCameraHeight = 1;
	[SerializeField] float maxCameraHeight = 16;

	private float x = 0.0f;
	private float y = 0.0f;
	float currentSpeed;
	Rigidbody m_RigidBody;

	private void Start()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		m_RigidBody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		currentSpeed = mainSpeed;

		//Keyboard commands

		if (Input.GetMouseButton(1))
		{
			HandleChangeRotation();
		}

		HandleMouseWheelZoom();

		if (Input.GetKey(KeyCode.LeftShift))
		{
			currentSpeed = currentSpeed + shiftAdd;
		}
		currentSpeed = currentSpeed * 0.02f;

		Vector3 direction = transform.rotation * Vector3.forward;
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
		Vector3 right = new Vector3(direction.z, 0, -direction.x);

		Vector3 targetDirection = (hor * right) + (ver * direction);
		targetDirection = targetDirection.normalized;

		Vector3 targetVelocity = targetDirection;
		targetVelocity *= currentSpeed;

		Vector3 velocityChange = (targetVelocity - m_RigidBody.velocity);
		velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
		velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
		velocityChange.y = 0;
		m_RigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	void Update()
	{
		

	}

	private void HandleChangeRotation()
	{
		x += Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
		y -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
		Quaternion rotation = Quaternion.Euler(y, x, 0);
		transform.rotation = rotation;
	}

	private void HandleMouseWheelZoom()
	{
		float changeY = Input.GetAxis("Mouse ScrollWheel");
		if (changeY != 0)
		{
			changeY = -changeY * mouseScrollHeight;
			Vector3 lastTransform = transform.position;
			transform.position = new Vector3(lastTransform.x, Mathf.Clamp(lastTransform.y + changeY, minCameraHeight, maxCameraHeight), lastTransform.z);
		}
	}
}
