using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextureGenerator : Singleton<TextureGenerator>
{
	// Output Directory
	private const string outputDirectoryName = "TextureGeneratorOutput";
	private string outputDirectoryPath;
	
	// Output Files
	private const string outputFileBaseName = "TextureGenerator";
	private const string outputFileJoiner = "_";
	private const string outputFileExtension = ".png";
	private const string metaFileExtension = ".meta";
	public int outputTextureWidth = 1000;
	public int outputTextureHeight = 1000;
	public TextureGenerationType currentGenerationType;
	
	// UI
	public GameObject textureButtonPrefab;
	public GameObject textureButtonsContentContainer;
	public TextureButton selectedTextureButton;

	// Color Palette
	private List<Color> colorPalette = new List<Color>();

	private void Start()
	{
		RegisterSingletonInstance(this);

		colorPalette.Add(Color.blue);
		colorPalette.Add(Color.green);
		colorPalette.Add(Color.red);

		outputDirectoryPath = Application.dataPath + "/" + outputDirectoryName;

		LoadOutputTextures();
	}

	public void GenerateTextures()
	{
		GenerateTexturesInternal(currentGenerationType);
	}

	private void GenerateTexturesInternal(TextureGenerationType generationType)
	{
		TexGenModule generator;
		switch (generationType)
		{
			case TextureGenerationType.RandomFill:
				{
					generator = new TexGenRandomFill() { ColorPalette = colorPalette };
					break;
				}
			case TextureGenerationType.RandomFillFromRandomGridPoints:
				{
					generator = new TexGenGridPoints() { ColorPalette = colorPalette, StepSize = 10 };
					break;
				}
			case TextureGenerationType.Geometric:
				{
					generator = new TexGenGeometric() { ColorPalette = colorPalette };
					break;
				}
			case TextureGenerationType.Default:
			default:
				{
					generator = new TexGenModule() { ColorPalette = colorPalette };
					break;
				}
		}
		var tex = generator.CreateTexture(outputTextureWidth, outputTextureHeight);
		SaveTextureToFile(tex, generationType.ToString());
		
		// Refresh textures
		ClearOutputTextures();
		LoadOutputTextures();

		Debug.Log("Done.");
	}

	public void InvertSelectedTexture()
	{
		Debug.Log("InvertSelectedTexture()");
		if (selectedTextureButton != null)
		{
			Debug.Log("selectedTextureButton != null");
			var tex = selectedTextureButton.GetTexture();
			var pixels = tex.GetPixels();
			for (int i= 0; i < pixels.Length; ++i)
			{
				pixels[i].r = 1.0f - pixels[i].r;
				pixels[i].g = 1.0f - pixels[i].g;
				pixels[i].b = 1.0f - pixels[i].b;
			}
			tex.SetPixels(pixels);
			tex.Apply();
		}
	}

	public void SaveSelectedTexture()
	{
		if (selectedTextureButton != null)
		{
			var tex = selectedTextureButton.GetTexture();

			// TODO
		}
	}

			public void SelectTextureButton(TextureButton textureButton)
	{
		if (selectedTextureButton != null)
		{
			selectedTextureButton.SetUnSelected();
		}
		selectedTextureButton = textureButton;
		if (selectedTextureButton != null)
		{
			selectedTextureButton.SetSelected();
		}
	}

	public void SetColorPalette(List<Color> newColors)
	{
		colorPalette = newColors;
		if (colorPalette == null || colorPalette.Count == 0)
		{
			colorPalette.Add(Color.white);
		}
	}

	private Texture2D CreateTexFromColors(Color[] colors)
	{
		Debug.Log("Setting texture pixels...");
		var tex = new Texture2D(outputTextureWidth, outputTextureHeight, TextureFormat.RGBA32, false);
		tex.SetPixels(colors);
		tex.Apply();
		return tex;
	}

	private void FillWithRandom(ref Color[] colors, int width, int height)
	{
		Debug.Log("Filling in pixels...");
		
	}

	private void FillGridPointsWithRandom(ref Color[] colors, int width, int height, int stepSize)
	{
		Debug.Log("Filling in pixels...");
		
	}
	
	private void SaveTextureToFile(Texture2D texture, string filename)
	{	
		Debug.Log("Saving texture to file...");
		var bytes = texture.EncodeToPNG();
		int fileNumber = 0;
		string outputFileName = filename + outputFileJoiner + fileNumber.ToString() + outputFileExtension;
		while (File.Exists(outputDirectoryPath + "/" + outputFileName))
		{
			fileNumber++;
			outputFileName = filename + outputFileJoiner + fileNumber.ToString() + outputFileExtension;
		}
		var file = File.Open(outputDirectoryPath + "/" + outputFileName, FileMode.Create);
		var binary = new BinaryWriter(file);
		binary.Write(bytes);
		file.Close();
	}

	private int GetGreatestOutputFileNumber()
	{
		int greatest = 0;
		if (Directory.Exists(outputDirectoryPath))
		{
			foreach (string file in Directory.GetFiles(outputDirectoryPath))
			{
				Debug.Log("filename: " + file);
				bool knownFileNameFormat = false;
				if (file.EndsWith(outputFileExtension))
				{
					Debug.Log("Ends with " + outputFileExtension);
					var trimmed = file.Substring(0, file.LastIndexOf(outputFileExtension));
					Debug.Log("Trimmed: " + trimmed);
					string[] fileSplit = trimmed.Split(outputFileJoiner.ToCharArray());
					foreach(var split in fileSplit)
					{
						Debug.Log("Split: " + split);
					}

					int result = 0;
					if (fileSplit.Length == 2 && int.TryParse(fileSplit[1], out result))
					{
						Debug.Log("fileSplit.Length == 2 && int.TryParse(fileSplit[1], out result)");
						knownFileNameFormat = true;
						if (result > greatest)
						{
							greatest = result;
						}
					}
				}
				else if(file.EndsWith(metaFileExtension))
				{
					knownFileNameFormat = true;
				}

				if (!knownFileNameFormat)
				{
					Debug.LogWarning("Unknown output file: " + file);
				}
			}
		}
		else
		{
			Directory.CreateDirectory(outputDirectoryPath);
		}
		return greatest;
	}

	private void ClearOutputTextures()
	{
		foreach (Transform child in textureButtonsContentContainer.transform)
		{
			Destroy(child.gameObject);
		}
	}

	private void LoadOutputTextures()
	{
		if (Directory.Exists(outputDirectoryPath))
		{
			foreach (string file in Directory.GetFiles(outputDirectoryPath))
			{
				if (file.EndsWith(outputFileExtension))
				{
					var bytes = File.ReadAllBytes(file);
					Texture2D texture2d = new Texture2D(2, 2);
					texture2d.LoadImage(bytes);

					GameObject obj = Instantiate(textureButtonPrefab);
					var btn = obj.GetComponent<TextureButton>();
					if (btn)
					{
						string name = file.Substring(file.LastIndexOf('\\') + 1).TrimEnd(outputFileExtension.ToCharArray());
						btn.SetData(texture2d, name);
					}
					obj.transform.SetParent(textureButtonsContentContainer.transform);
				}

			}
		}
	}

	public void SetTexGenType(TextureGenerationType newType)
	{
		currentGenerationType = newType;
	}
}
