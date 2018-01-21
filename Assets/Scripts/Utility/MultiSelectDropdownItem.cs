using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiSelectDropdownItem : MonoBehaviour, IPointerClickHandler
{
	public class Data
	{
		public string Name;
		public bool IsSelected;
	}

	private Data data;
	private MultiSelectDropdown parentDropdown;
	private int itemIndex;

	public Text label;
	public GameObject checkmark;

	public bool IsSelected
	{
		get
		{
			return data.IsSelected;
		}

		set
		{
			data.IsSelected = value;
			if (data.IsSelected)
			{
				checkmark.SetActive(true);
			}
			else
			{
				checkmark.SetActive(false);
			}
		}
	}

	public void Initialize(Data data, MultiSelectDropdown parentDropdown, int itemindex)
	{
		this.data = new Data();
		this.data.Name = data.Name;
		this.data.IsSelected = data.IsSelected;

		this.IsSelected = data.IsSelected;
		label.text = data.Name;

		this.parentDropdown = parentDropdown;
		this.itemIndex = itemindex;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		parentDropdown.ItemClicked(this, itemIndex);
	}
}