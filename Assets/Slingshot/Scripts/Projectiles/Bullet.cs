using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IProjectileBehavior
{
	[System.Serializable]
	public class Data
	{
		public float speed;
	}
	private Data data;

	public void SetLevel(int level)
	{
		data = DataManager.Get().GetProjectileData(typeof(Bullet), level) as Data;
	}

	public void Fire(Vector3 direction)
	{
		var rb = GetComponent<Rigidbody2D>();
		rb.velocity = direction.normalized * data.speed;
	}

	public bool OnCollision(GameObject collidingObject)
	{
		if (collidingObject.CompareTag("Enemy"))
		{
			return true;
		}
		return false;
	}

	public void OnUpdate()
	{

	}
}
