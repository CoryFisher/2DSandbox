using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragManager : Singleton<DragManager>
{
	public class DragPoints
	{
		public Vector3 Start { get; set; }
		public Vector3 End { get; set; }
		public DragPoints(Vector3 start, Vector3 end) { Start = start; End = end; }
		public Vector3 Drag { get { return End - Start; } }
	}
	private DragPoints dragPoints;

	// Events
	public delegate void DragEventHandler();
	public event DragEventHandler OnDragStart;
	public event DragEventHandler OnDragHold;
	public event DragEventHandler OnDragEnd;

	// Editor Values
	public float mouseDragMultiplier = 1.0f;


	// Public

	public static Vector3 GetMouseWorldPosition()
	{
		var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		worldPoint.z = 0.0f;
		return worldPoint;
	}

	public DragPoints GetDragPoints()
	{
		return dragPoints;
	}

	public Vector3 GetDrag()
	{
		return dragPoints.Drag;
	}

	// Private

	private void Awake()
	{
		RegisterSingletonInstance(this);
		dragPoints = new DragPoints(Vector3.zero, Vector3.zero);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// Press
			dragPoints.Start = dragPoints.End = GetMouseWorldPosition();
			if (OnDragStart != null) { OnDragStart.Invoke(); }
		}
		else if (Input.GetMouseButtonUp(0))
		{
			// Release
			if (OnDragEnd != null) { OnDragEnd.Invoke(); }
		}
		else if (Input.GetMouseButton(0))
		{
			// Hold
			Vector3 mouseWorldPos = GetMouseWorldPosition();
			dragPoints.End += (mouseWorldPos - dragPoints.End) * mouseDragMultiplier;
			if (OnDragHold != null) { OnDragHold.Invoke(); }
		}
	}
}
