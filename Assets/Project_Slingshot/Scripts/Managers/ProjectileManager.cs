using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
	Projectile,
	Bullet,
	Laser,

	Count
}

public class ProjectileManager : MonoBehaviour
{
	private List<Projectile>[] projectilePools;
	private ProjectileType currentProjectileType;
	private Projectile currentProjectile;
	private int poolIncreaseSize = 5;

	// Editor Values
	public GameObject projectilePrefab;
	public GameObject bulletPrefab;
	public GameObject LaserPrefab;
	public GameObject[] prefabs;


	// Private

	private void Awake()
	{
		InitPrefabList();

		// init object pools with prefabs
		projectilePools = new List<Projectile>[(int)ProjectileType.Count];
		for (int i = 0; i < projectilePools.Length; ++i)
		{
			projectilePools[i] = new List<Projectile>();
			IncreasePoolSize(i);
		}

		// init current projectile type
		currentProjectileType = ProjectileType.Projectile;
	}

	private void IncreasePoolSize(int index)
	{
		for (int i = 0; i < poolIncreaseSize; ++i)
		{
			var projectile = Instantiate(prefabs[index]).GetComponent<Projectile>();
			projectile.gameObject.SetActive(false);
			projectilePools[index].Add(projectile);
		}
	}

	private void InitPrefabList()
	{
		prefabs = new GameObject[(int)ProjectileType.Count];
		prefabs[(int)ProjectileType.Projectile] = projectilePrefab;
		prefabs[(int)ProjectileType.Bullet] = bulletPrefab;
		prefabs[(int)ProjectileType.Laser] = LaserPrefab;
	}

	private void Update()
	{
		// TODO: make this a callback from PlayerManager?
		if (Input.GetMouseButtonDown(1))
		{
			currentProjectile = GetNextProjectileOfType(currentProjectileType);

			Vector3 playerFiringPosition = PlayerManager.Get().GetPlayerFiringPosition();
			Vector3 mouseWorldPos = DragManager.GetMouseWorldPosition();

			// Activate
			currentProjectile.transform.position = playerFiringPosition;
			currentProjectile.gameObject.SetActive(true);
			currentProjectile.FireStart(playerFiringPosition, mouseWorldPos);
		}
		else if (Input.GetMouseButtonUp(1))
		{
			if (currentProjectile != null)
			{
				currentProjectile.FireEnd();
				currentProjectile = null;
			}
		}
	}

	private Projectile GetNextProjectileOfType(ProjectileType type)
	{
		Projectile nextProjectile = projectilePools[(int)type].Find(x => x.gameObject.activeSelf == false);
		if (nextProjectile == null)
		{
			IncreasePoolSize((int)type);
			nextProjectile = projectilePools[(int)type].Find(x => x.gameObject.activeSelf == false);
		}
		return nextProjectile;
	}
}
