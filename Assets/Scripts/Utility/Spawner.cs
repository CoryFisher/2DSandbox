using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	private GameObject spawnedObject;
	private OnDestroyEventHandler onDestoryHandlerScript;

	public GameObject objectToSpawn;
	public float objectLifetime;
	public bool respawnAfterDestruction;
	public float respawnAfterDestructionDelay;

	private void Start()
	{
		SpawnObject();
	}

	private void SpawnObject()
	{
		//spawn object
		spawnedObject = Instantiate(objectToSpawn, transform);

		if (objectLifetime > 0)
		{
			// initiate delayed destruction
			DestroyObject(spawnedObject, objectLifetime);
		}

		// get OnDestroyed event handler, else add it to object
		onDestoryHandlerScript = spawnedObject.GetComponent<OnDestroyEventHandler>();
		if (onDestoryHandlerScript == null)
		{
			Debug.LogWarning("objectToSpawn requires OnDestroyEventHandler script. Adding component and continuing.");
			onDestoryHandlerScript = spawnedObject.AddComponent<OnDestroyEventHandler>();
		}

		// register OnDestroy event handler
		onDestoryHandlerScript.OnDestroyed += this.OnSpawnedObjectDestroyed;
	}

	private void OnSpawnedObjectDestroyed()
	{
		onDestoryHandlerScript.OnDestroyed -= this.OnSpawnedObjectDestroyed;
		spawnedObject = null;
		onDestoryHandlerScript = null;

		if (respawnAfterDestruction && gameObject.activeSelf)
		{
			StartCoroutine("RespawnAfterDestruction");
		}
	}

	private IEnumerator RespawnAfterDestruction()
	{
		yield return new WaitForSeconds(respawnAfterDestructionDelay);
		SpawnObject();
	}
}
