using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
	Projectile,
	//Bullet,
	Count
}

// holds a list of ProjectileControllers and notifies the current selected controller about input events

public class ProjectileController : MonoBehaviour
{
	ObjectPool<GameObject>[] projectilePools;
	int current;
	bool firing;
	float reloadTimer;
	float currentReloadTime;
	Func<GameObject, bool> nextInactiveFunc = x => x.activeSelf == false;
	Func<GameObject, bool> nextAnyFunc = x => x != null;

	public GameObject projectilePrefab;
	//public GameObject bulletPrefab;

	private void Awake()
	{
		projectilePools = new ObjectPool<GameObject>[(int)ProjectileType.Count];
		
		projectilePools[(int)ProjectileType.Projectile] = new ObjectPool<GameObject>(
		5, 
		() => {
			var obj = Instantiate(projectilePrefab);
			obj.SetActive(false);
			return obj;
		});

		currentReloadTime = projectilePools[current].GetNext(nextAnyFunc).GetComponent<Projectile>().projectile.reloadTime;
	}

	bool CanFire()
	{
		return reloadTimer > currentReloadTime;
	}

	public void FireStart()
	{
		if (CanFire())
		{ 
			Fire();
			firing = true;
		}
	}

	private void Update()
	{
		if (firing && CanFire())
		{
			Fire();
		}
		reloadTimer += Time.deltaTime;
	}


	public void FireEnd()
	{
		firing = false;
	}

	private void Fire()
	{
		Vector3 firingPosition = PlayerManager.Get().GetPlayerFiringPosition();
		Vector3 mousePos = DragManager.GetMouseWorldPosition();

		var nextProjectile = projectilePools[current].GetNext(nextInactiveFunc);
		nextProjectile.transform.position = firingPosition;
		nextProjectile.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		nextProjectile.SetActive(true);

		var scr = nextProjectile.GetComponent<Projectile>();
		scr.Fire(firingPosition, mousePos);

		reloadTimer = 0.0f;
	}

	public void Next()
	{
		current = ++current % (int)ProjectileType.Count;
		currentReloadTime = projectilePools[current].GetNext(nextAnyFunc).GetComponent<Projectile>().projectile.reloadTime;
		reloadTimer = 0.0f;
	}
}

