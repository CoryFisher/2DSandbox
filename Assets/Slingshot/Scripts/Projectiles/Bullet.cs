using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
	[System.Serializable]
	public class BulletData : Data
	{
		public float speed;
		public float rechargeTime;
	}
	private BulletData bulletData;
	private float timer = 0.0f;

	public override void Initialize(int level)
	{
		data = DataManager.Get().GetProjectileData(typeof(BulletData), level) as Data;
	}

	public override void Fire(Vector3 direction)
	{
		if (timer > data.rechargeTime)
		{
			var rb = GetComponent<Rigidbody2D>();
			rb.velocity = direction.normalized * data.speed;
		}
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
		
		timer += Time.deltaTime;
	}
}
