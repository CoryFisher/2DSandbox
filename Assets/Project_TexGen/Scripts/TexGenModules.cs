using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TextureGenerationType
{
	Default,
	RandomFill,
	RandomFillFromRandomGridPoints,
	Geometric,

	Count
}

class TexGenModule
{
	public List<Color> ColorPalette { get; set; }

	public Texture2D CreateTexture(int width, int height)
	{
		Color[] colors = new Color[width * height];
		Fill(colors, width, height);
		return CreateTexFromColors(colors, width, height);
	}

	private static Texture2D CreateTexFromColors(Color[] colors, int width, int height)
	{
		var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		tex.SetPixels(colors);
		tex.Apply();
		return tex;
	}

	protected virtual void Fill(Color[] colors, int width, int height)
	{
		for (int i = 0; i < colors.Length; ++i)
		{
			colors[i] = Color.white;
		}
	}
}

class TexGenRandomFill : TexGenModule
{
	protected override void Fill(Color[] colors, int width, int height)
	{
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				int index = (y * width) + x;
				colors[index] = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
			}
		}
	}
}

class TexGenGridPoints : TexGenModule
{
	public int StepSize { get; set; }

	protected override void Fill(Color[] colors, int width, int height)
	{
		int heightStep = height / StepSize;
		int widthStep = width / StepSize;

		for (int y = 0; y < height; y += heightStep)
		{
			for (int x = 0; x < width; x += widthStep)
			{
				int index = (y * width) + x;
				colors[index] = ColorPalette[UnityEngine.Random.Range(0, ColorPalette.Count)];
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
}

class TexGenGeometric : TexGenModule
{
	public enum GeometricType
	{
		Triangle,
		Square,
	}

	public GeometricType Type { get; set; }

	protected override void Fill(Color[] colors, int width, int height)
	{

	}
}