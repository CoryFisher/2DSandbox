using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
	private const int poolIncreaseSize = 5;

	private T[] pool;
	private int current;
	private Func<T> Creator;

	public ObjectPool(int size, Func<T> creator)
	{
		Creator = creator;
		pool = new T[size];
		for (int i = 0; i < size; ++i)
		{
			pool[i] = Creator();
		}
	}
	
	private void IncreasePoolSize(int size)
	{
		T[] newPool = new T[pool.Length + size];
		pool.CopyTo(newPool, 0);
		for (int i = pool.Length; i < newPool.Length; ++i)
		{
			newPool[i] = Creator();
		}
		pool = newPool;
	}
	
	public T GetNext(Func<T, bool> evaluator)
	{
		for (int i = 0; i < pool.Length; ++i)
		{
			current = ++current % pool.Length;
			if (evaluator(pool[current]))
			{
				return pool[current];
			}
		}
		current = pool.Length;
		IncreasePoolSize(poolIncreaseSize);
		Debug.Assert(evaluator(pool[current]), "Newly created objects must satisfy the evaluator");
		return pool[current];
	}
}
