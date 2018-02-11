using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MazeGenerator : Singleton<MazeGenerator>
{
	bool accumulatingCompletionData;
	MazeObject mazeObject = null;
	bool generatingMaze = false;
	bool solvingMaze = false;
	bool finished = false;
	bool doTheDeed = false;

	public GameObject gridCellPrefab;
	public Sprite[] CellWallSprites;
	public bool UseDebugControlKeys = false;
	public int GridSize = 20;

	
	private void Awake()
	{
		RegisterSingletonInstance(this);
	}

	private void Update()
	{
		if (UseDebugControlKeys)
		{
			if (Input.GetKeyDown(KeyCode.Space) && !generatingMaze && !solvingMaze)
			{
				doTheDeed = !doTheDeed;
			}
		}

		if (!generatingMaze && !solvingMaze && doTheDeed)
		{
			if (mazeObject != null)
			{
				Destroy(mazeObject.gameObject);
				mazeObject = null;
			}

			CreateNewMaze(GridSize, GridSize);
		}

		if (generatingMaze && finished)
		{
			generatingMaze = false;
			solvingMaze = true;

			MazeSolver.Get().CalculateShortestPath(mazeObject);
		}

		if (solvingMaze && MazeSolver.Get().FinishedCalculating())
		{
			solvingMaze = false;
		}
	}

	public void CreateNewMaze(int numColumns, int numRows)
	{
		GameObject obj = new GameObject("Maze_" + numColumns + "x" + numRows);
		mazeObject = obj.AddComponent<MazeObject>();
		
		GenerateMazeData data;
		data.columns = numColumns;
		data.rows = numRows;

		finished = false;
		generatingMaze = true;

		StartCoroutine("CoGenerateNewMaze", data);
	}

	struct GenerateMazeData	{ public int columns; public int rows; }
	IEnumerator CoGenerateNewMaze(GenerateMazeData data)
	{
		yield return mazeObject.CreateMaze(data.columns, data.rows, gridCellPrefab);
		finished = true;
	}

	public Sprite GetCellWallSprite(int index)
	{
		Debug.Assert(index >=0 && index < CellWallSprites.Length);
		return CellWallSprites[index];
	}
}
