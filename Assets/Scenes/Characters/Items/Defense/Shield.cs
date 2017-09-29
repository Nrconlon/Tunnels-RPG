using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("RPG/Shield"))]
public class Shield : ScriptableObject {

	[SerializeField] GameObject shieldPrefab;
	[SerializeField] AnimationClip blockAnimation;
	[SerializeField] float percentBlocked;
	[SerializeField] float staminaCost;
	[SerializeField] float durability;
}
