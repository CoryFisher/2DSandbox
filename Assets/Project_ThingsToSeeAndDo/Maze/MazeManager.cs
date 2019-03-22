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
	public bool ShowShortestPathSprite;

	public GameObject GridCellPrefab;
	public Sprite[] CellWallSprites;

	public Color cellDistanceStart = Color.green;
	public Color cellDistanceEnd = Color.red;
	public Sprite unvisitedSprite;
	public Sprite visitedSprite;
	public Sprite currentVisitedSprite;
	public Sprite shortestPathSprite;
	public Sprite startCellSprite;
	public Sprite endCellSprite;
	Dictionary<CellAttribute, Sprite> cellSprites = new Dictionary<CellAttribute, Sprite>();

	public Sprite FogOfWarSprite;
	public Sprite FogOfWar2Sprite;
	public Sprite PlayerEntitySprite;
	public Sprite EnemyEntitySprite;
	public Sprite HealthEntitySprite;
	public Sprite MoneyEntitySprite;
	

	private void Awake()
	{
		RegisterSingletonInstance(this);
		mazeGenerators = new List<MazeGenerator>();
		mazeSolvers = new List<MazeSolver>();
		currentRoutines = new List<RoutineAndCallback>();
		newRoutines = new List<RoutineAndCallback>();
		solvedMazes = new List<SolvedMaze>();

		cellSprites[CellAttribute.None] = unvisitedSprite;
		cellSprites[CellAttribute.CurrentVisited] = currentVisitedSprite;
		cellSprites[CellAttribute.StartCell] = startCellSprite;
		cellSprites[CellAttribute.EndCell] = endCellSprite;
		cellSprites[CellAttribute.ShortestPath] = shortestPathSprite;
		cellSprites[CellAttribute.Visited] = visitedSprite;
		//cellSprites[CellAttribute.DistanceFromStart] = currentVisitedColor;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			// Camera
			Camera.main.orthographicSize = Mathf.Max(GridSize * Mathf.Min(gridCount, GridsPerLine - 1) * 0.5f, GridSize);

			var camPos = Camera.main.transform.position;

			camPos.x = Mathf.Min(gridCount, GridsPerLine - 1) * GridSize * 0.5f;

			currentGridLine = gridCount / GridsPerLine;
			camPos.y = currentGridLine * GridSize * 0.5f;

			Camera.main.transform.position = camPos;

			// Maze
			var mazeGenerator = GetNextGenerator();

			Vector3 mazePos = new Vector3();
			mazePos.x = (gridCount % GridsPerLine) * GridSize;
			mazePos.y = currentGridLine * GridSize;

			RoutineAndCallback rac = new RoutineAndCallback();
			rac.Routine = mazeGenerator.CoCreateNewMaze(GridSize, GridSize, GridCellPrefab, mazePos);
			rac.Callback = () => OnGeneratorComplete(mazeGenerator);
			rac.TickTime = 1f / GeneratorTickSpeed;
			newRoutines.Add(rac);

			++gridCount;
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			Camera.main.orthographicSize = Mathf.Max(GridSize * Mathf.Min(gridCount, GridsPerLine - 1) * 0.5f, GridSize);

			var camPos = Camera.main.transform.position;
			camPos.x = 0f;
			camPos.y = 0f;
			Camera.main.transform.position = camPos;

			// Maze
			DateTime startTime = DateTime.Now;

			var mazeGenerator = GetNextGenerator();
			var generateMaze = mazeGenerator.CoCreateNewMaze(GridSize, GridSize, GridCellPrefab, Vector3.zero);
			while (generateMaze.MoveNext()) { }
			var mazeObject = mazeGenerator.GetMazeObject();

			MazeSolver solver = GetNextSolver();
			var solveMaze = solver.CalculateShortestPath(mazeObject);
			while (solveMaze.MoveNext()) { }
			OnSolverComplete(solver);

			DateTime endTime = DateTime.Now;
			TimeSpan completionTime = endTime - startTime;
			Debug.Log("completionTime = " + completionTime.TotalSeconds);
		}
		else if (Input.GetKeyDown(KeyCode.Tab))
		{
			Vector2Int sizeAndIterations = new Vector2Int(25, 100);
			StartCoroutine("AccumulateTimingData", sizeAndIterations);
		}
		
		TickCurrentRoutines();

		currentRoutines.AddRange(newRoutines);
		newRoutines.Clear();
	}

	public Sprite GetEntitySprite(EntityType cellEntityType)
	{
		switch (cellEntityType)
		{	
			case EntityType.Enemy:
				return EnemyEntitySprite;
			case EntityType.Health:
				return HealthEntitySprite;
			case EntityType.Money:
				return MoneyEntitySprite;
			case EntityType.Player:
				return PlayerEntitySprite;
		}
		return null;
	}

	public Sprite GetFogOfWarSprite()
	{	
		return FogOfWarSprite;
	}

	private MazeGenerator GetNextGenerator()
	{
		MazeGenerator mg = mazeGenerators.Find(x => x.IsGenerating() == false);
		if (mg == null)
		{
			mg = gameObject.AddComponent<MazeGenerator>();
			mazeGenerators.Add(mg);
		}
		return mg;
	}

	private MazeSolver GetNextSolver()
	{
		MazeSolver ms = mazeSolvers.Find(x => x.IsSolving() == false);
		if (ms == null)
		{
			mazeSolvers.Add(ms);
		}
		return ms;
	}

	private void OnGeneratorComplete(MazeGenerator mazeGenerator)
	{
		var mazeObject = mazeGenerator.GetMazeObject();

		MazeSolver solver = GetNextSolver();

		RoutineAndCallback rac = new RoutineAndCallback();
		rac.Routine = solver.CalculateShortestPath(mazeObject);
		rac.Callback = () => OnSolverComplete(solver);
		rac.TickTime = 1f / SolverTickSpeed;
		newRoutines.Add(rac);
	}

	private void OnSolverComplete(MazeSolver solver)
	{
		var mazePath = solver.GetCoPath();
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
					if (currentRoutine.TickTime == 0f)
					{
						while(currentRoutine.Routine.MoveNext())
						{
							// run to completion
						}
						continue;
					}

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

	IEnumerator AccumulateTimingData(Vector2Int dimensions)
	{
		var timingData = new TimeSpan[dimensions.y];
		var size = new Vector2Int(dimensions.x, dimensions.x);
		for (int i = 0; i < dimensions.y; ++i)
		{
			timingData[i] = CreateAndDestroyMaze(size);
			yield return null;
		}

		double totalTime = 0f;
		foreach (var time in timingData)
		{
			totalTime += time.TotalSeconds;
		}
		totalTime /= dimensions.y;
		Debug.Log("Size "+ dimensions.x + ", Iterations " + dimensions.y + ", Average Time " + totalTime);
	}

	public Color GetCellSpriteColor(int distanceFromStart, int maxDistanceFromStart)
	{
		return Color.Lerp(cellDistanceStart, cellDistanceEnd, (float)distanceFromStart / (float)maxDistanceFromStart);
	}

	public Sprite GetCellSprite(CellAttribute attr)
	{
		return cellSprites[attr];
	}

	public MazeObject CreateMaze(Vector2Int dimensions)
	{
		var mazeGenerator = GetNextGenerator();
		mazeGenerator.CreateNewMaze(dimensions.x, dimensions.y, GridCellPrefab, Vector3.zero);
		return mazeGenerator.GetMazeObject();
	}

	TimeSpan CreateAndDestroyMaze(Vector2Int dimensions)
	{
		DateTime startTime = DateTime.Now;

		var mazeGenerator = GetNextGenerator();
		var generateMaze = mazeGenerator.CoCreateNewMaze(dimensions.x, dimensions.y, GridCellPrefab, Vector3.zero);
		while (generateMaze.MoveNext()) { }
		var mazeObject = mazeGenerator.GetMazeObject();

		//MazeSolver solver = GetNextSolver();
		//var solveMaze = solver.CalculateShortestPath(mazeObject);
		//while (solveMaze.MoveNext()) { }
		//OnSolverComplete(solver);

		DateTime endTime = DateTime.Now;
		TimeSpan completionTime = endTime - startTime;
		Debug.Log("completionTime = " + completionTime.TotalSeconds);

		Destroy(mazeObject.gameObject);

		return completionTime;
	}

	TimeSpan CreateAndSolveMaze(Vector2Int dimensions)
	{
		DateTime startTime = DateTime.Now;

		var mazeGenerator = GetNextGenerator();
		var generateMaze = mazeGenerator.CoCreateNewMaze(dimensions.x, dimensions.y, GridCellPrefab, Vector3.zero);
		while (generateMaze.MoveNext()) { }
		var mazeObject = mazeGenerator.GetMazeObject();

		MazeSolver solver = GetNextSolver();
		var solveMaze = solver.CalculateShortestPath(mazeObject);
		while (solveMaze.MoveNext()) { }
		//OnSolverComplete(solver);

		DateTime endTime = DateTime.Now;
		TimeSpan completionTime = endTime - startTime;

		Destroy(mazeObject.gameObject);

		return completionTime;
	}
}

public enum Direction
{
	Up,
	Right,
	Down,
	Left,
}

[Flags]
public enum DirectionFlag
{
	Up = 8,
	Right = 4,
	Down = 2,
	Left = 1,
}