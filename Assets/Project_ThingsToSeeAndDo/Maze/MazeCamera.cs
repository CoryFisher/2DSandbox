using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCamera : MonoBehaviour
{
	//Camera cam;
	Rigidbody2D rb;
	Transform targetTransform;

	public float maxSpeed = 5.0f;
	public float acceleration = 5.0f;
	public float camSeekLerpFactor = 0.85f;
	public float arrivalRadius = 1.0f;
	public float arrivalTime = 0.25f;

	private void Awake()
	{
		//cam = GetComponent<Camera>();
		rb = GetComponent<Rigidbody2D>();
	}

	//private void Update()
	//{
	//	if (Input.GetMouseButton(0))
	//	{
	//		target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	//		target.z = 0f;
	//	}
	//}

	private void LateUpdate()
	{
		targetTransform = MazeRunner.Get().GetCurrentCellTransform();
		if (targetTransform != null)
		{
			var target = targetTransform.position;
			target.z = -10f;

			Vector2 toTarget = target - transform.position;
			var toTargetMagnitude = toTarget.magnitude;

			// stop
			if (toTargetMagnitude < 0.001f)
			{
				rb.velocity = Vector2.zero;
				transform.position = target;
			}
			// arrive
			else if (toTargetMagnitude <= arrivalRadius) // arrival radius
			{
				var vel = toTarget;
				vel /= arrivalTime;

				if (vel.magnitude > maxSpeed)
				{
					vel /= vel.magnitude;
					vel *= maxSpeed;
				}

				rb.velocity = vel;

			}
			// seek
			else
			{
				// add accel
				var newVel = (toTarget.normalized * rb.velocity.magnitude) + toTarget.normalized * acceleration * Time.deltaTime;

				// max limit
				if (newVel.magnitude > maxSpeed)
				{
					newVel = newVel.normalized * maxSpeed;
				}

				rb.velocity = newVel;
			}
		}
	}
}
