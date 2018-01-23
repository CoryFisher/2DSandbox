using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
	[System.Serializable]
	public static class ProjectileData
	{
		public static Bullet.BulletData[] bulletLevelData;
	}
	Dictionary<Type, object[]> projectileDataDict;

	private void Awake()
	{
		RegisterSingletonInstance(this);

		// TODO: load data from disk
		
		projectileDataDict = new Dictionary<Type, object[]>();
		//projectileDataDict.Add(typeof(Bullet), projectiles.bullet);
	}

	public System.Object GetProjectileData(Type type, int level)
	{
		Debug.Log("type: " + type + ", level: " + level);
		object[] data = projectileDataDict[type];
		if (data != null)
		{
			if (data.Length > level)
			{
				return data[level];
			}
			else
			{
				Debug.LogWarning("DataManager: projectile data missing for level = " + level + " ; type = " + type);
				Debug.LogWarning("DataManager: projectile data for type = " + type + " ; max level = " + (data.Length - 1));
			}
		}
		else
		{
			Debug.LogWarning("DataManager: projectile data missing for type = " + type);
		}
		return null;
	}
}
