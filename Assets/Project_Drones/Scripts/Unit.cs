using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public int health = 1;
	
	public float moveSpeed = 1.0f;
	//private float sqrMoveSpeed = 0.0f;


	public Vector2 destination = Vector2.zero;

	public int resources = 0;
	public int resourceDrainAmount = 1;
	public float drainResourceInterval = 1.0f;
	private Resource currentResource = null;
	private float drainResourceTimer = 0.0f;

	private Rigidbody2D rigidBody = null;


	private void Start()
	{
		moveSpeed = moveSpeed + UnityEngine.Random.Range(0.0f, 5.0f);
		//sqrMoveSpeed = moveSpeed * moveSpeed;

		rigidBody = GetComponent<Rigidbody2D>();

		UnitManager.RegisterUnit(this);

		AquireRandomDestination();
	}

	private void AquireRandomDestination()
	{
		destination = UnitManager.GetRandomDestinationWithinBounds();
	}

	private void Update()
	{
		if ((destination - rigidBody.position).sqrMagnitude > 0.01f)
		{
			Vector2 translation = (destination - rigidBody.position).normalized * moveSpeed * Time.deltaTime;
			rigidBody.MovePosition(rigidBody.position + translation);
		}
		else if (currentResource != null)
		{
			drainResourceTimer += Time.deltaTime;
			if (drainResourceTimer > drainResourceInterval)
			{
				resources += currentResource.Drain(resourceDrainAmount);
				drainResourceTimer = 0.0f;
			}
			if (currentResource.IsDrained())
			{
				currentResource = null;
			}
		}
		else
		{
			AquireRandomDestination();
		}
	}

	private void OnMouseUpAsButton()
	{
		AquireRandomDestination();
	}
	
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Resource res = collision.gameObject.GetComponent<Resource>();
		if (res)
		{
			Debug.Log("Triggered with 2D resource");
			destination = transform.position;
			currentResource = res;
			drainResourceTimer = 0.0f;
		}
	}
}