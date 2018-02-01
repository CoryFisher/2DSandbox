using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	// private cache
	private GameObject playerObject;
	private Player playerScript;
	private Rigidbody2D playerRigidbody;

	// Editor Values
	public GameObject playerPrefab;
	
	// Private Methods

	private void Awake()
	{
		RegisterSingletonInstance(this);
		FindOrSpawnPlayer(Vector2.zero);
	}
	
	private void FindOrSpawnPlayer(Vector2 position)
	{
		playerObject = GameObject.FindGameObjectWithTag("Player");
		if (playerObject == null)
		{
			playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
		}
		playerScript = playerObject.GetComponent<Player>();
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
		return playerScript.firingPosition.position;
	}

	public void SetPlayerOverlayColor(Color color)
	{
		playerScript.overlaySpriteRenderer.color = color;
	}

	public Rigidbody2D GetPlayerRigidbody()
	{
		return playerRigidbody;
	}

	public PlayerData GetPlayerData()
	{
		return playerScript.data;
	}
}
