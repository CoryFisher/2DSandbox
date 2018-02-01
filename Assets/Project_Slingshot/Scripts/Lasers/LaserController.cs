using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// holds a list of LaserControllers and notifies the current selected controller about input events

public enum LaserType
{
	Laser,
	//FireLaser,
	Count
}

public class LaserController : MonoBehaviour
{
	ILaser[] lasers;
	int current;
	bool firing;

	public GameObject laserPrefab;
	//public ILaser fireLaser;


	private void Awake()
	{
		lasers = new ILaser[(int)LaserType.Count];
		var laser = Instantiate(laserPrefab, transform);
		lasers[(int)LaserType.Laser] = laser.GetComponent<ILaser>();
		//lasers[(int)LaserType.FireLaser] = fireLaser;
	}

	public void FireStart()
	{
		if (lasers[current].CanFire())
		{
			Vector3 firingPosition = PlayerManager.Get().GetPlayerFiringPosition();
			Vector3 mousePos = DragManager.GetMouseWorldPosition();
			lasers[current].StartFire(firingPosition, mousePos);
		}
		firing = true;
	}
	
	public void FireEnd()
	{
		lasers[current].EndFire();
		firing = false;
	}

	private void Update()
	{
		if (firing && lasers[current].CanFire())
		{
			Vector3 firingPosition = PlayerManager.Get().GetPlayerFiringPosition();
			Vector3 mousePos = DragManager.GetMouseWorldPosition();
			lasers[current].Fire(firingPosition, mousePos);
		}
	}

	public void Next()
	{
		current = ++current % (int)LaserType.Count;
	}
}

