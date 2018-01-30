using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public bool expiresOnDistance = true;
	public float distanceMax = 10.0f;
	private Vector3 distanceStartPoint;

	public bool expiresOnLifetime = true;
	public float lifetime = 10.0f;
	private float lifeTimer;

	public bool usesReloadTime = true;
	public float reloadTime = 1.0f;
	private float reloadTimer;
	private bool firing;

	#region Private

	private void Update()
	{
		if (usesReloadTime)
		{
			reloadTimer += Time.deltaTime;
		}

		if (expiresOnLifetime)
		{
			lifeTimer += Time.deltaTime;
			if (lifeTimer > lifetime)
			{
				Expire();
				return;
			}
		}

		if (expiresOnDistance)
		{
			var distSqr = (distanceStartPoint - transform.position).sqrMagnitude;
			if (distSqr > distanceMax * distanceMax)
			{
				Expire();
				return;
			}
		}

		OnUpdate();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		OnCollision(collision.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		OnCollision(collision.gameObject);
	}

	#endregion

	#region Protected

	protected void Expire()
	{
		OnExpire();
		gameObject.SetActive(false);
	}

	#endregion

	#region Public

	public void Initialize()
	{
		lifeTimer = 0.0f;
		reloadTimer = 0.0f;
		firing = false;

		OnInitialize();
	}

	public void FireStart(Vector3 position, Vector3 target)
	{
		if (usesReloadTime)
		{
			if (reloadTimer > reloadTime)
			{
				OnFireStart(position, target);
				reloadTimer = 0.0f;
				firing = true;
			}
		}
		else
		{
			OnFireStart(position, target);
			firing = true;
		}
	}

	public void FireEnd()
	{
		if (firing)
		{
			OnFireEnd();
			firing = false;
		}
	}

	#endregion

	#region Virtual
	
	protected virtual void OnInitialize()
	{
	}
	
	protected virtual void OnFireStart(Vector3 position, Vector3 target)
	{
	}

	protected virtual void OnFireEnd()
	{
	}
	
	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnCollision(GameObject collidingObject)
	{
	}

	protected virtual void OnExpire()
	{
	}

	#endregion
}
