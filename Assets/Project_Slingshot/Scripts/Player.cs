using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
	public float maxMovementSpeed = 5.0f;
	public float maxHealth = 10.0f;
	public float health = 10.0f;
}

public class Player : MonoBehaviour
{
	public SpriteRenderer baseSpriteRenderer;
	public SpriteRenderer overlaySpriteRenderer;
	public Transform firingPosition;
	public PlayerData data;

	private void Update()
	{
		Vector3 mousePos = DragManager.GetMouseWorldPosition();
		Vector3 toMousePos = mousePos  - transform.position;
		transform.rotation = Quaternion.Euler(0.0f, 0.0f, Vector3.SignedAngle(Vector3.right, toMousePos, Vector3.forward));
	}
}
