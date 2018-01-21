using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerationTypeDropdown : MonoBehaviour
{
	private Dropdown dropdown;

	private void Awake()
	{
		dropdown = GetComponent<Dropdown>();
		dropdown.ClearOptions();
		var options = new List<Dropdown.OptionData>();

		string[] texGenTypeNames = System.Enum.GetNames(typeof(TextureGenerationType));
		for (int i = 0; i < texGenTypeNames.Length - 1; i++)
		{
			var option = new Dropdown.OptionData(texGenTypeNames[i]);
			options.Add(option);
		}
		
		dropdown.options = options;
		dropdown.onValueChanged.AddListener(OnDropdownValueChange);
	}
	
	public void OnDropdownValueChange(int selected)
	{
		Debug.Log("OnDropdownValueChange : " + selected);
		var newType = (TextureGenerationType)selected;
		TextureGenerator.Get().SetTexGenType(newType);
	}
}
