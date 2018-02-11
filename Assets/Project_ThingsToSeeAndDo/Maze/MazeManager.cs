using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : Singleton<MazeManager>
{
	class RoutineAndCallback
	{
		public IEnumerator Routine;
		public Action Callback;
		public float Timer;
		public float TickTime;
	}

	class SolvedMaze
	{
		public MazeObject Maze;
		public MazePath Path;
	}

	private List<MazeGenerator> mazeGenerators;
	private List<MazeSolver> mazeSolvers;
	private List<RoutineAndCallback> currentRoutines;
	private List<RoutineAndCallback> newRoutines;
	private List<SolvedMaze> solvedMazes;
	private int gridCount;
	private int currentGridLine;

	public int GridSize = 25;
	public int GridsPerLine = 6;
	public float GeneratorTickSpeed = 100f;
	public float SolverTickSpeed = 100f;

	public GameObject GridCellPrefab;
	public Sprite[] CellWallSprites;

	private void Awake()
	{
		RegisterSingletonInstance(this);
		mazeGenerators = new List<MazeGenerator>();
		mazeSolvers = new List<MazeSolver>();
		currentRoutines = new List<RoutineAndCallback>();
		newRoutines = new List<RoutineAndCallback>();
		solvedMazes = new List<SolvedMaze>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			// Camera
			Camera.main.orthographicSize = Mathf.Max(GridSize * Mathf.Min(gridCount, GridsPerLine-1) * 0.5f, GridSize);

			var camPos = Camera.main.transform.position;

			camPos.x = Mathf.Min(gridCount, GridsPerLine-1) * GridSize * 0.5f;

			currentGridLine = gridCount / GridsPerLine;
			camPos.y = currentGridLine * GridSize * 0.5f;

			Camera.main.transform.position = camPos;

			// Maze
			var mazeGenerator = mazeGenerators.Find(x => x.IsGenerating() == false);
			if (mazeGenerator == null)
			{
				mazeGenerator = gameObject.AddComponent<MazeGenerator>();
				mazeGenerators.Add(mazeGenerator);
			}
			
			Vector3 mazePos = new Vector3();
			mazePos.x = (gridCount % GridsPerLine) * GridSize;
			mazePos.y = currentGridLine * GridSize;

			RoutineAndCallback rac = new RoutineAndCallback();
			rac.Routine = mazeGenerator.CreateNewMaze(GridSize, GridSize, GridCellPrefab, mazePos);
			rac.Callback = () => OnGeneratorComplete(mazeGenerator);
			rac.TickTime = 1f / GeneratorTickSpeed;
			newRoutines.Add(rac);

			++gridCount;
		}
		
		TickCurrentRoutines();

		currentRoutines.AddRange(newRoutines);
		newRoutines.Clear();
	}

	private void OnGeneratorComplete(MazeGenerator mazeGenerator)
	{
		var mazeObject = mazeGenerator.GetMazeObject();

		MazeSolver solver = mazeSolvers.Find(x => x.IsSolving() == false);
		if (solver == null)
		{
			solver = gameObject.AddComponent<MazeSolver>();
			mazeSolvers.Add(solver);
		}

		RoutineAndCallback rac = new RoutineAndCallback();
		rac.Routine = solver.CalculateShortestPath(mazeObject);
		rac.Callback = () => OnSolverComplete(solver);
		rac.TickTime = 1f / SolverTickSpeed;
		newRoutines.Add(rac);
	}

	private void OnSolverComplete(MazeSolver solver)
	{
		var mazePath = solver.GetPath();
		var mazeObject = solver.GetMazeObject();

		SolvedMaze solvedMaze = new SolvedMaze();
		solvedMaze.Maze = mazeObject;
		solvedMaze.Path = mazePath;
		solvedMazes.Add(solvedMaze);
	}

	void TickCurrentRoutines()
	{
		if (currentRoutines.Count > 0)
		{
			// for each routine
			for (int routineindex = 0; routineindex < currentRoutines.Count; ++routineindex)
			{
				var currentRoutine = currentRoutines[routineindex];

				if (currentRoutine.Routine != null)
				{
					// tick the timer
					currentRoutine.Timer += Time.deltaTime;

					if (currentRoutine.Timer > currentRoutine.TickTime)
					{
						int numTicks = (int)(currentRoutine.Timer / currentRoutine.TickTime);
						if (numTicks > 0)
						{
							currentRoutine.Timer -= currentRoutine.TickTime * numTicks;
							for (int i = 0; i < numTicks; ++i)
							{
								if (currentRoutine.Routine.MoveNext() == false)
								{
									if (currentRoutine.Callback != null)
									{
										currentRoutine.Callback();
									}

									// prevent double calling :(
									currentRoutine.Routine = null;
									currentRoutine.Callback = null;
									break;
								}
							}
						}
					}
				}
			}
			
			currentRoutines.RemoveAll(x => x.Routine == null);
		}
	}
	
	public Sprite GetCellWallSprite(int index)
	{
		Debug.Assert(index >= 0 && index < CellWallSprites.Length);
		return CellWallSprites[index];
	}
}
