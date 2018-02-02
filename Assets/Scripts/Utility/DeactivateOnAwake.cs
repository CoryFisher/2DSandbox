using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnAwake : MonoBehaviour
{
	private void Awake()
	{
		gameObject.SetActive(false);
	}
}
