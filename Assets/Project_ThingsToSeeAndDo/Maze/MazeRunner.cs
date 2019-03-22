using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeRunner : Singleton<MazeRunner>
{
	// class references
	MazeManager mazeManager;
	MazeObject maze;
	MazeCellData currentCell;
	MazeSolver solver;

	// player data
	public int playerHealth = 100;
	public int playerHealthMax = 100;
	public int playerMoney = 0;
	public int playerDamage = 1;
	public int playerExp = 0;
	public float MoveSpeed = 1.5f;
	public float VisitedMoveSpeed = 1.5f;
	bool isDead;
	float movementTimer;
	float idleActivationTimer;

	// editor values
	public Text StatsText;
	public Camera cam;
	public ParticleSystem damageParticles;
	public ParticleSystem moneyParticles;
	public float minHighlight = 0f;
	public float maxHighlight = 0.25f;
	public Vector2Int MazeDimensions = new Vector2Int(10, 10);
	public float idleActivationTime = 3.0f;
	public bool BreadthFirstSolving = false;
	public Text healthText;
	public Text moneyText;
	public Text damageText;
	public Text moveSpeedText;




	KeyCode[] movementKeys = new KeyCode[] { KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow};

	private void Awake()
	{
		RegisterSingletonInstance(this);
		solver = new MazeSolver();
	}

	private void Start()
	{
		mazeManager = MazeManager.Get();

		MazeDimensions = new Vector2Int(4, 4);
		CreateAndInitMaze();
	}

	private void Update()
	{
		if (!isDead)
		{
			movementTimer += Time.deltaTime;
			idleActivationTimer += Time.deltaTime;
			float movementTime = 1f / MoveSpeed;
			float visitedSpeedMult = 1f / VisitedMoveSpeed;
			bool keyPressed = false;
			foreach (Direction dir in Enum.GetValues(typeof(Direction)))
			{
				if (Input.GetKey(movementKeys[(int)dir]))
				{
					keyPressed = true;
					if (currentCell.HasNeighbor(dir) &&
						!currentCell.HasWall(dir))
					{
						if ((currentCell.GetNeighbor(dir).GetCellAttribute(CellAttribute.Visited) &&
							movementTimer > movementTime * visitedSpeedMult) ||
							movementTimer > movementTime)
						{
							movementTimer = 0f;
							TryMove(dir);
							break;
						}
					}
				}
			}

			if (keyPressed)
			{
				idleActivationTimer = 0f;
			}

			if (idleActivationTimer > idleActivationTime)
			{					
				// solver
				var nextToVisit = solver.GetNextCellToVisit();
				if (nextToVisit == null)
				{
					Debug.LogWarning("nextToVisit == null");
					return;
				}

				// is a neighbor?
				foreach (Direction dir in Enum.GetValues(typeof(Direction)))
				{
					if (currentCell.GetNeighbor(dir) == nextToVisit)
					{
						if ((currentCell.GetNeighbor(dir).GetCellAttribute(CellAttribute.Visited) &&
							movementTimer > movementTime * visitedSpeedMult) ||
							movementTimer > movementTime)
						{
							movementTimer = 0f;
							TryMove(dir);
							return;
						}
					}
				}

				// path to it
				MazePath pathToNext = MazeSolver.SolveToGetPath(maze, currentCell, nextToVisit);
				foreach (var cell in pathToNext.GetPath())
				{
					if (cell != currentCell)
					{
						// is a neighbor? It fn should be
						foreach (Direction dir in Enum.GetValues(typeof(Direction)))
						{
							if (currentCell.GetNeighbor(dir) == cell)
							{
								if ((currentCell.GetNeighbor(dir).GetCellAttribute(CellAttribute.Visited) &&
									movementTimer > movementTime * visitedSpeedMult) ||
									movementTimer > movementTime)
								{
									movementTimer = 0f;
									TryMove(dir);
									return;
								}
							}
						}
					}
				}
			}
		}
		
		UpdateStatsText();
	}

	private void UpdateStatsText()
	{
		healthText.text = String.Format("{0}/{1}", playerHealth, playerHealthMax);
		moneyText.text = playerMoney.ToString();
		damageText.text = playerDamage.ToString();
		moveSpeedText.text = MoveSpeed.ToString();
	}

	private void TryMove(Direction dir)
	{
		// get neighbor
		var nextCell = currentCell.GetNeighbor(dir);
		bool canMove = true;

		// something is on the cell
		if (nextCell.HasEntity())
		{
			canMove = OnEncounterEntity(currentCell, nextCell, nextCell.GetEntity());
		}

		if (canMove)
		{
			// increase exp
			if (nextCell.GetCellAttribute(CellAttribute.Visited) == false)
			{
				++playerExp;
			}

			// move
			currentCell.SetCellAttribute(CellAttribute.Visited, true);
			currentCell.SetCellAttribute(CellAttribute.CurrentVisited, false);
			nextCell.SetCellAttribute(CellAttribute.CurrentVisited, true);

			currentCell = nextCell;
			solver.NotifyCellVisited(currentCell, BreadthFirstSolving);


			// update FogOfWar
			UpdateFogOfWar();
			
			// reached the end?
			if (currentCell == maze.GetMazeData().GetEndCell())
			{
				// TODO: something when we beat a level

				CreateAndInitMaze();
			}
		}
	}

	public Transform GetCurrentCellTransform()
	{
		if (currentCell != null)
		{
			var displayObject = currentCell.GetDisplayObject();
			if (displayObject != null)
			{
				return displayObject.transform;
			}
		}
		return null;
	}

	bool OnEncounterEntity(MazeCellData currentCell, MazeCellData nextCell, Entity entity)
	{
		if (entity.GetEntityType() == EntityType.Enemy)
		{
			damageParticles.transform.position = nextCell.GetDisplayObject().transform.position;
			damageParticles.Play();
		}
		else if (entity.GetEntityType() == EntityType.Money)
		{
			moneyParticles.transform.position = nextCell.GetDisplayObject().transform.position;
			moneyParticles.Play();
		}

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
		// destroy old maze
		if (maze != null)
		{
			Destroy(maze.gameObject);
		}

		// create new maze
		MazeDimensions += Vector2Int.one;
		maze = mazeManager.CreateMaze(MazeDimensions);
		Debug.Assert(maze != null);

		var mazedata = maze.GetMazeData();
		Debug.Assert(mazedata != null);

		// populate with entities
		List<MazeCellData> cells = new List<MazeCellData>(mazedata.GetCells());
		PopulateCells(cells);

		// init current cell
		currentCell = maze.GetMazeData().GetStartCell();
		currentCell.SetCellAttribute(CellAttribute.CurrentVisited, true);

		// init solver
		solver.InitForMaze(maze);
		solver.NotifyCellVisited(currentCell, BreadthFirstSolving);

		// turn on FogOfWar for all cells
		foreach (var cell in cells)
		{
			cell.SetFogOfWar(true);
		}

		// turn off FogOfWar for starting area
		UpdateFogOfWar();
	}

	private void PopulateCells(List<MazeCellData> cells)
	{
		// treasure guarded by enemies on dead end cells 
		var treasureCells = cells.FindAll(x => x.IsDeadEnd() &&
										!x.GetCellAttribute(CellAttribute.EndCell) &&
										!x.GetCellAttribute(CellAttribute.StartCell) &&
										!x.HasEntity());
		foreach (var treasureCell in treasureCells)
		{
			treasureCell.SetEntity(new MoneyEntity());
			
			// will be the only adjacent cell
			var neighbor = treasureCell.GetAnyConnectedNeighbor();
			if (neighbor != null)
			{
				neighbor.SetEntity(new EnemyEntity());
			}
		}

		// money cells on random tiles
		var moneyCells = cells.FindAll(x => x.GetDistanceRatio() > 0.1 &&
									   x.GetDistanceRatio() < 0.9 &&
									   !x.HasEntity());
		int numMoneys = moneyCells.Count / 4;
		for (int i = 0; i < moneyCells.Count && i < numMoneys; ++i)
		{
			moneyCells[i].SetEntity(new MoneyEntity());
		}

		// enemy cells on random tiles
		var enemyCells = cells.FindAll(x => x.GetDistanceRatio() > 0.1 &&
									   x.GetDistanceRatio() < 0.9 &&
									   !x.HasEntity());
		int numEnemies = enemyCells.Count / 4; // means less enemies than moneys
		for (int i = 0; i < enemyCells.Count && i < numEnemies; ++i)
		{
			enemyCells[i].SetEntity(new EnemyEntity());
		}

		// traps?

		// boss enemy on end cell adjacent
		MazeCellData endCell = maze.GetMazeData().GetEndCell();
		MazeCellData endCellAdjacent = null;
		endCellAdjacent = endCell.GetAnyConnectedNeighbor();
		if (endCellAdjacent != null)
		{
			endCellAdjacent.SetEntity(new EnemyEntity());
		}
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
		UpdateStatsText();
		isDead = true;
		Debug.LogError("----- YOU DIED -----");
	}

	public void OnHPButtonClicked()
	{
		if (playerMoney >= 25)
		{
			playerMoney -= 25;
			playerHealthMax += 25;
			playerHealth += 25;
			UpdateStatsText();
		}
	}

	public void OnSpeedButtonClicked()
	{
		if (playerMoney >= 25)
		{
			playerMoney -= 25;
			MoveSpeed += 1f;
			VisitedMoveSpeed += 0.5f;
			cam.orthographicSize += 0.1f;
			UpdateStatsText();
		}
	}

	public void OnDamageButtonClicked()
	{
		if (playerMoney >= 25)
		{
			playerMoney -= 25;
			playerDamage += 1;
			UpdateStatsText();
		}
	}

	public void OnMoneyButtonClicked()
	{
		playerMoney++;
		UpdateStatsText();
	}
}
