using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayer : MonoBehaviour {

	[SerializeField] Transform target;
	[SerializeField] Vector3 targetOffset = Vector3.zero;
	[SerializeField] float distance = 4.0f;



	[SerializeField] LayerMask lineOfSightMask = 0;
	[SerializeField] float closerRadius = 0.2f;
	[SerializeField] float closerSnapLag = 0.2f;

	[SerializeField] float xSpeed = 200.0f;
	[SerializeField] float ySpeed = 80.0f;

	[SerializeField] float yMinLimit = -20;
	[SerializeField] float yMaxLimit = 80;

	[HideInInspector] public Vector3 direction = Vector3.zero;

	private Transform WhereCamShouldBe;

	private MolatPlayerController _MolatPlayerController;

	private float currentDistance = 10.0f;
	private float x = 0.0f;
	private float y = 0.0f;
	private float distanceVelocity = 0.0f;

	void Start()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		currentDistance = distance;
		Cursor.lockState = CursorLockMode.Locked;
		_MolatPlayerController = GetComponentInParent<MolatPlayerController>();

		// Make the rigid body not change rotation
		if (GetComponentInParent<Rigidbody>())
			GetComponentInParent<Rigidbody>().freezeRotation = true;
	}

	void LateUpdate()
	{
		if (target)
		{
			x += Input.GetAxisRaw("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxisRaw("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 targetPos = target.position + targetOffset;
			direction = rotation * Vector3.forward;

			float targetDistance = AdjustLineOfSight(targetPos, direction);
			currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, closerSnapLag * .3f);

			transform.rotation = rotation;
			//transform.position = targetPos + direction * currentDistance;
			//_MolatPlayerController.myCamera = WhereCamShouldBe;
		}
	}

	float AdjustLineOfSight(Vector3 target, Vector3 direction)
	{
		RaycastHit hit;
		if (Physics.Raycast(target, direction, out hit, distance, lineOfSightMask.value))
			return hit.distance - closerRadius;
		else
			return distance;
	}

	static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp(angle, min, max);
	}
}
