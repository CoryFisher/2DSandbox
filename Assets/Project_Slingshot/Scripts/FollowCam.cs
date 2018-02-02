using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
	private float zOffset = -10.0f;
	private GameObject target;

	private void Start()
	{
		target = PlayerManager.Get().GetPlayer();
	}

	private void LateUpdate()
	{
		if (target != null)
		{
			var newPos = target.transform.position;
			newPos.z = zOffset;
			transform.position = newPos;
		}
	}
}
