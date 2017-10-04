using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPriority {
	private float _vision = 0;
	private float _distance = 0;
	private float _hitMe = 0;
	private float _hitMyFriend = 0;
	private float _spider = 0;
	private float _taunted = 0;

	private float scaredTimer = 5;
	private float maxDistance = 10;

	private const float SPIDER_PRIORITY = 10000;
	private const float DISTANCE_MULTIPLIER = 6;
	private const float HIT_FRIEND_MULTIPLIER = 0.2f;
	private const float DAMAGE_VALUE_MULTIPLIER = 100f;


	public void TheyHitMe(float percentDamage)
	{
		_hitMe += percentDamage * DAMAGE_VALUE_MULTIPLIER;
	}
	public void TheyHitMyFriend(float percentDamage)
	{
		_hitMyFriend += percentDamage * HIT_FRIEND_MULTIPLIER * DAMAGE_VALUE_MULTIPLIER;
	}

	public float ScareTimer { set { scaredTimer = value; } }
	public float MaxDistance { set { maxDistance = value; } }
	public float Vision { get { return _vision; } }

	public void ISawYou()
	{
		_vision = 1;
	}
	public void IDidNotSeeYou(float deltaTime)
	{
		_vision = Mathf.Clamp(_vision - ((1 / scaredTimer) * deltaTime), 0, 1);
	}

	public void IsSpider()
	{
		_spider = SPIDER_PRIORITY;
	}

	public void setDistance(float distance)
	{
		_distance = Mathf.Clamp(maxDistance - distance, 0, maxDistance) * DISTANCE_MULTIPLIER;
	}




	public float getTotalPriority()
	{
		return _vision * (_hitMe + _hitMyFriend + _spider + _distance + _taunted);
	}

}
