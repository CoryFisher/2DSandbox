using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T singletonInstance;

	public static T Get()
	{
		return singletonInstance;
	}

	protected static void RegisterSingletonInstance(T instance)
	{
		if (singletonInstance == null)
		{
			singletonInstance = instance;
		}
		else if (singletonInstance != instance)
		{
			Debug.LogWarning("Multiple Singleton Instances of type: " + typeof(T).Name);
			Debug.LogWarning("Destroying second instance");
			Destroy(instance);
		}
	}
}