using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
	private Rigidbody2D rb;
	private LineRenderer lineRenderer;
	private float lineMaxLengthSqr;
	private Vector3 currentDrag;

	// Editor values
	public float lineStartWidth = 0.25f;
	public float lineMaxWidth = 1.0f;
	public float lineMaxLength = 2.5f;
	public Color lineStartColor = Color.white;
	public Color lineEndColor = Color.red;

	
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

		UpdateDrag(DragManager.Get().GetDragPoints());
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
		lineRenderer.SetPosition(0, (Vector3)rb.position);
		lineRenderer.SetPosition(1, (Vector3)rb.position + currentDrag);

		// set line width lerped
		lineRenderer.endWidth = Mathf.Lerp(lineStartWidth, lineMaxWidth, currentDrag.sqrMagnitude / lineMaxLengthSqr);

		// set end color lerped
		Color endColor = Color.Lerp(lineStartColor, lineEndColor, currentDrag.sqrMagnitude / lineMaxLengthSqr);
		lineRenderer.endColor = endColor;
	}
}
