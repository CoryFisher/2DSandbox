using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SelectionProcedures
{
	public enum SelectionProcedureType
	{
		ForwardOrder,
		ReverseOrder,
		StraightSet,
		CurvySet,
		VerticallySymmetricalSet,
		HorizontallySymmetricalSet,
	}

	public interface ISelectionProcedure
	{
		// the instruction text to display
		string GetInstructionText();
		// call this to reset (init, reset on fail)
		void Initialize();
		// call this when a letter is selected
		bool NotifyDataSelected(DisplayData selectedData);
		// returns true when procedure is completed
		bool IsComplete();
		// returns the total set of data to display
		DisplayData[] GetDataSet();
	}
	
	public class LetterAttributeSelectionProcedure : ISelectionProcedure
	{
		LetterAttribute letterAttribute;
		bool ordered;
		bool reversed;

		string[] validDataNames;
		List<string> selectionPool;
		int currentIndex;

		public LetterAttributeSelectionProcedure(LetterAttribute letterAttribute, bool ordered = false, bool reversed = false)
		{
			this.letterAttribute = letterAttribute;
			this.ordered = ordered;
			this.reversed = reversed;
			selectionPool = new List<string>();
		}

		public DisplayData[] GetDataSet()
		{
			return DataSelectionDataController.Get().GetAlphabetDisplayData(LetterAttribute.None);
		}

		public string GetInstructionText()
		{
			string attrName = LetterAttributeHelper.GetLetterAttributeName(letterAttribute);
			string attrString = (letterAttribute != LetterAttribute.None) ? (attrName + ' ') : "";
			string orderedString = ordered ? "in " + (reversed ? "reverse " : "") + "alphabertical order" : "";
			return "Select all " + attrString + "letters " + orderedString;
		}

		public void Initialize()
		{
			validDataNames = DataSelectionDataController.Get().GetAlphabetStrings(letterAttribute);
			if (ordered)
			{
				if (reversed)
				{
					currentIndex = validDataNames.Length - 1;
				}
				else
				{
					currentIndex = 0;
				}
			}
			else
			{	
				selectionPool.Clear();
				selectionPool.AddRange(validDataNames);
			}
		}

		public bool IsComplete()
		{
			if (ordered)
			{
				if (reversed)
				{
					return currentIndex < 0;
				}
				else
				{
					return currentIndex >= validDataNames.Length;
				}
			}
			else
			{
				return selectionPool.Count == 0;
			}
		}

		public bool NotifyDataSelected(DisplayData selectedData)
		{
			if (ordered && currentIndex < validDataNames.Length && currentIndex >= 0)
			{
				if (validDataNames[currentIndex] == selectedData.Name)
				{
					currentIndex = reversed ? currentIndex - 1 : currentIndex + 1;
					return true;
				}
			}
			else if (selectionPool.Contains(selectedData.Name))
			{
				selectionPool.Remove(selectedData.Name);
				return true;
			}
			return false;
		}
	}

	public enum CompletionState
	{
		NotStarted,
		InProgress,
		Succeeded,
		Failed
	}

	public class SelectionProcedureController
	{
		CompletionState completion;
		List<char> expectedLetters;

		ISelectionProcedure[] selectionProcedures;
		ISelectionProcedure currentSelectionProcedure;

		public bool StrictSelection;

		public SelectionProcedureController()
		{
			// register selection procedures
			selectionProcedures = new ISelectionProcedure[Enum.GetValues(typeof(SelectionProcedureType)).Length];

			// TODO: remove SelectionProcedureType in favor of on-demand creation of selection procedures with given attributes
				// none
				// straight/curvy
				// horizontal/vertical symmetry
				// in order/reverse order
				// vowel/consoant
			selectionProcedures[(int)SelectionProcedureType.ForwardOrder] = new LetterAttributeSelectionProcedure(LetterAttribute.None, true, false);
			selectionProcedures[(int)SelectionProcedureType.ReverseOrder] = new LetterAttributeSelectionProcedure(LetterAttribute.None, true, true);
			selectionProcedures[(int)SelectionProcedureType.StraightSet] = new LetterAttributeSelectionProcedure(LetterAttribute.Straight);
			selectionProcedures[(int)SelectionProcedureType.CurvySet] = new LetterAttributeSelectionProcedure(LetterAttribute.Curvy);
			selectionProcedures[(int)SelectionProcedureType.VerticallySymmetricalSet] = new LetterAttributeSelectionProcedure(LetterAttribute.VerticallySymmetric);
			selectionProcedures[(int)SelectionProcedureType.HorizontallySymmetricalSet] = new LetterAttributeSelectionProcedure(LetterAttribute.HorizontallySymmetric);

			// set current selection procedure
			Initialize(SelectionProcedureType.ForwardOrder);
		}

		public void Initialize(SelectionProcedureType selectionProcedure)
		{
			// reset completion state
			completion = CompletionState.NotStarted;

			// set current selection procedure
			currentSelectionProcedure = selectionProcedures[(int)selectionProcedure];
			currentSelectionProcedure.Initialize();
		}

		public bool NotifyDataSelected(DisplayData data)
		{
			bool valid = false;
			if (completion == CompletionState.NotStarted || completion == CompletionState.InProgress)
			{
				if (currentSelectionProcedure.NotifyDataSelected(data))
				{
					completion = CompletionState.InProgress;
					valid = true;
				}
				else
				{
					completion = CompletionState.Failed;
				}

				if (currentSelectionProcedure.IsComplete())
				{
					completion = CompletionState.Succeeded;
				}
			}
			return valid;
		}

		public DisplayData[] GetDataSet()
		{
			return currentSelectionProcedure.GetDataSet();
		}

		public string GetInstructionText()
		{
			return currentSelectionProcedure.GetInstructionText();
		}

		public CompletionState GetCompletionState()
		{
			return completion;
		}
	}
}
