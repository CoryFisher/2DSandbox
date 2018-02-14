using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeRunner : MonoBehaviour
{
	MazeManager mazeManager;
	MazeObject maze;
	MazeCellData currentCell;
	float movementTimer;

	public int playerHealth = 100;
	public int playerHealthMax = 100;
	public int playerMoney = 0;
	public int playerDamage = 1;
	public int playerExp = 0;
	bool isDead;

	public Text StatsText;
	public Camera cam;
	public float MovementInputDelay = 0.2f;
	public float minHighlight = 0f;
	public float maxHighlight = 0.25f;
	public Vector2Int mazeDimensions = new Vector2Int(10, 10);

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

		// move camera
		if (currentCell != null)
		{
			Vector3 camPos = Vector2.Lerp(cam.transform.position, currentCell.GetDisplayObject().transform.position, Time.deltaTime);
			camPos.z = -10f;
			cam.transform.position = camPos;
		}

		UpdateStatsText();
	}

	private void UpdateStatsText()
	{
		string stats = "";

		stats += String.Format("Health : {0}/{1}\n", playerHealth, playerHealthMax);
		stats += String.Format("Money : {0}\n", playerMoney);
		stats += String.Format("Exp : {0}\n", playerExp);
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
		if (nextCell.HasEntity())
		{
			canMove = OnEncounterEntity(currentCell, nextCell, nextCell.GetEntity());
		}

		if (canMove)
		{
			// move player
			//nextCell.SetEntity(EntityAttribute.Player, true);
			nextCell.SetCellAttribute(CellAttribute.CurrentVisited, true);
			//currentCell.SetEntityAttribute(EntityAttribute.Player, false);
			currentCell.SetCellAttribute(CellAttribute.Visited, true);
			currentCell.SetCellAttribute(CellAttribute.CurrentVisited, false);
			currentCell = nextCell;

			// increase exp
			++playerExp;

			// update FogOfWar
			UpdateFogOfWar();
			
			// reached the end?
			if (currentCell == maze.GetMazeData().GetEndCell())
			{
				CreateAndInitMaze();
			}
		}
	}

	bool OnEncounterEntity(MazeCellData currentCell, MazeCellData nextCell, Entity entity)
	{
		// TODO: visual feedback on entity encounters

		bool canMove = entity.OnEncounter(this);

		// remove the entity if we're moving to the space
		// TODO: don't remove, switch to off/defeated state?
		if (canMove)
		{
			nextCell.SetEntity(null);
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

	public void TakeDamage(int damage)
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
		PopulateCells(cells);

		// current cell
		MazeCellData startCell = maze.GetMazeData().GetStartCell();
		currentCell = startCell;

		//currentCell.SetEntityAttribute(EntityAttribute.Player, true);
		currentCell.SetCellAttribute(CellAttribute.CurrentVisited, true);

		// turn on FogOfWar
		foreach (var cell in cells)
		{
			cell.SetFogOfWar(true);
		}

		// turn off FogOfWar for starting area
		UpdateFogOfWar();

		// init camera
		Vector3 camPos = currentCell.GetDisplayObject().transform.position;
		camPos.z = cam.transform.position.z;
		cam.transform.position = camPos;
		cam.orthographicSize = 2f;
	}

	private void PopulateCells(List<MazeCellData> cells)
	{
		// boss enemy on end cell adjacent
		MazeCellData endCell = maze.GetMazeData().GetEndCell();
		MazeCellData endCellAdjacent = null;
		endCellAdjacent = endCell.GetAnyConnectedNeighbor();
		if (endCellAdjacent != null)
		{
			endCellAdjacent.SetEntity(new EnemyEntity());
		}

		// money guarded by enemies on dead end cells 
		var treasureCells = cells.FindAll(x => x.IsDeadEnd() &&
										!x.GetCellAttribute(CellAttribute.EndCell) &&
										!x.GetCellAttribute(CellAttribute.StartCell));
		int numCells = Mathf.FloorToInt(Mathf.Sqrt(Mathf.Min(mazeDimensions.x, mazeDimensions.y)));
		for (int i = 0; i < numCells; ++i)
		{
			int index = UnityEngine.Random.Range(0, treasureCells.Count);
			if (!treasureCells[index].HasEntity())
			{
				treasureCells[index].SetEntity(new MoneyEntity());
				var neighbor = treasureCells[index].GetAnyConnectedNeighbor();
				if (neighbor != null)
				{
					neighbor.SetEntity(new EnemyEntity());
				}
				continue;
			}
			--i;
		}

		// enemies and money cells on random tiles
		var moneyCells = cells.FindAll(x => x.GetDistanceRatio() > 0.1 &&
									   x.GetDistanceRatio() < 0.9 &&
									   !x.HasAnyAttribute() &&
									   !x.HasEntity());
		//numCells = Mathf.FloorToInt(Mathf.Sqrt(Mathf.Min(mazeDimensions.x, mazeDimensions.y)));
		for (int i = 0; i < numCells; ++i)
		{
			int index = UnityEngine.Random.Range(0, moneyCells.Count);
			if (!moneyCells[index].HasEntity())
			{
				Entity entity = i % 2 == 0 ? (Entity)(new MoneyEntity()) : (Entity)(new EnemyEntity());
				moneyCells[index].SetEntity(entity);
				continue;
			}
			--i;
		}


		// traps?
	}

	private void UpdateFogOfWar()
	{
		// clear the 3x3 around the player
		currentCell.SetFogOfWar(false);

		foreach (Direction dir in Enum.GetValues(typeof(Direction)))
		{
			var neighbor = currentCell.GetNeighbor(dir);
			if (neighbor != null)
			{
				neighbor.SetFogOfWar(false);
				var neighborAdjacent = neighbor.GetNeighbor(DirectionHelper.RightOf(dir));
				if (neighborAdjacent != null)
				{
					neighborAdjacent.SetFogOfWar(false);
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
