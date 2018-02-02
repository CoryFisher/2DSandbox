using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationMovementController : MonoBehaviour
{
	Rigidbody2D rb;
	Vector2 desiredDirection;

	public float forceMult = 40f;
	public float slowMult = 0.9f;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		desiredDirection = Vector2.zero;

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			desiredDirection += Vector2.up;
		}
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			desiredDirection += Vector2.down;
		}
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			desiredDirection += Vector2.left;
		}
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			desiredDirection += Vector2.right;
		}
	}

	private void FixedUpdate()
	{
		if (rb.velocity != Vector2.zero)
		{
			rb.velocity = rb.velocity * slowMult;
		}

		Vector2 force = desiredDirection.normalized * forceMult;
		rb.AddForce(force);

		float maxSpeed = PlayerManager.Get().GetPlayerData().maxMovementSpeed;
		if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
		{
			rb.velocity = rb.velocity.normalized * maxSpeed;
		}

		Debug.Log("Velocity: " + rb.velocity);
	}
}
