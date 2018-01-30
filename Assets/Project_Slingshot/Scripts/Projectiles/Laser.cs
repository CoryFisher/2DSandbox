using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Projectile
{
	private bool firing;

	public float damagePerSecond = 1.0f;
	public float distance = 10.0f;

	protected override void OnInitialize()
	{
		firing = false;
	}

	protected override void OnFireStart(Vector3 position, Vector3 target)
	{
		var direction = target - position;
		FireRay(position, direction);
		firing = true;
	}

	protected override void OnFireEnd()
	{
		firing = false;
	}

	protected override void OnUpdate()
	{
		if (firing)
		{
			var position = PlayerManager.Get().GetPlayerPosition();
			Vector3 target = DragManager.GetMouseWorldPosition();
			var direction = target - position;
			FireRay(position, direction);
		}
	}

	protected override void OnCollision(GameObject collidingObject)
	{
		if (collidingObject.CompareTag("Enemy"))
		{
			Expire();
		}
	}

	private void FireRay(Vector3 position, Vector3 direction)
	{
		var hit = Physics2D.Raycast(position, direction, distance);
		if (hit.transform.CompareTag("Enemy"))
		{
			Enemy enemy = hit.transform.GetComponent<Enemy>();
			if (enemy != null)
			{
				enemy.TakeDamage(damagePerSecond * Time.deltaTime);
			}
		}
	}
}
