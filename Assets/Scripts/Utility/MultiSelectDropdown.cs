using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiSelectDropdown : MonoBehaviour, IPointerClickHandler
{
	private bool isOpen;
	private List<MultiSelectDropdownItem.Data> items;

	// templates
	public GameObject dropdownMenu;
	public Transform dropdownItemContainer;
	public GameObject itemTemplate;

	public delegate void OnItemClickedHandler();
	public event OnItemClickedHandler OnItemClicked;

	public delegate void OnDropdownHandler();
	public event OnDropdownHandler OnOpened;
	public event OnDropdownHandler OnClosed;

	protected void Initialize()
	{
		itemTemplate.SetActive(false);
		
		// Generate item data
		items = new List<MultiSelectDropdownItem.Data>();
		GenerateItems(ref items);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (isOpen)
		{
			// Close
			foreach (Transform t in dropdownItemContainer)
			{
				if (t.gameObject != itemTemplate)
				{
					Destroy(t.gameObject);
				}
			}

			dropdownMenu.SetActive(false);
			isOpen = false;
			if (OnClosed != null)
			{
				OnClosed.Invoke();
			}
		}
		else
		{
			// Open
			for (int i = 0; i < items.Count; ++i)
			{
				GameObject item = Instantiate(itemTemplate, dropdownItemContainer);
				item.SetActive(true);
				var itemScript = item.GetComponent<MultiSelectDropdownItem>();
				itemScript.Initialize(items[i], this, i);
			}

			dropdownMenu.SetActive(true);
			isOpen = true;
			if (OnOpened != null)
			{
				OnOpened.Invoke();
			}
		}
	}

	public void ItemClicked(MultiSelectDropdownItem item, int itemIndex)
	{
		item.IsSelected = !item.IsSelected;
		items[itemIndex].IsSelected = item.IsSelected;
		if (OnItemClicked != null)
		{
			OnItemClicked.Invoke();
		}
	}

	public List<string> GetSelectedItems()
	{
		List<string> selectedItemNames = null;
		if (items != null)
		{
			selectedItemNames = new List<string>(items.Count);
			foreach (var item in items)
			{
				if (item.IsSelected)
				{
					selectedItemNames.Add(item.Name);
				}
			}
		}
		return selectedItemNames;
	}

	protected virtual void GenerateItems(ref List<MultiSelectDropdownItem.Data> items)
	{

	}
}
