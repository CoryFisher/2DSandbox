using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextureButton : MonoBehaviour, IPointerClickHandler
{
	public Image UIimage;
	public Text UItext;
	public GameObject selectedBackground;
	private Texture2D texture;

	public void SetData(Texture2D tex, string texName)
	{
		texture = tex;
		UIimage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
		UItext.text = texName;
	}

	public Texture2D GetTexture()
	{
		return texture;
	}

	public void SetTexture(Texture2D tex)
	{
		texture = tex;
		UIimage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
	}

	public void SetSelected()
	{
		selectedBackground.SetActive(true);
	}

	public void SetUnSelected()
	{
		selectedBackground.SetActive(false);
	}
	
	public void OnPointerClick(PointerEventData eventData)
	{
		TextureGenerator.Get().SelectTextureButton(this);
	}
}
