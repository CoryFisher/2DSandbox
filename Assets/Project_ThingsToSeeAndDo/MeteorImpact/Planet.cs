using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// gets hit by things and gains their pixels and mass
public class Planet : MonoBehaviour
{
	public GameObject meteorPrefab;
	public Rigidbody2D RigidBody;

	public float meteorDistance = 5.0f;
	public float meteorSpeed = 2.0f;
	public float meteorPenetrationAmount = 0.2f;
	public float fireDelay = 0.25f;
	public float angleUnit = 20.0f;
	public float absorbMeteorLerpFactor = 0.3f;
	public float arcLength = 1.0f;

	private float timer = 0.0f;
	private int counter = 0;
	private Vector2 meteorPos;
	private Vector2 targetPos;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		ContactPoint2D[] contactPoints = new ContactPoint2D[5];
		collision.GetContacts(contactPoints);
		OnCollision(collision.gameObject, contactPoints[0]);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		ContactPoint2D[] contactPoints = new ContactPoint2D[5];
		collision.GetContacts(contactPoints);
		OnCollision(collision.gameObject, contactPoints[0]);
	}

	private void OnCollision(GameObject collisionObject, ContactPoint2D contactPoint)
	{
		var meteor = collisionObject.GetComponent<Meteor>();
		if (meteor)
		{
			// store the impact force
			//var force = meteor.RigidBody.mass * meteor.RigidBody.velocity;

			// lock it's transorm to ours
			//AbsorbMeteorData data = new AbsorbMeteorData();
			//data.lerpTransform = meteor.SpriteObject.transform;
			//data.putOnTarget = meteor.transform;
			//data.target = meteor.transform.position + ((Vector3)meteor.RigidBody.velocity.normalized * meteor.transform.localScale.x * 0.5f);
			//StartCoroutine("AbsorbMeteor", data);

			meteor.transform.position = meteor.transform.position + ((Vector3)meteor.RigidBody.velocity.normalized * meteor.transform.localScale.x * meteorPenetrationAmount);

			// stop the meteor
			Destroy(meteor.RigidBody);
			
			var toContactPoint = contactPoint.point - (Vector2)collisionObject.transform.position;
			var raycastHits = Physics2D.RaycastAll(collisionObject.transform.position, toContactPoint, toContactPoint.magnitude + 0.5f);
			
			var hit = raycastHits.First(x => x.transform != collisionObject.transform);
			meteor.transform.SetParent(hit.transform);
			

			// apply impact force
			//RigidBody.AddTorque(1.0f, ForceMode2D.Impulse);
		}
	}

	struct AbsorbMeteorData
	{
		public Transform lerpTransform;
		public Transform putOnTarget;
		public Vector3 target;

	};
	IEnumerator AbsorbMeteor(AbsorbMeteorData data)
	{
		var lerpStart = data.lerpTransform.position;
		var fromTarget = lerpStart - data.target;
		
		data.putOnTarget.position = data.target;

		float t = 0.0f;
		while ((data.lerpTransform.position - data.target).sqrMagnitude > 0.00001)
		{
			data.lerpTransform.localPosition = Vector3.Lerp(fromTarget, Vector3.zero, t);
			t += 0.05f;
			yield return null;
		}

		data.lerpTransform.localPosition = Vector3.zero;
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (Input.GetKey(KeyCode.Alpha1) && timer > fireDelay)
		{
			timer = 0.0f;

			float angle = Random.Range(0f, 360f);
			meteorPos = Quaternion.Euler(0f, 0f, angle) * (Vector2.right * meteorDistance);

			targetPos = (meteorPos - (Vector2)transform.position).normalized * transform.localScale.x * 0.5f;
			angle = Random.Range(-60f, 60f);
			targetPos = Quaternion.Euler(0f, 0f, angle) * targetPos;

			FireMeteor(meteorPos, targetPos);

			Debug.DrawLine(meteorPos, targetPos, Color.red);
		}
		else if (Input.GetKey(KeyCode.Alpha2) && timer > fireDelay)
		{
			timer = 0.0f;
			++counter;

			float angle = counter * angleUnit;
			meteorPos = Quaternion.Euler(0f, 0f, angle) * (Vector2.right * meteorDistance);

			targetPos = transform.position;

			FireMeteor(meteorPos, targetPos);

			Debug.DrawLine(meteorPos, targetPos, Color.red);
		}
		else if (Input.GetKey(KeyCode.Alpha3) && timer > fireDelay)
		{
			timer = 0.0f;
			++counter;

			// todo why this no work??
			float radius = (transform.localScale.x * 0.5f) * (counter * 1.1f);
			float innerAnglePerArc = (arcLength / radius) * Mathf.Rad2Deg;
			float angle = counter * innerAnglePerArc;
			meteorPos = Quaternion.Euler(0f, 0f, angle) * (Vector2.right * meteorDistance);

			targetPos = transform.position;

			FireMeteor(meteorPos, targetPos);

			Debug.DrawLine(meteorPos, targetPos, Color.red);
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			for (int i = 0; i < transform.childCount; ++i)
			{
				var child = transform.GetChild(i);
				if (child.CompareTag("Meteor"))
				{
					Destroy(child.gameObject);
				}
			}

			timer = 0.0f;
			counter = 0;
		}
	}



	void FireMeteor(Vector2 position, Vector2 target)
	{
		var meteorObj = Instantiate(meteorPrefab);
		meteorObj.transform.position = position;
		Vector2 toTarget = target - position;
		var meteor = meteorObj.GetComponent<Meteor>();
		meteor.RigidBody.velocity = toTarget.normalized * meteorSpeed;
		meteor.SpriteRenderer.color = Random.ColorHSV();
	}

	void PrintPixels(Color[] pixels, string label)
	{
		string pixelOutput = "";
		foreach (var pixel in pixels)
		{
			pixelOutput += pixel + "\n";
		}
		Debug.Log(label + " : " + pixels.Length + "\n" + pixelOutput);
	}
}
