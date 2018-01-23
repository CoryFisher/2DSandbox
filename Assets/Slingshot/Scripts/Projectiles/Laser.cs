using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Projectile
{
	[System.Serializable]
	public class Data
	{
		float damage;
		float rechargeTime;
	}
	private Data data;

	public override void Initialize(int level)
	{
		data = DataManager.Get().GetProjectileData(typeof(Laser), level) as Data;
	}

	public override void Fire(Vector3 direction)
	{
		var rb = GetComponent<Rigidbody2D>();
		rb.velocity = direction.normalized * data.speed;
	}

	public override bool OnCollision(GameObject collidingObject)
	{
		if (collidingObject.CompareTag("Enemy"))
		{
			return true;
		}
		return false;
	}

	public override void OnUpdate()
	{

	}
}
