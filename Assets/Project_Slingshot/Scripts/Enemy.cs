using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public float maxHealth = 10.0f;
	public float health = 10.0f;

	public void TakeDamage(float damage)
	{
		health -= damage;
		if (health < 0.0f)
		{
			Die();
		}
	}

	private void Die()
	{
		Destroy(gameObject);
	}
}
