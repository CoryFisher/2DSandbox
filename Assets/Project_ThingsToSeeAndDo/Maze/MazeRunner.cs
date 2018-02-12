using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}

	private void TryMove(Direction dir)
	{
		// get neighbor
		var nextCell = currentCell.GetNeighbor(dir);
		
		// something is on the cell
		if (nextCell.GetEntity() != CellEntityType.None)
		{
			switch (nextCell.GetEntity())
			{
				case CellEntityType.Enemy:
					TakeDamage(25);
					break;
				case CellEntityType.Health:
					IncreaseHealth(25);
					break;
				case CellEntityType.Money:
					IncreaseMoney(25);
					break;
			}
			nextCell.SetEntity(CellEntityType.None);
		}

		// move player
		nextCell.SetIsPlayer(true);
		currentCell.SetIsPlayer(false);
		currentCell.SetVisited(true);
		currentCell = nextCell;

		// reached the end?
		if (currentCell == maze.GetMazeData().GetEndCell())
		{
			CreateAndInitMaze();
		}
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

		var deadEndCells = cells.FindAll(x => x.IsDeadEnd());
		int index = UnityEngine.Random.Range(0, deadEndCells.Count);
		deadEndCells[index].SetEntity(CellEntityType.Enemy);

		MazeCellData startCell = maze.GetMazeData().GetStartCell();
		currentCell = startCell;

		currentCell.SetIsPlayer(true);
	}

	private void Die()
	{
		isDead = true;
		Debug.LogError("----- YOU DIED -----");
	}
}
