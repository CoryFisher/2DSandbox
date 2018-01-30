using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
	private Rigidbody2D rb;
	public float speed;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	protected override void OnInitialize()
	{
		rb.velocity = Vector2.zero;
	}

	protected override void OnFireStart(Vector3 position, Vector3 target)
	{
		rb.velocity = (target - position).normalized * speed;
	}

	protected override void OnCollision(GameObject collidingObject)
	{
		if (collidingObject.CompareTag("Enemy"))
		{
			Expire();
		}
	}
}
