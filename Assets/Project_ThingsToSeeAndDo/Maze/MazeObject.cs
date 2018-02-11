using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeData
{
	const int InvalidIndex = -1;

	MazeCellData[] cells;
	int columns;
	int rows;

	MazeCellData startCell;
	MazeCellData endCell;

	public float OrthographicCameraSize { get; private set; }

	public MazeData(int columns, int rows)
	{
		this.columns = columns;
		this.rows = rows;
		OrthographicCameraSize = Mathf.Max(columns, rows) / 2f;

		// create array to hold cells
		cells = new MazeCellData[columns * rows];
		for (int i = 0; i < cells.Length; ++i)
		{
			MazeCellData cell = new MazeCellData();
			cells[i] = cell;
		}

		// set neighbors
		for (int row = 0; row < rows; ++row)
		{
			for (int col = 0; col < columns; ++col)
			{
				// current cell
				int index = ValidCellIndex(col, row);
				var cell = cells[index];

				// up
				index = ValidCellIndex(col, row + 1);
				if (index != InvalidIndex)
				{
					cell.SetNeighbor(Direction.Up, cells[index]);
				}

				// right
				index = ValidCellIndex(col + 1, row);
				if (index != InvalidIndex)
				{
					cell.SetNeighbor(Direction.Right, cells[index]);
				}

				// down
				index = ValidCellIndex(col, row - 1);
				if (index != InvalidIndex)
				{
					cell.SetNeighbor(Direction.Down, cells[index]);
				}

				// left
				index = ValidCellIndex(col - 1, row);
				if (index != InvalidIndex)
				{
					cell.SetNeighbor(Direction.Left, cells[index]);
				}
			}
		}
	}

	public IEnumerator GenerateMaze()
	{
		// generate maze from cells using recursive backtracking (depth-first) algorithm
		MazeCellData current = GetCell(0);
		current.SetVisited(true);
		startCell = current;
		Stack<MazeCellData> cellStack = new Stack<MazeCellData>();

		int numVisited = 1;
		int numCells = columns * rows;
		int maxDistFromStart = 0;

		// while there are still unvisited cells 
		while (numVisited < numCells)
		{
			if (current.HasUnvisitedNeighbors())
			{
				// get random neighbor
				MazeCellData next = null;
				Direction direction;
				do
				{
					direction = (Direction)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Direction)).Length);
					next = current.GetNeighbor(direction);
				} while (next == null || next.Visited());

				// push current to stack
				cellStack.Push(current);

				// remove walls between current and next
				current.SetWallIsOpen(direction, true);
				next.SetWallIsOpen(DirectionHelper.OppositeOf(direction), true);

				// dist from start
				int distFromStart = current.GetDistanceFromStart() + 1;
				next.SetDistanceFromStart(distFromStart);
				if (distFromStart > maxDistFromStart)
				{
					maxDistFromStart = distFromStart;
				}

				// make next cell current and mark as visited
				current = next;
				current.SetVisited(true);
				++numVisited;

				yield return null;
			}
			else if (cellStack.Count > 0)
			{
				current = cellStack.Pop();
			}
		}

		foreach (var cell in cells)
		{
			cell.SetVisited(false);
			cell.SetMaxDistanceFromStart(maxDistFromStart);
			if (cell.GetDistanceFromStart() == maxDistFromStart && endCell == null)
			{
				// just take the first one
				endCell = cell;
				cell.SetIsEndCell(true);
			}
		}
	}
	
	public MazeCellData GetCell(int column, int row)
	{
		int index = (this.columns * row) + column;
		if (index >= 0 && index < cells.Length)
		{
			return cells[index];
		}
		return null;
	}

	public MazeCellData GetCell(int index)
	{
		if (index >= 0 && index < cells.Length)
		{
			return cells[index];
		}
		return null;
	}

	public MazeCellData GetStartCell()
	{
		return startCell;
	}

	public MazeCellData GetEndCell()
	{
		return endCell;
	}
	
	int ValidCellIndex(int column, int row)
	{
		if (column < 0 || column >= columns)
		{
			return InvalidIndex;
		}
		if (row < 0 || row >= rows)
		{
			return InvalidIndex;
		}
		return (columns * row) + column;
	}
}

public class MazeObject : MonoBehaviour
{
	MazeCellObject[] cellObjects;
	MazeData mazeData;
	
	public IEnumerator CreateMaze(int numColumns, int numRows, GameObject cellObjectPrefab)
	{
		// create dummy cell object to get its information
		var cellObj = Instantiate(cellObjectPrefab);
		var cellScript = cellObj.GetComponent<MazeCellObject>();

		float cellPixelWidth = cellScript.cellSpriteRenderer.sprite.texture.width;
		float pixelsPerUnit = cellScript.cellSpriteRenderer.sprite.pixelsPerUnit;
		float cellWorldWidth = cellPixelWidth / pixelsPerUnit;
		float cellPlacementWorldOffset = cellWorldWidth * 0.75f;

		float gridWidth = (numColumns * cellPlacementWorldOffset) + (cellWorldWidth * 0.25f);
		float minWorldPosX = gridWidth * -0.5f;
		float gridHeight = (numRows * cellPlacementWorldOffset) + (cellWorldWidth * 0.25f);
		float minPosY = gridHeight * -0.5f;

		// destroy dummy cell
		Destroy(cellObj);
		cellObj = null;
		cellScript = null;

		// create maze data
		mazeData = new MazeData(numColumns, numRows);
		Camera.main.orthographicSize = mazeData.OrthographicCameraSize;
		cellObjects = new MazeCellObject[numColumns * numRows];

		// create cell objects and fill with maze data
		for (int row = 0; row < numRows; ++row)
		{
			for (int col = 0; col < numColumns; ++col)
			{
				// create cell object
				cellObj = Instantiate(cellObjectPrefab);
				// place object in world space
				cellObj.transform.position = new Vector3(minWorldPosX + (col * cellPlacementWorldOffset), minPosY + (row * cellPlacementWorldOffset), 0);
				// child under this gameObject
				cellObj.transform.SetParent(transform);
				// get cell data and object scripts
				var cellObjScript = cellObj.GetComponent<MazeCellObject>();
				var cellDataScript = mazeData.GetCell(col, row);
				cellObjects[(row * numColumns) + col] = cellObjScript;

				cellDataScript.SetParentObject(cellObjScript);
				cellObjScript.SetData(cellDataScript);
			}
		}

		yield return mazeData.GenerateMaze();
	}
	
	public MazeData GetMazeData()
	{
		return mazeData;
	}
}