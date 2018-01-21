using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
	public GameObject unitPrefab;
	private static List<Unit> units = new List<Unit>();
	private static int numUnits = 0;

	public int numUnitsToSpawn = 100;

	int m_frameCounter = 0;
	float m_timeCounter = 0.0f;
	float m_lastFramerate = 0.0f;
	public float m_refreshTime = 0.5f;
	public Text fpsText;
	public Text numUnitsText;

	private static Rect bounds = new Rect(-30.0f, -30.0f, 60.0f, 60.0f);

	public static void RegisterUnit(Unit unit)
	{
		units.Add(unit);
	}
	public static Vector2 GetRandomDestinationWithinBounds()
	{
		return new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			foreach (var unit in units)
			{
				unit.destination = new Vector2(Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y));
			}
		}
		else if (Input.GetMouseButtonDown(1))
		{
			Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			point.z = 0.0f;
			for (int i = 0; i < numUnitsToSpawn; ++i)
			{
				Instantiate(unitPrefab, point, Quaternion.identity);
				++numUnits;
			}
			Debug.Log("Created unit #" + numUnits);
			numUnitsText.text = "Units: " + numUnits;
		}

		if (m_timeCounter < m_refreshTime)
		{
			m_timeCounter += Time.deltaTime;
			m_frameCounter++;
		}
		else
		{
			//This code will break if you set your m_refreshTime to 0, which makes no sense.
			m_lastFramerate = (float)m_frameCounter / m_timeCounter;
			fpsText.text = "FPS: " + m_lastFramerate;
			m_frameCounter = 0;
			m_timeCounter = 0.0f;
		}
	}
}
