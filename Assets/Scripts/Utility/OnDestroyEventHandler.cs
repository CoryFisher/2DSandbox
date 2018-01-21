using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDestroyEventHandler : MonoBehaviour
{
	public delegate void OnDestroyedHandler();
	public event OnDestroyedHandler OnDestroyed;

	private void OnDestroy()
	{
		OnDestroyed.Invoke();
		OnDestroyed = null;
	}
}
