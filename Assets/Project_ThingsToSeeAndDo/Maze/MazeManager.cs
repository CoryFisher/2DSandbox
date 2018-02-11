using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : Singleton<MazeManager>
{
	private bool solvingMaze;
	private bool generatingMaze;
	private MazeObject mazeObject;
	private MazePath mazePath;

	public int GridSize = 20;
	public GameObject gridCellPrefab;
	public Sprite[] CellWallSprites;

	private void Awake()
	{
		RegisterSingletonInstance(this);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Camera.main.orthographicSize = GridSize / 2f;

			if (mazeObject != null)
			{
				Destroy(mazeObject.gameObject);
			}

			MazeGenerator.Get().CreateNewMaze(GridSize, GridSize, gridCellPrefab);
			mazeObject = MazeGenerator.Get().GetMazeObject();

			MazeSolver.Get().CalculateShortestPath(mazeObject);
			mazePath = MazeSolver.Get().GetPath();
		}
		else if (Input.GetKeyDown(KeyCode.Return) && !generatingMaze && !solvingMaze)
		{
			Camera.main.orthographicSize = GridSize / 2f;

			if (mazeObject != null)
			{
				Destroy(mazeObject.gameObject);
			}

			MazeGenerator.Get().CreateNewMaze(GridSize, GridSize, gridCellPrefab, true);
			generatingMaze = true;
		}

		if (generatingMaze && MazeGenerator.Get().FinishedGenerating())
		{
			mazeObject = MazeGenerator.Get().GetMazeObject();
			MazeSolver.Get().CalculateShortestPath(mazeObject, true);
			generatingMaze = false;
			solvingMaze = true;
		}

		if (solvingMaze && MazeSolver.Get().FinishedCalculating())
		{
			mazePath = MazeSolver.Get().GetPath();
			solvingMaze = false;
		}
	}

	public Sprite GetCellWallSprite(int index)
	{
		Debug.Assert(index >= 0 && index < CellWallSprites.Length);
		return CellWallSprites[index];
	}
}
