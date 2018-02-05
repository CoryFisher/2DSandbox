using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DisplayData
{
	public string Name;
	public string Text;
	public Sprite Sprite;
}

public enum LetterAttribute
{
	None,
	Curvy,
	Straight,
	VerticallySymmetric,
	HorizontallySymmetric,
}

public static class LetterAttributeHelper
{	
	private static readonly string[] LetterAttributeNames =
	{
		"all",
		"curvy",
		"straight",
		"vertically symmetric",
		"horizontally symmetric",
	};

	public static string GetLetterAttributeName(LetterAttribute attr)
	{
		return LetterAttributeNames[(int)attr];
	}
}

public class DataSelectionDataController : Singleton<DataSelectionDataController>
{
	private readonly string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
	private readonly string[] curvy = { "B", "C", "D", "G", "J", "O", "P", "Q", "R", "S", "U", "W" };
	private readonly string[] straight = { "A", "E", "F", "H", "I", "K", "L", "M", "N", "T", "V", "X", "Y", "Z" };
	private readonly string[] verticallySymmetric = { "B", "C", "D", "E", "H", "I", "K", "O", "X" };
	private readonly string[] horizontallySymmetric = { "A", "H", "I", "M", "O", "T", "U", "V", "W", "X", "Y" };
	private string[][] letterAttributeStringLists;
	
	public Sprite[] LetterSprites;

	void Awake()
	{
		RegisterSingletonInstance(this);

		letterAttributeStringLists = new string[Enum.GetValues(typeof(LetterAttribute)).Length][];
		letterAttributeStringLists[(int)LetterAttribute.None] = alphabet;
		letterAttributeStringLists[(int)LetterAttribute.Curvy] = curvy;
		letterAttributeStringLists[(int)LetterAttribute.Straight] = straight;
		letterAttributeStringLists[(int)LetterAttribute.VerticallySymmetric] = verticallySymmetric;
		letterAttributeStringLists[(int)LetterAttribute.HorizontallySymmetric] = horizontallySymmetric;
	}
	
	public DisplayData[] GetAlphabetDisplayData(LetterAttribute attr)
	{
		string[] attrList = letterAttributeStringLists[(int)attr];
		var alphabetDisplayData = new DisplayData[attrList.Length];
		for (int i = 0; i < attrList.Length; ++i)
		{
			var newData = new DisplayData();
			newData.Name = attrList[i];
			newData.Sprite = LetterSprites[LetterIndex(attrList[i])];
			alphabetDisplayData[i] = newData;
		}
		return alphabetDisplayData;
	}

	public string[] GetAlphabetStrings(LetterAttribute attr)
	{
		return letterAttributeStringLists[(int)attr];
	}

	static int LetterIndex(string letter)
	{
		return letter.ToCharArray()[0] - 'A';
	}
}
