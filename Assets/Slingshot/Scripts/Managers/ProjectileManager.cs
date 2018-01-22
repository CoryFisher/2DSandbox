using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
	private class ProjectileComponents
	{
		public GameObject projectile;
		public Rigidbody2D projectileRigidbody;
		public Projectile projectileScript;

		public ProjectileComponents(GameObject projectile)
		{
			this.projectile = projectile;
			this.projectileRigidbody = projectile.GetComponent<Rigidbody2D>();
			this.projectileScript = projectile.GetComponent<Projectile>();
		}
	}
	private List<ProjectileComponents> projectilePool;

	private List<System.Type> projectileTypes;
	private System.Type currentProjectileType;


	// Editor Values
	public GameObject projectilePrefab;
	public int projectilePoolSize = 20;
	public int projectilePoolSizeIncrease = 5;
	public float projectileMaxDist = 10.0f;
	public float projectileLifetime = 1.0f;


	// Private

	private void Awake()
	{
		projectilePool = new List<ProjectileComponents>(projectilePoolSize);
		IncreaseProjectilePool(projectilePoolSize);

		projectileTypes = new List<System.Type>();
		projectileTypes.Add(typeof(Bullet));

		currentProjectileType = projectileTypes[0];
	}
	
	private void Update()
	{
		// TODO: make this a callback from PlayerManager
		// TODO: fire rate
		if (Input.GetMouseButtonDown(1))
		{
			Vector3 playerPosition = PlayerManager.Get().GetPlayerPosition();
			Vector3 mouseWorldPos = DragManager.GetMouseWorldPosition();
			Vector3 toMousePos = mouseWorldPos - playerPosition;

			ProjectileComponents data = GetNextProjectile();
			if (data != null)
			{
				// Activate
				data.projectile.transform.position = playerPosition;
				data.projectile.SetActive(true);

				// Reset velocity
				data.projectileRigidbody.velocity = Vector3.zero;

				// Set expiry
				data.projectileScript.ExpireByDistance(playerPosition, projectileMaxDist);
				data.projectileScript.ExpireByLifetime(projectileLifetime);

				// Create behavior
				var projectileBehavior = data.projectile.AddComponent(currentProjectileType) as IProjectileBehavior;
				projectileBehavior.SetLevel(0);
				data.projectileScript.SetBehavior(projectileBehavior);

				// Fire
				data.projectileScript.Fire(toMousePos);
			}
		}
	}
	private ProjectileComponents GetNextProjectile()
	{
		ProjectileComponents freeProjectile = projectilePool.Find(x => x.projectile.activeSelf == false);
		if (freeProjectile == null)
		{
			IncreaseProjectilePool(projectilePoolSizeIncrease);
			freeProjectile = projectilePool.Find(x => x.projectile.activeSelf == true);
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
			projectilePool.Add(data);
		}
	}
}
