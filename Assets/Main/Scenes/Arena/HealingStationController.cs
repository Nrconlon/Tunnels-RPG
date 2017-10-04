using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingStationController : MonoBehaviour {
	[SerializeField] GameObject healingStationPrefab;
	List<HealingStation> healingStations = new List<HealingStation>();
	public void SpawnHealingStation(Vector3 location)
	{
		location = new Vector3(location.x, location.y + 0.1f, location.z);
		GameObject clone = Instantiate(healingStationPrefab, location, Quaternion.Euler(90, 0, 0));
		HealingStation station = clone.GetComponent<HealingStation>();
		station.DestroyMeDel += DeleteStation;
		healingStations.Add(station);
	}

	private void DeleteStation(HealingStation station)
	{
		healingStations.Remove(station);
		Destroy(station.gameObject);
	}

	public HealingStation GetClosestStation(Vector3 closestToPos)
	{
		float closestDistance = float.PositiveInfinity;
		HealingStation closestStation = null;

		foreach (HealingStation station in healingStations)
		{
			float distance = Vector3.Distance(closestToPos, station.transform.position);
			if(distance < closestDistance)
			{
				closestDistance = distance;
				closestStation = station;
			}
		}

		return closestStation;
	}
}
