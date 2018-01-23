using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	[System.Serializable]
	public class PlayerStats
	{
		public float maxMovementSpeed = 5.0f;
	}

	private GameObject player;
	private Rigidbody2D playerRigidbody;

	// Editor Values
	public GameObject playerPrefab;
	// TODO: move to Data Manager
	public PlayerStats playerStats;
	

	// Public

	public GameObject GetPlayer()
	{
		return player;
	}

	public Vector3 GetPlayerPosition()
	{
		return player.transform.position;
	}

	public Rigidbody2D GetPlayerRigidbody()
	{
		return playerRigidbody;
	}

	public PlayerStats GetPlayerStats()
	{
		return playerStats;
	}


	// Private

	private void Awake()
	{
		RegisterSingletonInstance(this);

		player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		playerRigidbody = player.GetComponent<Rigidbody2D>();
	}
}
