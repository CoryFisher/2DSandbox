using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
	MazeObject currentMazeObject;
	bool isGenerating;
	
	public bool IsGenerating()
	{
		return isGenerating;
	}

	public MazeObject GetMazeObject()
	{
		isGenerating = false;
		return currentMazeObject;
	}

	public void CreateNewMaze(int numColumns, int numRows, GameObject gridCellPrefab, Vector3 worldPosition)
	{
		//Debug.Log("MazeGenerator :: CreateNewMaze()");

		GameObject obj = new GameObject("Maze_" + numColumns + "x" + numRows);

		currentMazeObject = obj.AddComponent<MazeObject>();
		currentMazeObject.SetPosition(worldPosition);
		currentMazeObject.SetDimensions(numColumns, numRows);
		currentMazeObject.SetCellPrefab(gridCellPrefab);
		currentMazeObject.CreateMaze();
	}

	public IEnumerator CoCreateNewMaze(int numColumns, int numRows, GameObject gridCellPrefab, Vector3 worldPosition)
	{
		//Debug.Log("MazeGenerator :: CoCreateNewMaze()");
		isGenerating = true;

		GameObject obj = new GameObject("Maze_" + numColumns + "x" + numRows);
		
		currentMazeObject = obj.AddComponent<MazeObject>();
		currentMazeObject.SetPosition(worldPosition);
		currentMazeObject.SetDimensions(numColumns, numRows);
		currentMazeObject.SetCellPrefab(gridCellPrefab);

		var co = currentMazeObject.CoCreateMaze();
		while (co.MoveNext()) 
		{
			yield return null;
		}
	}
}
