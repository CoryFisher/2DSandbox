using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeRunner : MonoBehaviour
{
	MazeManager mazeManager;
	MazeObject maze;
	Vector2Int mazeDimensions = new Vector2Int(10, 10);
	MazeCellData currentCell;
	float movementTimer;

	int playerHealth = 100;
	int playerHealthMax = 100;
	int playerMoney = 0;
	bool isDead;

	public Text StatsText;
	public float MovementInputDelay = 0.2f;
	public float minHighlight = 0f;
	public float maxHighlight = 0.25f;

	KeyCode[] movementKeys = new KeyCode[] { KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow};

	private void Start()
	{
		mazeManager = MazeManager.Get();

		CreateAndInitMaze();
	}

	private void Update()
	{
		if (!isDead)
		{
			movementTimer += Time.deltaTime;
			foreach (Direction dir in Enum.GetValues(typeof(Direction)))
			{
				if (Input.GetKey(movementKeys[(int)dir]) &&
					currentCell.HasNeighbor(dir) &&
					!currentCell.HasWall(dir) &&
					movementTimer > MovementInputDelay)
				{
					movementTimer = 0f;
					TryMove(dir);
					break;
				}
			}
		}

		UpdateStatsText();
	}

	private void UpdateStatsText()
	{
		string stats = "";

		stats += String.Format("Health : {0}/{1}\n", playerHealth, playerHealthMax);
		stats += String.Format("Money : {0}\n", playerMoney);
		if (isDead)
		{
			stats += String.Format("IS DEAD\n");
		}

		StatsText.text = stats;
	}

	private void TryMove(Direction dir)
	{
		bool canMove = true;

		// get neighbor
		var nextCell = currentCell.GetNeighbor(dir);

		// something is on the cell
		if (nextCell.HasAnyEntity())
		{
			foreach (EntityAttribute entity in Enum.GetValues(typeof(EntityAttribute)))
			{
				if (nextCell.GetEntityAttribute(entity))
				{
					canMove = OnEncounterEntity(currentCell, nextCell, entity);
				}
			}
		}

		// move player
		nextCell.SetEntityAttribute(EntityAttribute.Player, true);
		nextCell.SetCellAttribute(CellAttribute.CurrentVisited, true);
		currentCell.SetEntityAttribute(EntityAttribute.Player, false);
		currentCell.SetCellAttribute(CellAttribute.Visited, true);
		currentCell.SetCellAttribute(CellAttribute.CurrentVisited, false);
		currentCell = nextCell;

		// update FogOfWar
		UpdateFogOfWar();

		// reached the end?
		if (currentCell == maze.GetMazeData().GetEndCell())
		{
			CreateAndInitMaze();
		}
	}

	bool OnEncounterEntity(MazeCellData currentCell, MazeCellData nextCell, EntityAttribute entity)
	{
		// TODO: visual feedback on entity encounters

		bool canMove = true;
		switch (entity)
		{
			case EntityAttribute.Enemy:
				TakeDamage(25);
				//if (!MazeEnemy.IsDead())
				//{
				//	canMove = false;
				//}
				break;
			case EntityAttribute.Health:
				IncreaseHealth(25);
				break;
			case EntityAttribute.Money:
				IncreaseMoney(25);
				break;
		}

		// remove the entity if we're moving to the space
		// TODO: only remove enemies, switch 
		if (canMove)
		{
			nextCell.SetEntityAttribute(entity, false);
		}

		return canMove;
	}

	private void IncreaseMoney(int money)
	{
		playerMoney += money;
	}

	private void IncreaseHealth(int health)
	{
		playerHealth += health;
		if (playerHealth > playerHealthMax)
		{
			playerHealth = playerHealthMax;
		}
	}

	private void TakeDamage(int damage)
	{
		playerHealth -= damage;
		if (playerHealth <= 0)
		{
			Die();
		}
	}

	private void CreateAndInitMaze()
	{
		if (maze != null)
		{
			Destroy(maze.gameObject);
		}

		maze = mazeManager.CreateMaze(mazeDimensions);

		List<MazeCellData> cells = new List<MazeCellData>(maze.GetMazeData().GetCells());
		
		// boss enemy on end cell adjacent
		MazeCellData endCell = maze.GetMazeData().GetEndCell();

		MazeCellData endCellAdjacent = null;
		Direction direction;
		// get neighbor
		do
		{
			direction = (Direction)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Direction)).Length);
			endCellAdjacent = endCell.GetNeighbor(direction);
		} while (endCellAdjacent == null);
		endCellAdjacent.SetEntityAttribute(EntityAttribute.Enemy, true);

		// enemies on dead end cells
		var deadEndCells = cells.FindAll(x => x.IsDeadEnd() && 
										!x.GetCellAttribute(CellAttribute.EndCell) &&
										!x.GetCellAttribute(CellAttribute.StartCell));
		foreach (var cell in deadEndCells)
		{
			cell.SetEntityAttribute(EntityAttribute.Enemy, true);
		}

		// money cells on almost any tile
		var moneyCells = cells.FindAll(x => x.GetDistanceRatio() > 0.1 && 
									   x.GetDistanceRatio() < 0.9 &&
									   !x.HasAnyAttribute() &&
									   !x.HasAnyEntity());
		for (int i = 0; i < 10; ++i)
		{
			int index = UnityEngine.Random.Range(0, moneyCells.Count);
			moneyCells[index].SetEntityAttribute(EntityAttribute.Money, true);
		}


		// traps?


		// current cell
		MazeCellData startCell = maze.GetMazeData().GetStartCell();
		currentCell = startCell;

		currentCell.SetEntityAttribute(EntityAttribute.Player, true);

		// turn on FogOfWar
		foreach (var cell in cells)
		{
			cell.SetEntityAttribute(EntityAttribute.FogOfWar, true);
		}

		// turn off FogOfWar for starting area
		UpdateFogOfWar();
	}

	private void UpdateFogOfWar()
	{
		currentCell.SetEntityAttribute(EntityAttribute.FogOfWar, false);
		currentCell.SetEntityAttribute(EntityAttribute.FogOfWar2, false);
		foreach (var neighbor in currentCell.GetValidNeighbors())
		{
			neighbor.SetEntityAttribute(EntityAttribute.FogOfWar2, false);
			neighbor.SetEntityAttribute(EntityAttribute.FogOfWar, false);

			foreach (var nsquared in neighbor.GetValidNeighbors())
			{
				if (nsquared.GetEntityAttribute(EntityAttribute.FogOfWar))
				{
					nsquared.SetEntityAttribute(EntityAttribute.FogOfWar, false);
					nsquared.SetEntityAttribute(EntityAttribute.FogOfWar2, true);
				}
			}

		}
	}

	private void Die()
	{
		isDead = true;
		Debug.LogError("----- YOU DIED -----");
	}
}
