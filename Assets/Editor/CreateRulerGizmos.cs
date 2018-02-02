using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CreateRulerGizmos
{
	private const string createFirstTen = "CUSTOM/Create Ruler Gizmos 0-10";

	[MenuItem(createFirstTen)]
	public static void CreatePrefab()
	{
		GameObject rulerGizmos = new GameObject("RulerGizmos");
		for (int i = 0; i < 10; ++i)
		{
			GameObject rulerMarker = new GameObject("Marker_" + i);
			rulerMarker.transform.position = new Vector3(i, 0f, 0f);
			rulerMarker.transform.SetParent(rulerGizmos.transform);

			
		}
	}

	[MenuItem(createFirstTen, true)]
	public static bool ValidateCreatePrefab()
	{
		GameObject gizmosParent = GameObject.Find("RulerGizmos");
		return gizmosParent == null;
	}


	private const string destroyGizmos = "CUSTOM/Destroy Ruler Gizmos";

	[MenuItem(destroyGizmos)]
	public static void DestroyGizmos()
	{
		GameObject gizmosParent = GameObject.Find("RulerGizmos");
		if (gizmosParent != null)
		{
			GameObject.DestroyImmediate(gizmosParent);
		}
	}
}
