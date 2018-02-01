using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
	private void Awake()
	{
		projectile.color = Color.black;
	}
}
