using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazePath
{
	Stack<MazeCellData> path = new Stack<MazeCellData>();

	public void AddCell(MazeCellData cell)
	{
		path.Push(cell);
	}

	public IEnumerable<MazeCellData> GetPath()
	{
		return path;
	}

	public bool Contains(MazeCellData cell)
	{
		return path.Contains(cell);
	}
}

public class MazeSolver
{
	// Breadth-first search and set parent style solving

	public static MazePath SolveToGetPathStartToEnd(MazeObject maze)
	{
		MazeData mazeData = maze.GetMazeData();
		MazeCellData start = mazeData.GetStartCell();
		MazeCellData end = mazeData.GetEndCell();
		
		SolveFromPoint(maze, start, end);
		return GetCurrentPathToPoint(end);
	}

	public static MazePath SolveToGetPath(MazeObject maze, MazeCellData start, MazeCellData end)
	{	
		SolveFromPoint(maze, start, end);
		return GetCurrentPathToPoint(end);
	}

	static MazePath GetCurrentPathToPoint(MazeCellData point)
	{
		var returnPath = new MazePath();
		returnPath.AddCell(point);
		MazeCellData solverParent = point.GetSolverParent();
		while (solverParent != null)
		{
			returnPath.AddCell(solverParent);
			solverParent = solverParent.GetSolverParent();
		}
		return returnPath;
	}

	static void SolveFromPoint(MazeObject maze, MazeCellData point, MazeCellData end = null)
	{
		// todo  add a bool for "calculateDistanceFromStart"

		// clear solver parents lol
		var cells = maze.GetMazeData().GetCells();
		foreach (var cell in cells)
		{
			cell.SetSolverParent(null);
		}

		Queue<MazeCellData> cellsToCheck = new Queue<MazeCellData>();
		List<MazeCellData> visited = new List<MazeCellData>();
		
		cellsToCheck.Enqueue(point);

		while (cellsToCheck.Count > 0)
		{
			MazeCellData current = cellsToCheck.Dequeue();
			
			if (end != null && current == end)
			{
				return;
			}
			
			foreach (Direction dir in Enum.GetValues(typeof(Direction)))
			{
				if (!current.HasWall(dir))
				{
					var neighbor = current.GetNeighbor(dir);
					if (!visited.Contains(neighbor))
					{
						neighbor.SetSolverParent(current);
						cellsToCheck.Enqueue(neighbor);
					}
				}
			}

			visited.Add(current);
		}
	}



	// State machine for mazerunner auto movement
	MazeObject maze;
	List<MazeCellData> cellsToCheck = new List<MazeCellData>();
	List<MazeCellData> visited = new List<MazeCellData>();
	MazePath shortestPathToEnd;

	public void InitForMaze(MazeObject maze)
	{
		Debug.Log("MazeSolver :: InitForMaze()");
		this.maze = maze;

		shortestPathToEnd = SolveToGetPathStartToEnd(maze);
		
		cellsToCheck.Clear();
		visited.Clear();
		
		cellsToCheck.Add(maze.GetMazeData().GetStartCell());
	}

	public MazeCellData GetNextCellToVisit()
	{
		if (cellsToCheck.Count > 0)
		{
			return cellsToCheck[0];
		}
		return null;
	}

	public void NotifyCellVisited(MazeCellData cell, bool breadthFirst)
	{
		Debug.Log("MazeSolver :: NotifyCellVisited()");
		if (cellsToCheck.Contains(cell))
		{
			cellsToCheck.Remove(cell);
			visited.Add(cell);

			if (breadthFirst)
			{
				foreach (Direction dir in Enum.GetValues(typeof(Direction)))
				{
					if (!cell.HasWall(dir))
					{
						var neighbor = cell.GetNeighbor(dir);
						if (!visited.Contains(neighbor))
						{
							neighbor.SetSolverParent(cell);
							cellsToCheck.Add(neighbor);
						}
					}
				}
			}
			else
			{	
				DepthFirstSearch(cell);				
			}
		}
	}

	void DepthFirstSearch(MazeCellData cell)
	{
		// add side-routes to completion first
		foreach (var neighbor in cell.GetConnectedNeighbors())
		{
			if (!visited.Contains(neighbor) &&
				!cellsToCheck.Contains(neighbor) &&
				!shortestPathToEnd.Contains(neighbor))
			{
				neighbor.SetSolverParent(cell);
				cellsToCheck.Add(neighbor);
				DepthFirstSearch(neighbor);
			}
		}

		// then add shortest path cell
		foreach (var neighbor in cell.GetConnectedNeighbors())
		{
			if (!visited.Contains(neighbor) &&
				!cellsToCheck.Contains(neighbor))
			{
				neighbor.SetSolverParent(cell);
				cellsToCheck.Add(neighbor);
			}
		}
	}




	// Some thing about coroutines and stepping through manually lol

	MazeObject mazeObject;
	bool isSolving;
	MazePath path;
	public IEnumerator CalculateShortestPath(MazeObject mazeObject)
	{
		this.mazeObject = mazeObject;

		//Debug.Log("MazeSolver :: CalculateShortestPath()");

		isSolving = true;

		MazeData mazeData = mazeObject.GetMazeData();

		MazeCellData start = mazeData.GetStartCell();
		MazeCellData end = mazeData.GetEndCell();

		Queue<MazeCellData> cellsToCheck = new Queue<MazeCellData>();
		cellsToCheck.Enqueue(start);

		List<MazeCellData> visited = new List<MazeCellData>();

		bool foundEnd = false;
		while (!foundEnd && cellsToCheck.Count > 0)
		{
			MazeCellData current = cellsToCheck.Dequeue();

			if (current == end)
			{
				if (visited.Count > 0)
				{
					visited[visited.Count - 1].SetCellAttribute(CellAttribute.CurrentVisited, false);
				}
				foundEnd = true;
				break;
			}

			foreach (Direction dir in Enum.GetValues(typeof(Direction)))
			{
				if (!current.HasWall(dir))
				{
					var neighbor = current.GetNeighbor(dir);
					if (!visited.Contains(neighbor))
					{
						neighbor.SetSolverParent(current);
						cellsToCheck.Enqueue(neighbor);
					}
				}
			}

			if (visited.Count > 0)
			{
				visited[visited.Count - 1].SetCellAttribute(CellAttribute.CurrentVisited, false);
			}
			current.SetCellAttribute(CellAttribute.CurrentVisited, true);
			current.SetCellAttribute(CellAttribute.Visited, true);

			visited.Add(current);
			yield return null;
		}

		if (foundEnd)
		{
			path = new MazePath();
			path.AddCell(end);
			MazeCellData solverParent = end.GetSolverParent();
			while (solverParent != null)
			{
				path.AddCell(solverParent);
				solverParent = solverParent.GetSolverParent();
			}
		}

		foreach (var cell in path.GetPath())
		{
			cell.SetCellAttribute(CellAttribute.ShortestPath, true);
			yield return null;
		}

		//Debug.Log("MazeSolver :: Finished Solving()");
	}

	internal bool IsSolving()
	{
		return isSolving;
	}

	internal MazePath GetCoPath()
	{
		return path;
	}

	internal MazeObject GetMazeObject()
	{
		return mazeObject;
	}
}
