using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MazeGenerator : Singleton<MazeGenerator>
{
	MazeObject currentMazeObject;
	bool finished;
	
	private void Awake()
	{
		RegisterSingletonInstance(this);
	}

	public bool FinishedGenerating()
	{
		return finished;
	}

	public MazeObject GetMazeObject()
	{
		if (finished)
		{
			return currentMazeObject;
		}
		return null;
	}

	public void CreateNewMaze(int numColumns, int numRows, GameObject gridCellPrefab, bool async = false)
	{
		Debug.Log("MazeGenerator::CreateNewMaze()");

		finished = false;

		GameObject obj = new GameObject("Maze_" + numColumns + "x" + numRows);
		currentMazeObject = obj.AddComponent<MazeObject>();
		currentMazeObject.SetDimensions(numColumns, numRows);
		currentMazeObject.SetCellPrefab(gridCellPrefab);

		if (async)
		{
			StartCoroutine("CoGenerateNewMaze");
		}
		else
		{
			var co = currentMazeObject.CreateMaze();
			while (co.MoveNext()) { }
			finished = true;
		}
	}

	IEnumerator CoGenerateNewMaze()
	{
		yield return currentMazeObject.CreateMaze();
		finished = true;
	}
}
