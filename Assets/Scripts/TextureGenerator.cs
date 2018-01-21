using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum TextureGenerationType
{
	RandomFill,
	RandomFillFromGridPoints,
}

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
	private int currentOutputFileNumber = 0;
	public int outputTextureWidth = 1000;
	public int outputTextureHeight = 1000;
	public TextureGenerationType currentGenerationType;

	// UI
	public GameObject textureButtonPrefab;
	public GameObject textureButtonsContentContainer;
	public TextureButton selectedTextureButton;

	// Color Palette
	private List<Color> colorPalette = new List<Color>();
	
	public int gridSize = 10;

	private void Start()
	{
		RegisterSingletonInstance(this);

		colorPalette.Add(Color.blue);
		colorPalette.Add(Color.green);
		colorPalette.Add(Color.red);

		outputDirectoryPath = Application.dataPath + "/" + outputDirectoryName;
		currentOutputFileNumber = GetGreatestOutputFileNumber();
		Debug.Log("currentOutputFileNumber: " + currentOutputFileNumber);

		LoadOutputTextures();
	}

	public void GenerateTextures()
	{
		GenerateTexturesInternal(currentGenerationType);
	}

	private void GenerateTexturesInternal(TextureGenerationType generationType)
	{	
		switch (generationType)
		{
			case TextureGenerationType.RandomFill:
				{
					Color[] colors = new Color[outputTextureWidth * outputTextureHeight];
					FillWithRandom(ref colors, outputTextureWidth, outputTextureHeight);
					SaveTextureToFile(CreateTexFromColors(colors));
					break;
				}
			case TextureGenerationType.RandomFillFromGridPoints:
				{
					Color[] colors = new Color[outputTextureWidth * outputTextureHeight];
					FillGridPointsWithRandom(ref colors, outputTextureWidth, outputTextureHeight, 10);
					SaveTextureToFile(CreateTexFromColors(colors));
					break;
				}
		}
		
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
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				int index = (y * width) + x;
				colors[index] = colorPalette[Random.Range(0, colorPalette.Count)];
			}
		}
	}

	private void FillGridPointsWithRandom(ref Color[] colors, int width, int height, int stepSize)
	{
		Debug.Log("Filling in pixels...");
		int heightStep = height / stepSize;
		int widthStep = width / stepSize;

		for (int y = 0; y < height; y += heightStep)
		{
			for (int x = 0; x < width; x += widthStep)
			{
				int index = (y * width) + x;
				colors[index] = colorPalette[Random.Range(0, colorPalette.Count)];
			}
		}

		for (int y = 0; y < height; ++y)
		{
			int prevY = (y / (heightStep)) * heightStep;
			int nextY = (prevY + (heightStep)) % height;
			float tY = (float)(y % heightStep) / (float)(heightStep);

			for (int x = 0; x < width; ++x)
			{
				int prevX = (x / (widthStep)) * widthStep;
				int nextX = (prevX + (widthStep)) % width;
				Color prevXColor = Color.Lerp(colors[prevX + (prevY * width)], colors[prevX + (nextY * width)], tY);
				Color nextXColor = Color.Lerp(colors[nextX + (prevY * width)], colors[nextX + (nextY * width)], tY);
				float tX = (float)(x % widthStep) / (float)(widthStep);
				int index = (y * width) + x;
				colors[index] = Color.Lerp(prevXColor, nextXColor, tX);
			}
		}
	}
	
	private void SaveTextureToFile(Texture2D texture)
	{	
		Debug.Log("Saving texture to file...");
		var bytes = texture.EncodeToPNG();
		string outputFileName = outputFileBaseName + outputFileJoiner + currentOutputFileNumber.ToString() + outputFileExtension;
		currentOutputFileNumber++;
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
}
