using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("RPG/Armor"))]
public class Armor : ScriptableObject
{

	[SerializeField] GameObject armorPrefab;
	[SerializeField] float percentBlocked;
	[SerializeField] float durability;
}
