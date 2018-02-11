using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
	Up,
	Right,
	Down,
	Left,
}

[Flags]
public enum DirectionFlags
{
	Up = 8,
	Right = 4,
	Down = 2,
	Left = 1,
}

public class DirectionHelper
{
	public static Direction OppositeOf(Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				return Direction.Down;
			case Direction.Right:
				return Direction.Left;
			case Direction.Down:
				return Direction.Up;
			case Direction.Left:
				return Direction.Right;
		}
		return Direction.Up;
	}
}

public class MazeCellObject : MonoBehaviour
{
	public static Color unvisitedColor = Color.white;
	public static Color visitedColor = Color.magenta;

	public static Color minDistFromStartColor = Color.green;
	public static Color maxDistFromStartColor = Color.red;
	
	public static Color shortestPathColor = Color.yellow;
	public static Color endCellColor = Color.black;

	public SpriteRenderer cellSpriteRenderer;
	public SpriteRenderer wallSpriteRenderer;

	MazeCellData data;

	public void SetData(MazeCellData data)
	{
		this.data = data;
		UpdateWallSprite();
		UpdateCellSprite();
	}

	public void UpdateWallSprite()
	{
		if (data != null)
		{
			// update sprite
			int spriteIndex = 0;
			if (data.GetWallIsOpen(Direction.Up))
			{
				spriteIndex |= (int)DirectionFlags.Up;
			}
			if (data.GetWallIsOpen(Direction.Right))
			{
				spriteIndex |= (int)DirectionFlags.Right;
			}
			if (data.GetWallIsOpen(Direction.Down))
			{
				spriteIndex |= (int)DirectionFlags.Down;
			}
			if (data.GetWallIsOpen(Direction.Left))
			{
				spriteIndex |= (int)DirectionFlags.Left;
			}
			wallSpriteRenderer.sprite = MazeGenerator.Get().GetCellWallSprite(spriteIndex);
		}
	}

	public void UpdateCellSprite()
	{
		if (data != null)
		{
			float t = (float)data.GetDistanceFromStart() / (float)data.GetMaxDistanceFromStart();
			cellSpriteRenderer.color = Color.Lerp(minDistFromStartColor, maxDistFromStartColor, t);

			if (data.Visited())
			{
				cellSpriteRenderer.color = visitedColor;
			}

			if (data.GetIsOnShortestPath())
			{
				cellSpriteRenderer.color = shortestPathColor;
			}

			if (data.IsEndCell())
			{
				cellSpriteRenderer.color = endCellColor;
			}
		}
	}
	
	public void DrawLinesToNieghbors()
	{
		if (data != null)
		{
			var upNeighbor = data.GetNeighbor(Direction.Up);
			if (upNeighbor != null)
			{
				var neighborObj = upNeighbor.GetParentObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(transform.position, neighborObj.transform.position, Color.red);
				}
			}
			var rightNeighbor = data.GetNeighbor(Direction.Right);
			if (rightNeighbor != null)
			{
				var neighborObj = rightNeighbor.GetParentObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(transform.position, neighborObj.transform.position, Color.yellow);
				}
			}
			var downNeighbor = data.GetNeighbor(Direction.Down);
			if (downNeighbor != null)
			{
				var neighborObj = downNeighbor.GetParentObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(transform.position, neighborObj.transform.position, Color.green);
				}
			}
			var leftNeighbor = data.GetNeighbor(Direction.Left);
			if (leftNeighbor != null)
			{
				var neighborObj = leftNeighbor.GetParentObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(transform.position, neighborObj.transform.position, Color.blue);
				}
			}
		}
	}

	private void OnMouseDown()
	{
		
	}
}

public class MazeCellData
{
	MazeCellData[] neighbors = new MazeCellData[4];
	bool[] walls = new bool[4];
	MazeCellObject parent;
	
	// maze generation data
	bool visited;
	int distanceFromStart;
	MazeCellData solverParent;
	bool isOnShortestPath;
	bool isEndCell = false;
	public int maxDistanceFromStart;
	

	public MazeCellData()
	{
		walls[0] = true;
		walls[1] = true;
		walls[2] = true;
		walls[3] = true;
		visited = false;
	}

	public void SetParentObject(MazeCellObject parent)
	{
		this.parent = parent;
	}

	public MazeCellObject GetParentObject()
	{
		return parent;
	}

	public void SetNeighbor(Direction dir, MazeCellData neighbor)
	{
		neighbors[(int)dir] = neighbor;
	}

	public MazeCellData GetNeighbor(Direction dir)
	{
		return neighbors[(int)dir];
	}

	public bool HasNeighbor(Direction dir)
	{
		return neighbors[(int)dir] != null;
	}

	public void SetWallIsOpen(Direction dir, bool open)
	{
		walls[(int)dir] = !open;

		if (parent != null)
		{
			parent.UpdateWallSprite();
		}
	}

	public bool GetWallIsOpen(Direction dir)
	{
		return walls[(int)dir] == false;
	}

	public bool HasUnvisitedNeighbors()
	{
		for (int i = 0; i < neighbors.Length; ++i)
		{
			if (neighbors[i] != null && neighbors[i].Visited() == false)
			{
				return true;
			}
		}
		return false;
	}

	public void SetVisited(bool visited)
	{
		this.visited = visited;
		if (parent != null)
		{
			parent.UpdateCellSprite();
		}
	}

	public bool Visited()
	{
		return visited;
	}

	public void SetDistanceFromStart(int dist)
	{
		distanceFromStart = dist;

		if (parent != null)
		{
			parent.UpdateCellSprite();
		}
	}

	public int GetDistanceFromStart()
	{
		return distanceFromStart;
	}

	public void SetMaxDistanceFromStart(int dist)
	{
		maxDistanceFromStart = dist;
		if (parent != null)
		{
			parent.UpdateCellSprite();
		}
	}

	public int GetMaxDistanceFromStart()
	{
		return maxDistanceFromStart;
	}

	public void SetSolverParent(MazeCellData cell)
	{
		if (cell != neighbors[0] &&
			cell != neighbors[1] &&
			cell != neighbors[2] &&
			cell != neighbors[3])
		{
			Debug.LogError("cell must be a neighbor to be a solver parent");
		}
		solverParent = cell;
	}

	public MazeCellData GetSolverParent()
	{
		return solverParent;
	}

	public void SetIsOnShortestPath(bool isOnShortestPath)
	{
		this.isOnShortestPath = isOnShortestPath;
		if (parent != null)
		{
			parent.UpdateCellSprite();
		}
	}

	public bool GetIsOnShortestPath()
	{
		return isOnShortestPath;
	}

	public void SetIsEndCell(bool isEndCell)
	{
		this.isEndCell = isEndCell;
		if (parent != null)
		{
			parent.UpdateCellSprite();
		}
	}

	public bool IsEndCell()
	{
		return isEndCell;
	}
}