using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectileBehavior
{
	void SetLevel(int level);
	void Fire(Vector3 direction);
	void OnUpdate();
	bool OnCollision(GameObject collidingObject);
}
