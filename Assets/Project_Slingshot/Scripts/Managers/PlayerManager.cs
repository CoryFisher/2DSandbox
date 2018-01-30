using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
	public float maxMovementSpeed = 5.0f;
}

public class PlayerManager : Singleton<PlayerManager>
{
	// private cache
	private GameObject playerObject;
	private Rigidbody2D playerRigidbody;
	private PlayerData playerData;

	// Editor Values
	public GameObject playerPrefab;

	
	// Private Methods

	private void Awake()
	{
		RegisterSingletonInstance(this);
	}
	
	private void SpawnPlayer(Vector2 position)
	{
		playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
		playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
	}


	// Public Methods

	public GameObject GetPlayer()
	{
		return playerObject;
	}

	public Vector3 GetPlayerPosition()
	{
		return playerObject.transform.position;
	}

	public Vector3 GetPlayerFiringPosition()
	{
		return playerObject.transform.position;
	}

	public Rigidbody2D GetPlayerRigidbody()
	{
		return playerRigidbody;
	}

	public PlayerData GetPlayerData()
	{
		return playerData;
	}
}
