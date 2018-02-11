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

	public MazeData(int columns, int rows)
	{
		this.columns = columns;
		this.rows = rows;

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
		Debug.Log("MazeData::GenerateMaze()");

		// generate maze from cells using recursive backtracking (depth-first) algorithm
		MazeCellData current = GetCell(0);
		startCell = current;
		current.SetVisited(true);
		current.SetIsStartCell(true);
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
			yield return null;
		}

		Debug.Log("MazeData::Finished Generating Maze()");
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
	MazeData mazeData;
	MazeCellObject[] cellObjects;
	GameObject gridCellPrefab;
	int columns = -1;
	int rows = -1;
	Vector3 position;

	public void SetDimensions(int columns, int rows)
	{
		this.columns = columns;
		this.rows = rows;
	}

	public void SetCellPrefab(GameObject gridCellPrefab)
	{
		this.gridCellPrefab = gridCellPrefab;
	}

	public void SetPosition(Vector3 position)
	{
		this.position = position;
	}

	public MazeData GetMazeData()
	{
		return mazeData;
	}
	
	public IEnumerator CreateMaze()
	{
		if (gridCellPrefab == null ||
			columns < 0 || rows < 0)
		{
			Debug.LogError("MazeObject::CoCreateMaze() : Must set dimensions and cell prefabs before calling CreateMaze");
			yield break;
		}

		Debug.Log("MazeObject::CreateMaze()");
				
		cellObjects = new MazeCellObject[columns * rows];
		
		LayoutData layoutData = GetLayoutDataFromDummyObject();
		mazeData = new MazeData(columns, rows);
		CreateAndFillCells(layoutData, mazeData);

		var co = mazeData.GenerateMaze();
		while (co.MoveNext())
		{ 
			yield return null;
		}
	}

	struct LayoutData
	{
		public Vector2 MinWorldPosition { get; set; }
		public float CellPlacementWorldOffset { get; set; }
	}

	LayoutData GetLayoutDataFromDummyObject()
	{
		// create dummy cell object to calculate layout information
		var cellObj = Instantiate(gridCellPrefab);
		var cellScript = cellObj.GetComponent<MazeCellObject>();

		float cellPixelWidth = cellScript.cellSpriteRenderer.sprite.texture.width;
		float pixelsPerUnit = cellScript.cellSpriteRenderer.sprite.pixelsPerUnit;
		float cellWorldWidth = cellPixelWidth / pixelsPerUnit;
		float cellPlacementWorldOffset = cellWorldWidth * 0.75f;

		float gridWidth = (columns * cellPlacementWorldOffset) + (cellWorldWidth * 0.25f);
		float minWorldPosX = gridWidth * -0.5f;
		float gridHeight = (rows * cellPlacementWorldOffset) + (cellWorldWidth * 0.25f);
		float minWorldPosY = gridHeight * -0.5f;

		// destroy dummy cell
		Destroy(cellObj);
		cellObj = null;
		cellScript = null;

		return new LayoutData { 
			MinWorldPosition = new Vector2(minWorldPosX, minWorldPosY), 
			CellPlacementWorldOffset = cellPlacementWorldOffset,
		};
	}

	void CreateAndFillCells(LayoutData layoutData, MazeData mazeData)
	{
		for (int row = 0; row < rows; ++row)
		{
			for (int col = 0; col < columns; ++col)
			{
				// create cell object
				var cellObj = Instantiate(gridCellPrefab);

				// place object in world space
				cellObj.transform.position = new Vector3(position.x + layoutData.MinWorldPosition.x + (col * layoutData.CellPlacementWorldOffset),
														 position.y + layoutData.MinWorldPosition.y + (row * layoutData.CellPlacementWorldOffset), 
														 0);
				
				// child under this gameObject
				cellObj.transform.SetParent(transform);

				// get cell data and object scripts
				var cellObjScript = cellObj.GetComponent<MazeCellObject>();
				cellObjects[(row * columns) + col] = cellObjScript;

				var cellDataScript = mazeData.GetCell(col, row);
				cellDataScript.SetParentObject(cellObjScript);
				cellObjScript.SetData(cellDataScript);
			}
		}
	}
}