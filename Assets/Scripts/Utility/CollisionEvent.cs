using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
	public delegate void CollisionEventHandler(GameObject obj);
	public CollisionEventHandler OnCollision;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (OnCollision != null)
		{
			OnCollision.Invoke(collision.gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (OnCollision != null)
		{
			OnCollision.Invoke(collision.gameObject);
		}
	}

	private void OnDestroy()
	{
		OnCollision = null;
	}
}
