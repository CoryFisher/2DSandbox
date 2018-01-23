using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
	private List<GameObject>[] projectilePools;
	private System.Type[] projectileTypes;

	private System.Type currentProjectileType;
	private Projectile currentProjectileBehavior;


	// Editor Values
	public GameObject projectilePrefab;
	public int projectilePoolSize = 20;
	public int projectilePoolSizeIncrease = 5;
	public float projectileMaxDist = 10.0f;
	public float projectileLifetime = 1.0f;


	// Private

	private void Awake()
	{
		projectilePools = new List<GameObject>[];
		IncreaseProjectilePool(projectilePoolSize);

		projectileTypes = new List<System.Type>();
		projectileTypes.Add(typeof(Bullet));

		currentProjectileType = projectileTypes[0];
	}

	private void Update()
	{
		// TODO: make this a callback from PlayerManager?
		// TODO: 
		if (Input.GetMouseButtonDown(1))
		{
			Vector3 playerPosition = PlayerManager.Get().GetPlayerPosition();
			Vector3 mouseWorldPos = DragManager.GetMouseWorldPosition();
			Vector3 playerToMousePos = mouseWorldPos - playerPosition;

			//ProjectileComponents data = GetNextProjectile();
			GameObject projectileObject = GetNextProjectileOfType(currentProjectileType)
			if (projectileObject != null)
			{
				// Activate
				projectileObject.transform.position = playerPosition;
				projectileObject.SetActive(true);


				projectileBehavior = projectileObject.GetComponent(currentProjectileType) as Projectile;
				// TODO: move to pool intialization
				projectileBehavior.Initialize(0);
				projectileBehavior.StartFire(playerPosition, mouseWorldPos, playerToMousePos);
			}
		}
		else if (Input.GetMouseButtonUp(1))
		{
			Vector3 playerPosition = PlayerManager.Get().GetPlayerPosition();
			Vector3 mouseWorldPos = DragManager.GetMouseWorldPosition();
			Vector3 playerToMousePos = mouseWorldPos - playerPosition;
			
			projectileBehavior.EndFire(playerPosition, mouseWorldPos, playerToMousePos);
		}
	}

	private ProjectileComponents GetNextProjectile()
	{
		ProjectileComponents freeProjectile = projectilePools.Find(x => x.projectile.activeSelf == false);
		if (freeProjectile == null)
		{
			IncreaseProjectilePool(projectilePoolSizeIncrease);
			freeProjectile = projectilePools.Find(x => x.projectile.activeSelf == true);
		}
		return freeProjectile;
	}

	private void IncreaseProjectilePool(int size)
	{
		for (int i = 0; i < size; ++i)
		{
			GameObject projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
			projectile.SetActive(false);
			ProjectileComponents data = new ProjectileComponents(projectile);
			projectilePools.Add(data);
		}
	}
}
