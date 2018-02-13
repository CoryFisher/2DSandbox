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

public class MazeSolver : MonoBehaviour
{
	MazePath path;
	MazeObject mazeObject;
	bool isSolving;
	
	public MazePath GetPath()
	{
		isSolving = false;
		return path;
	}

	public MazeObject GetMazeObject()
	{
		return mazeObject;
	}

	public bool IsSolving()
	{
		return isSolving;
	}

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

		foreach (var cell in path.GetPathReverse())
		{
			cell.SetCellAttribute(CellAttribute.ShortestPath, true);
			yield return null;
		}

		//Debug.Log("MazeSolver :: Finished Solving()");
	}
}
