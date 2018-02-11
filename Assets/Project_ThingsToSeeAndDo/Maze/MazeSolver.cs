using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePath
{
	List<MazeCellData> path = new List<MazeCellData>();

	public void AddCell(MazeCellData cell)
	{
		path.Add(cell);
	}

	public IEnumerable<MazeCellData> GetPathReverse()
	{
		for (int i = path.Count - 1; i >= 0; --i)
		{
			yield return path[i];
		}
	}
}

public class MazeSolver : Singleton<MazeSolver>
{
	MazePath path;
	MazeObject mazeObject;
	bool finished = true;

	private void Awake()
	{
		RegisterSingletonInstance(this);
	}

	public void CalculateShortestPath(MazeObject mazeObject)
	{
		this.mazeObject = mazeObject;
		finished = false;
		StartCoroutine("CoCalculateShortestPath");
	}

	public bool FinishedCalculating()
	{
		return finished;
	}

	public MazePath GetPath()
	{
		return path;
	}

	IEnumerator CoCalculateShortestPath()
	{
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
				foundEnd = true;
				break;
			}

			foreach (Direction dir in Enum.GetValues(typeof(Direction)))
			{
				if (current.GetWallIsOpen(dir))
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
			current.SetVisited(true);
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

		foreach (var cell in path.GetPathReverse())
		{
			cell.SetIsOnShortestPath(true);
			yield return null;
		}

		finished = true;
	}
}
