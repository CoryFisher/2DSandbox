using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SelectionProcedures;

public class DataSelectorInterfaceController : MonoBehaviour
{
	// dynamically resize the layout until we find something we like
	public float XPosMin = -4.0f;
	public float YPosMin = -1.0f;
	public int MaxLettersPerLine = 9;
	public float Spacing = 1.0f;
	private float last_xPosMin;
	private float last_yPosMin;
	private int last_maxLettersPerLine;
	private float last_spacing;
	private GridLayoutGroup layoutGroup;
	
	// private cache
	private List<GameObject> dataSelectionObjects;
	private SelectionProcedureController selectionProcedureController;

	// state
	private bool initialized;

	// editor
	public GameObject TextDataDisplayPrefab;
	public Text instructionText;
	public GameObject dataDisplayParent;
	
	// TODO: move to private?
	public SelectionProcedureType currentSelectionProcedureType;
	
	private void Start()
	{
		selectionProcedureController = new SelectionProcedureController();
		dataSelectionObjects = new List<GameObject>();
		layoutGroup = dataDisplayParent.GetComponent<GridLayoutGroup>();
		InitializeFor(currentSelectionProcedureType);
	}

	private void InitializeFor(SelectionProcedureType spType)
	{
		// init controller
		selectionProcedureController.Initialize(spType);

		// set instruction text
		instructionText.text = selectionProcedureController.GetInstructionText();
		
		// create data display objects under a parent for easy cleanup
		Cleanup();

		DisplayData[] dataSet = selectionProcedureController.GetDataSet();
		for (int i = 0; i < dataSet.Length; ++i)
		{
			var obj = Instantiate(TextDataDisplayPrefab, dataDisplayParent.transform);
			dataSelectionObjects.Add(obj);

			var scr = obj.GetComponent<DataDisplayObject>();
			scr.Initialize(this, dataSet[i]);
		}

		// update letters layout
		UpdateObjectLayout();
	}

	private void ResetOnFail()
	{
		// reset letters
		foreach (var obj in dataSelectionObjects)
		{
			obj.GetComponent<DataDisplayObject>().Enable();
		}

		// reset controller
		selectionProcedureController.Initialize(currentSelectionProcedureType);
		instructionText.text = selectionProcedureController.GetInstructionText();
	}

	private void Update()
	{
		if (last_xPosMin != XPosMin ||
			last_yPosMin != YPosMin ||
			last_maxLettersPerLine != MaxLettersPerLine ||
			last_spacing != Spacing)
		{
			if (MaxLettersPerLine == 0)
			{
				MaxLettersPerLine = last_maxLettersPerLine;
			}
			UpdateObjectLayout();
		}

		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			ResetOnFail();
		}
		else if (Input.GetKeyDown(KeyCode.Tab))
		{
			int next = ((int)currentSelectionProcedureType + 1) % Enum.GetValues(typeof(SelectionProcedureType)).Length;
			currentSelectionProcedureType = (SelectionProcedureType)next;
			InitializeFor(currentSelectionProcedureType);
		}
	}

	public void NotifyObjectSelected(DataDisplayObject alphabetObject)
	{
		// notify selection controller
		if (selectionProcedureController.NotifyDataSelected(alphabetObject.GetData()))
		{
			// TODO: do something with alphabetObject
			//alphabetObject.gameObject.SetActive(false);
			alphabetObject.Disable();
		}

		// check completion state
		if (selectionProcedureController.GetCompletionState() == CompletionState.Succeeded)
		{
			// do something, YAY
			instructionText.text = "YAY, YOU WIN";
		}
		else if (selectionProcedureController.GetCompletionState() == CompletionState.Failed)
		{
			// do something, BOO
			instructionText.text = "BOO, YOU LOSE";

			// reset
			StartCoroutine("ResetAfterSeconds", 3.0f);
		}
	}

	void UpdateObjectLayout()
	{
		/*if (dataSelectionObjects != null)
		{
			float xPosOffset = 0.0f;
			float yPosOffset = 0.0f;
			for (int i = 0; i < dataSelectionObjects.Count; ++i)
			{
				xPosOffset = (i % MaxLettersPerLine) * Spacing;
				yPosOffset = (i / MaxLettersPerLine) * Spacing;
				dataSelectionObjects[i].transform.position = new Vector3(XPosMin + xPosOffset, YPosMin - yPosOffset, 0);
			}
		}*/

		layoutGroup.constraintCount = MaxLettersPerLine;
		

		last_xPosMin = XPosMin;
		last_yPosMin = YPosMin;
		last_maxLettersPerLine = MaxLettersPerLine;
		last_spacing = Spacing;
	}

	void Cleanup()
	{	
		foreach (var obj in dataSelectionObjects)
		{
			Destroy(obj);
		}
		dataSelectionObjects.Clear();
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	IEnumerator ResetAfterSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		ResetOnFail();
	}
}