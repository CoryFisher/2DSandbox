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

	public IEnumerator CreateNewMaze(int numColumns, int numRows, GameObject gridCellPrefab, Vector3 worldPosition)
	{
		Debug.Log("MazeGenerator :: CreateNewMaze()");
		isGenerating = true;

		GameObject obj = new GameObject("Maze_" + numColumns + "x" + numRows);
		
		currentMazeObject = obj.AddComponent<MazeObject>();
		currentMazeObject.SetPosition(worldPosition);
		currentMazeObject.SetDimensions(numColumns, numRows);
		currentMazeObject.SetCellPrefab(gridCellPrefab);

		var co = currentMazeObject.CreateMaze();
		while (co.MoveNext()) 
		{
			yield return null;
		}
	}
}
