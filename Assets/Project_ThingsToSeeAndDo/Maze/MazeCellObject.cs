using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




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

	public static Direction RightOf(Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				return Direction.Right;
			case Direction.Right:
				return Direction.Down;
			case Direction.Down:
				return Direction.Left;
			case Direction.Left:
				return Direction.Up;
		}
		return Direction.Up;
	}
}

public class MazeCellObject : MonoBehaviour
{
	public SpriteRenderer cellSpriteRenderer;
	public SpriteRenderer wallSpriteRenderer;
	public SpriteRenderer overlaySpriteRenderer;
	
	public void SetCellSprite(Sprite cellSprite)
	{
		cellSpriteRenderer.sprite = cellSprite;
	}

	public void SetWallSprite(Sprite wallSprite)
	{
		wallSpriteRenderer.sprite = wallSprite;
	}

	public void SetOverlaySprite(Sprite overlaySprite)
	{
		overlaySpriteRenderer.sprite = overlaySprite;
	}
}






[Flags]
public enum CellAttribute
{
	None = 0,
	CurrentVisited = 1,
	StartCell = 2,
	EndCell = 4,
	ShortestPath = 8,
	Visited = 16,
	//DistanceFromStart = 32,
}

	//None = 0,
	//FogOfWar2 = 1,
	//FogOfWar = 2,

public enum EntityType
{
	None,
	Player,
	Enemy,
	Health,
	Money,
}

public abstract class Entity
{
	public abstract EntityType GetEntityType();
	// returns whether the player can move to this cell
	public abstract bool OnEncounter(MazeRunner player);
}

public class EnemyEntity : Entity
{
	public int Health = 2;
	public int Defense = 0;
	public int Damage = 2;
	public int Exp = 2;

	public override EntityType GetEntityType()
	{
		return EntityType.Enemy;
	}

	public override bool OnEncounter(MazeRunner player)
	{
		Health -= Mathf.Max(player.playerDamage - Defense, 0);
		if (Health < 1)
		{
			player.playerExp += Exp;
			return true;
		}
		else
		{
			player.TakeDamage(Damage);
			return false;
		}
	}
}

public class MoneyEntity : Entity
{
	public int Value = 25;

	public override EntityType GetEntityType()
	{
		return EntityType.Money;
	}

	public override bool OnEncounter(MazeRunner player)
	{
		player.playerMoney += Value;
		return true;
	}
}

// virtual data for a maze cell (neighbors, walls, attributes)
public class MazeCellData
{
	MazeCellData[] neighbors = new MazeCellData[4];
	bool[] walls = new bool[4];
	MazeCellObject displayObject;
	int distanceFromStart;
	int maxDistanceFromStart;
	MazeCellData solverParent;
	float distanceRatio;

	int cellTypeAttributes;
	Entity entity;
	bool fogOfWar;

	public MazeCellData()
	{
		walls[0] = true;
		walls[1] = true;
		walls[2] = true;
		walls[3] = true;
	}

	// DISPLAY OBJECT
	public void SetDisplayObject(MazeCellObject cellObject)
	{
		this.displayObject = cellObject;
		UpdateCellSprite();
		UpdateEntitySprite();
		UpdateWallSprite();
	}

	public MazeCellObject GetDisplayObject()
	{
		return displayObject;
	}

	// GRID NEIGHBORS
	public void SetNeighbor(Direction dir, MazeCellData neighbor)
	{
		neighbors[(int)dir] = neighbor;
	}

	public MazeCellData GetNeighbor(Direction dir)
	{
		return neighbors[(int)dir];
	}

	public MazeCellData GetAnyConnectedNeighbor()
	{
		foreach (var neighbor in neighbors)
		{
			if (neighbor != null)
			{
				return neighbor;
			}
		}
		return null;
	}

	public IEnumerable<MazeCellData> GetConnectedNeighbors()
	{
		foreach (var neighbor in neighbors)
		{
			if (neighbor != null)
			{
				yield return neighbor;
			}
		}
	}
	
	public bool HasNeighbor(Direction dir)
	{
		return neighbors[(int)dir] != null;
	}

	public bool HasUnvisitedNeighbors()
	{
		for (int i = 0; i < neighbors.Length; ++i)
		{
			if (neighbors[i] != null && neighbors[i].GetCellAttribute(CellAttribute.Visited) == false)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<MazeCellData> GetValidNeighbors()
	{
		for (int i = 0; i < neighbors.Length; ++i)
		{
			if (neighbors[i] != null)
			{
				yield return neighbors[i];
			}
		}
	}

	// WALLS
	public void SetWall(Direction dir, bool enabled)
	{
		walls[(int)dir] = enabled;
		UpdateWallSprite();
	}

	public bool HasWall(Direction dir)
	{
		return walls[(int)dir];
	}
	
	public bool IsDeadEnd()
	{
		int numOpenWalls = 0;
		foreach (Direction dir in Enum.GetValues(typeof(Direction)))
		{
			if (!HasWall(dir))
			{
				++numOpenWalls;
			}
		}
		return numOpenWalls == 1;
	}

	void UpdateWallSprite()
	{
		if (displayObject)
		{
			int spriteIndex = 0;
			spriteIndex |= HasWall(Direction.Up)    ? 0 : (int)DirectionFlag.Up;
			spriteIndex |= HasWall(Direction.Right) ? 0 : (int)DirectionFlag.Right;
			spriteIndex |= HasWall(Direction.Down)  ? 0 : (int)DirectionFlag.Down;
			spriteIndex |= HasWall(Direction.Left)  ? 0 : (int)DirectionFlag.Left;
			displayObject.SetWallSprite(MazeManager.Get().GetCellWallSprite(spriteIndex));
		}
	}

	// MAZE PROPERTIES
	public void SetDistanceFromStart(int dist)
	{
		distanceFromStart = dist;
		if (maxDistanceFromStart > float.Epsilon)
		{
			distanceRatio = (float)distanceFromStart / maxDistanceFromStart;
		}
		else
		{
			distanceRatio = 1f;
		}
	}

	public int GetDistanceFromStart()
	{
		return distanceFromStart;
	}

	public void SetMaxDistanceFromStart(int dist)
	{
		maxDistanceFromStart = dist;
		if (maxDistanceFromStart > float.Epsilon)
		{
			distanceRatio = (float)distanceFromStart / maxDistanceFromStart;
		}
	}

	public int GetMaxDistanceFromStart()
	{
		return maxDistanceFromStart;
	}

	public float GetDistanceRatio()
	{
		return distanceRatio;
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

	// CELL ATTRIBUTES
	public void SetCellAttribute(CellAttribute attr, bool val)
	{
		if (val)
		{
			cellTypeAttributes |= (int)attr;
		}
		else
		{
			cellTypeAttributes &= ~(int)attr;
		}
		UpdateCellSprite();
	}

	public bool GetCellAttribute(CellAttribute attr)
	{
		return (cellTypeAttributes & (int)attr) != 0;
	}

	public bool HasAnyAttribute()
	{
		return cellTypeAttributes != 0;
	}

	void UpdateCellSprite()
	{
		if (displayObject)
		{
			foreach (CellAttribute attr in Enum.GetValues(typeof(CellAttribute)))
			{
				if ((cellTypeAttributes & (int)attr) != 0)
				{
					displayObject.SetCellSprite(MazeManager.Get().GetCellSprite(attr));
					// CellAttributes are prioritized so take the first one
					return;
				}
			}
			displayObject.SetCellSprite(MazeManager.Get().GetCellSprite(CellAttribute.None));
		}
	}

	// ENTITY ATTRIBUTES
	public void SetEntity(Entity entity)
	{
		this.entity = entity;
		UpdateEntitySprite();
	}

	public Entity GetEntity()
	{
		return entity;
	}

	public bool HasEntity()
	{
		return entity != null;
	}

	void UpdateEntitySprite()
	{
		if (displayObject)
		{
			if (fogOfWar)
			{
				displayObject.SetOverlaySprite(MazeManager.Get().GetFogOfWarSprite());
			}
			else if (entity != null)
			{
				displayObject.SetOverlaySprite(MazeManager.Get().GetEntitySprite(entity.GetEntityType()));
			}
			else
			{
				displayObject.SetOverlaySprite(MazeManager.Get().GetEntitySprite(EntityType.None));
			}
		}
	}

	public void SetFogOfWar(bool val)
	{
		fogOfWar = val;
		UpdateEntitySprite();
	}

	// DEBUG
	public void DrawLinesToNieghbors()
	{
		if (displayObject)
		{
			var upNeighbor = GetNeighbor(Direction.Up);
			if (upNeighbor != null)
			{
				var neighborObj = upNeighbor.GetDisplayObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(displayObject.transform.position, neighborObj.transform.position, Color.red);
				}
			}
			var rightNeighbor = GetNeighbor(Direction.Right);
			if (rightNeighbor != null)
			{
				var neighborObj = rightNeighbor.GetDisplayObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(displayObject.transform.position, neighborObj.transform.position, Color.yellow);
				}
			}
			var downNeighbor = GetNeighbor(Direction.Down);
			if (downNeighbor != null)
			{
				var neighborObj = downNeighbor.GetDisplayObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(displayObject.transform.position, neighborObj.transform.position, Color.green);
				}
			}
			var leftNeighbor = GetNeighbor(Direction.Left);
			if (leftNeighbor != null)
			{
				var neighborObj = leftNeighbor.GetDisplayObject();
				if (neighborObj != null)
				{
					Debug.DrawLine(displayObject.transform.position, neighborObj.transform.position, Color.blue);
				}
			}
		}
	}
}