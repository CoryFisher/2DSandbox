using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
	public ProjectileController projectile;
	public LaserController laser;
	Object current;

	private void Awake()
	{
		current = projectile;
	}

	void Toggle()
	{
		if (current == projectile)
		{
			current = laser;
		}
		else
		{
			current = projectile;
		}
	}

	private void Update()
	{
		if (Input.GetMouseButton(1))
		{
			if (current == projectile)
			{
				projectile.FireStart();
			}
			else
			{
				laser.FireStart();
			}
		}
		else if (Input.GetMouseButtonUp(1))
		{
			if (current == projectile)
			{
				projectile.FireEnd();
			}
			else
			{
				laser.FireEnd();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Tab))
		{
			Toggle();
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			if (current == projectile)
			{
				projectile.Next();
			}
			else
			{
				laser.Next();
			}
		}
	}
}
