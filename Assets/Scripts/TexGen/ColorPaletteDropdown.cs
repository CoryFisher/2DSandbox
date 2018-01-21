using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPaletteDropdown : MultiSelectDropdown
{	
	private Dictionary<string, Color> colors;

	private void Awake()
	{
		colors = new Dictionary<string, Color>(10);
		colors.Add("Black", Color.black);
		colors.Add("Blue", Color.blue);
		colors.Add("Clear", Color.clear);
		colors.Add("Cyan", Color.cyan);
		colors.Add("Gray", Color.grey);
		colors.Add("Green", Color.green);
		colors.Add("Magenta", Color.magenta);
		colors.Add("Red", Color.red);
		colors.Add("White", Color.white);
		colors.Add("Yellow", Color.yellow);

		Initialize();
		OnItemClicked += ColorPaletteDropdownChanged;
		OnClosed += ColorPaletteDropdownChanged;
	}

	private void ColorPaletteDropdownChanged()
	{
		TextureGenerator.Get().SetColorPalette(GetColorPalette());
	}

	protected override void GenerateItems(ref List<MultiSelectDropdownItem.Data> items)
	{
		foreach (var color in colors)
		{
			var data = new MultiSelectDropdownItem.Data();
			data.Name = color.Key;
			data.IsSelected = true;
			items.Add(data);
		}
	}

	public List<Color> GetColorPalette()
	{
		var selectedItems = GetSelectedItems();
		List<Color> selectedColors = new List<Color>(selectedItems.Count);
		foreach (var item in selectedItems)
		{
			selectedColors.Add(colors[item]);
		}
		return selectedColors;
	}
}
