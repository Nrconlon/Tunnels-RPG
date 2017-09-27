using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserArenaController : MonoBehaviour {

	[SerializeField] GameObject AIPrefab;

	// Use this for initialization
	void Start () {
		CameraRaycaster cameraRaycaster = GetComponent<CameraRaycaster>();
		cameraRaycaster.NotifyMouseClickObservers += MouseHasBeenClicked;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void MouseHasBeenClicked(RaycastHit raycastHit, int layerHit)
	{
		print("here 3");
		if(layerHit == 0)
		{
			Instantiate(AIPrefab, raycastHit.point, Quaternion.LookRotation(Vector3.zero,Vector3.up));
			print("here 4");
		}
	}
}
