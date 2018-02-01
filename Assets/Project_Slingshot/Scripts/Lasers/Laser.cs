using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILaser
{
	bool CanFire();
	void StartFire(Vector3 position, Vector3 target);
	void Fire(Vector3 position, Vector3 target);
	void EndFire();
	Color GetColor();
}

public class Laser : MonoBehaviour, ILaser
{
	[System.Serializable]
	public class LaserProperties
	{
		public Color color = Color.red;
		public float damagePerSecond = 1.0f;
		public float distance = 10.0f;
		public GameObject impactOverlay;
	}
	public LaserProperties laser;


	private void Awake()
	{
		laser.impactOverlay.SetActive(false);
	}

	public bool CanFire()
	{
		return true;
	}

	public void StartFire(Vector3 position, Vector3 target)
	{
		FireRay(position, target);
	}

	public void Fire(Vector3 position, Vector3 target)
	{
		FireRay(position, target);
	}

	public void EndFire()
	{
	}
	
	public Color GetColor()
	{
		return laser.color;
	}
	
	private void FireRay(Vector3 position, Vector3 target)
	{
		var hit = Physics2D.Raycast(position, target - position, laser.distance);
		if (hit)
		{
			laser.impactOverlay.transform.position = hit.point;
			laser.impactOverlay.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, hit.normal));
			laser.impactOverlay.SetActive(true);

			if (hit.transform.CompareTag("Enemy"))
			{
				Enemy enemy = hit.transform.GetComponent<Enemy>();
				if (enemy != null)
				{
					enemy.TakeDamage(laser.damagePerSecond * Time.deltaTime);
				}
			}
		}
		else
		{
			laser.impactOverlay.SetActive(false);
		}
	}
}
