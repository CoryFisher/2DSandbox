using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DataDisplayObject : MonoBehaviour
{
	DisplayData data;
	DataSelectorInterfaceController owner;

	// editor
	public Image image;
	public Text text;
	public Button button;
	
	public void Initialize(DataSelectorInterfaceController owner, DisplayData data)
	{
		this.owner = owner;
		this.data = data;

		Enable();
	}

	public DisplayData GetData()
	{
		return data;
	}
	
	public void OnClick()
	{
		owner.NotifyObjectSelected(this);
	}

	public void Disable()
	{
		image.gameObject.SetActive(false);
		text.gameObject.SetActive(false);
		button.enabled = false;
	}

	public void Enable()
	{
		if (!string.IsNullOrEmpty(data.Text))
		{
			text.gameObject.SetActive(true);
			text.text = data.Text;
		}

		if (data.Sprite != null)
		{
			image.gameObject.SetActive(true);
			image.sprite = data.Sprite;
		}

		button.enabled = true;
	}
}
