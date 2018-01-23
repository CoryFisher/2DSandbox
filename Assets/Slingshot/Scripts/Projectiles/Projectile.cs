using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public class Data
	{
		public bool expiresOnDistance;
		public Vector3 expiryDistanceStartPoint;
		public float maxDistance;

		public bool expiresOnLifetime;
		public float lifetime;
	}
	protected Data data;
	protected Rigidbody2D rb;
	
	// VIRTUALS

	public virtual void Initialize(int level)
	{
		data = DataManager.Get().GetProjectileData(this.GetType(), level) as Data;
	}
	
	public virtual void Fire(Vector3 direction)
	{
		if (data.expiresOnDistance)
		{
			data.expiryDistanceStartPoint = 
		}

		if (data.expiresOnLifetime) 
		{

		}
	}

	protected virtual void OnUpdate()
	{
		if (data.expiresOnDistance)
		{
			var distSqr = (data.expiryDistanceStartPoint - transform.position).sqrMagnitude;
			if (distSqr > data.maxDistance * data.maxDistance)
			{
				Expire();
			}
		}

		if (data.expiresOnLifetime)
		{
			if (Time.time > data.lifetime)
			{
				Expire();
			}
		}
	}

	protected virtual void OnCollision(GameObject collidingObject)
	{
		Expire();
	}

	protected virtual void Expire()
	{
		rb.velocity = Vector3.zero;
		gameObject.SetActive(false);
	}

	// COLLISION TRIGGERS
	void OnCollisionEnter2D(Collision2D collision)
	{
		OnCollision(collision.gameObject);
	}
	void OnTriggerEnter2D(Collider2D collision)
	{
		OnCollision(collision.gameObject);
	}

	// UPDATE TRIGGER
	private void Update()
	{
		OnUpdate();
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}
}
