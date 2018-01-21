using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
	public int amount = 0;
	public int maxAmount = 0;

	public int Drain(int drainAmount)
	{
		int drained = Math.Min(amount, drainAmount);
		amount -= drained;
		return drained;
	}

	public bool IsDrained()
	{
		return amount < 1;
	}

	private void Awake()
	{
		if (amount == 0)
		{
			amount = maxAmount;
		}
	}
	
	private void OnMouseUpAsButton()
	{
		amount = maxAmount;
	}
}