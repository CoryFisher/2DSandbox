using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	private bool expiresOnDistance;
	private Vector3 expiryDistancePoint;
	private float maxDistance;
	
	private bool expiresOnLifetime;
	private float expiryTime;

	private IProjectileBehavior projectileBehavior;
	
	public void ExpireByDistance(Vector3 point, float dist)
	{
		expiresOnDistance = true;
		expiryDistancePoint = point;
		maxDistance = dist;
	}

	public void ExpireByLifetime(float time)
	{
		expiresOnLifetime = true;
		expiryTime = Time.time + time;
	}

	public void Fire(Vector3 direction)
	{
		if (projectileBehavior != null)
		{
			projectileBehavior.Fire(direction);
		}
	}

	public void SetBehavior(IProjectileBehavior behavior)
	{
		projectileBehavior = behavior;
	}

	protected void Expire()
	{
		// remove projectile behavior
		if (projectileBehavior != null)
		{
			Component behavior = GetComponent(projectileBehavior.GetType());
			Destroy(behavior);
			behavior = null;
		}

		// deactivate
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (expiresOnDistance)
		{
			var distSqr = (expiryDistancePoint - transform.position).sqrMagnitude;
			if (distSqr > maxDistance * maxDistance)
			{
				Expire();
			}
		}

		if (expiresOnLifetime)
		{
			if (Time.time > expiryTime)
			{
				Expire();
			}
		}

		if (projectileBehavior != null)
		{
			projectileBehavior.OnUpdate();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		OnCollision(collision.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		OnCollision(collision.gameObject);
	}

	private void OnCollision(GameObject collidingObject)
	{
		if (projectileBehavior != null)
		{
			if (projectileBehavior.OnCollision(collidingObject))
			{
				Expire();
			}
		}
	}
}
