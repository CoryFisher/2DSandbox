using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CollisionEvent))]
public class Projectile : MonoBehaviour
{
	[System.Serializable]
	public class ProjectileProperties
	{
		public Color color = Color.white;

		public bool expiresOnDistance = true;
		public float distanceMax = 10.0f;

		public bool expiresOnLifetime = true;
		public float lifetime = 10.0f;

		public float reloadTime = 1.0f;

		public float speed = 1.0f;
		public float damage = 1.0f;
	}
	public ProjectileProperties projectile;

	// protected
	protected Rigidbody2D rb;

	// private
	private Vector3 distanceStartPoint;
	private float lifeTimer;
	private CollisionEvent collisionEvent;

	private void Awake()
	{
		collisionEvent = GetComponent<CollisionEvent>();
		collisionEvent.OnCollision += OnCollision;
		rb = GetComponent<Rigidbody2D>();
	}
	
	public virtual void Fire(Vector3 position, Vector3 target)
	{
		rb.velocity = (target - position).normalized * projectile.speed;
		lifeTimer = 0.0f;
		distanceStartPoint = position;
	}

	private void Update()
	{
		if (projectile.expiresOnLifetime)
		{
			lifeTimer += Time.deltaTime;
			if (lifeTimer > projectile.lifetime)
			{
				Expire();
				return;
			}
		}

		if (projectile.expiresOnDistance)
		{
			var distSqr = (distanceStartPoint - transform.position).sqrMagnitude;
			if (distSqr > projectile.distanceMax * projectile.distanceMax)
			{
				Expire();
				return;
			}
		}
	}
	
	protected void OnCollision(GameObject collidingObject)
	{
		if (collidingObject.CompareTag("Enemy"))
		{
			Enemy enemy = collidingObject.GetComponent<Enemy>();
			if (enemy != null)
			{
				enemy.TakeDamage(projectile.damage);
			}
			Expire();
		}
		else if (!collidingObject.CompareTag("PlayerProjectile") &&
				 !collidingObject.CompareTag("Player"))
		{
			Expire();
		}
	}

	protected void Expire()
	{
		gameObject.SetActive(false);
	}
}
