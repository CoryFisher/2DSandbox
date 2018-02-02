using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingshotMovementController : MonoBehaviour
{
	private Rigidbody2D rb;
	private LineRenderer lineRenderer;
	private float lineMaxLengthSqr;
	private Vector3 currentDrag;
	private Vector3 dragStart;

	// Editor values for UI
	public float lineStartWidth = 0.25f;
	public float lineMaxWidth = 1.0f;
	public float lineMaxLength = 2.5f;
	public Color lineStartColor = Color.white;
	public Color lineEndColor = Color.red;
	//public bool renderLineOnMousePos = true;
	public enum LineRenderPosition { dragStart, mouse, player }
	public LineRenderPosition lineRenderPosition;


	// Private

	private void Awake()
	{	
		rb = GetComponent<Rigidbody2D>();
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.startWidth = lineStartWidth;
		lineRenderer.endWidth = lineStartWidth;
		lineRenderer.enabled = false;
		lineMaxLengthSqr = lineMaxLength * lineMaxLength;
	}

	private void Start()
	{
		DragManager.Get().OnDragStart += SlingshotController_OnDragStart;
		DragManager.Get().OnDragHold += SlingshotController_OnDragHold;
		DragManager.Get().OnDragEnd += SlingshotController_OnDragEnd;
	}
	
	private void OnDestroy()
	{
		DragManager.Get().OnDragStart -= SlingshotController_OnDragStart;
		DragManager.Get().OnDragHold -= SlingshotController_OnDragHold;
		DragManager.Get().OnDragEnd -= SlingshotController_OnDragEnd;
	}

	private void SlingshotController_OnDragStart()
	{
		rb.velocity = Vector3.zero;
		var points = DragManager.Get().GetDragPoints();
		dragStart = points.Start;
		UpdateDrag(points);
		lineRenderer.enabled = true;
	}

	private void SlingshotController_OnDragHold()
	{
		UpdateDrag(DragManager.Get().GetDragPoints());
	}

	private void SlingshotController_OnDragEnd()
	{
		float playerMoveSpeedMax = PlayerManager.Get().GetPlayerData().maxMovementSpeed;
		rb.velocity = currentDrag.normalized * -playerMoveSpeedMax * (currentDrag.magnitude / lineMaxLength);
		rb.simulated = true;

		lineRenderer.enabled = false;
	}

	private void UpdateDrag(DragManager.DragPoints dragPoints)
	{
		currentDrag = dragPoints.Drag;

		// truncate at max length
		if (currentDrag.sqrMagnitude > lineMaxLengthSqr)
		{
			currentDrag = currentDrag.normalized * lineMaxLength;
		}

		// set line position
		switch (lineRenderPosition)
		{
			case LineRenderPosition.dragStart:
				lineRenderer.SetPosition(0, dragStart);
				lineRenderer.SetPosition(1, dragStart + currentDrag);
				break;
			case LineRenderPosition.mouse:
				var mousepos = DragManager.GetMouseWorldPosition();
				lineRenderer.SetPosition(0, mousepos);
				lineRenderer.SetPosition(1, mousepos + currentDrag);
				break;
			case LineRenderPosition.player:
				lineRenderer.SetPosition(0, (Vector3)rb.position);
				lineRenderer.SetPosition(1, (Vector3)rb.position + currentDrag);
				break;
		}

		// set line width lerped
		lineRenderer.endWidth = Mathf.Lerp(lineStartWidth, lineMaxWidth, currentDrag.sqrMagnitude / lineMaxLengthSqr);

		// set end color lerped
		Color endColor = Color.Lerp(lineStartColor, lineEndColor, currentDrag.sqrMagnitude / lineMaxLengthSqr);
		lineRenderer.endColor = endColor;
	}
}
