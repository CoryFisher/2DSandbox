using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FakeGizmo : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<SpriteRenderer>().enabled = false;
	}
}
