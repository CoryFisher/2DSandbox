using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardValues
{
	public static readonly int Invalid = -1;
	public static readonly int Empty = 0;
	public static readonly int Player = 1;
	public static readonly int PieceStart = 2;
}

[System.Serializable]
public class Piece
{
	public static readonly Vector2Int invalid = Vector2Int.one * -1;

	public Vector2Int position = invalid;
	public int length = 2;
	public bool vertical = false;

	public Piece(int length, bool vertical)
	{
		if (length < 2)
		{
			length = 2;
		}
		this.length = length;
		this.vertical = vertical;
	}

	public Piece(Piece otherPiece)
	{
		position = otherPiece.position;
		length = otherPiece.length;
		vertical = otherPiece.vertical;
	}

	public Vector2Int GetForwardMovePosition(int numSpaces)
	{
		if (vertical)
		{
			return position + (Vector2Int.down * (length - 1)) + (Vector2Int.down * numSpaces);
		}
		else
		{
			return position + (Vector2Int.right * (length - 1)) + (Vector2Int.right * numSpaces);
		}
	}

	public Vector2Int GetBackwardMovePosition(int numSpaces)
	{
		if (vertical)
		{
			return position + (Vector2Int.up * numSpaces);
		}
		else
		{
			return position + (Vector2Int.left * numSpaces);
		}
	}

	public void MoveForward(int numSpaces)
	{
		if (vertical)
		{
			position += Vector2Int.down * numSpaces;
		}
		else
		{
			position += Vector2Int.right * numSpaces;
		}
	}

	public void MoveBackward(int numSpaces)
	{
		if (vertical)
		{
			position += Vector2Int.up * numSpaces;
		}
		else
		{
			position += Vector2Int.left * numSpaces;
		}
	}
}

public class Board
{
	public List<Piece> pieces;

	int[,] board;
	int width;
	int height;
	int exitsIndex;


	public Board(int width, int height, int exitsIndex)
	{
		// early out invalid input
		if (!IsValidInput(width, height, exitsIndex))
		{	
			return;
		}

		board = new int[height,width];
		this.width = width;
		this.height = height;
		this.exitsIndex = exitsIndex;
		pieces = new List<Piece>();
		CreateInitialPlayer();
	}

	public Board(Board otherBoard)
	{
		board = new int[otherBoard.height, otherBoard.width];
		this.width = otherBoard.width;
		this.height = otherBoard.height;
		this.exitsIndex = otherBoard.exitsIndex;
		pieces = new List<Piece>();
		Piece player = new Piece(otherBoard.pieces[0]);
		pieces.Add(player);
	}

	void CreateInitialPlayer()
	{
		Piece player = new Piece(2, false);
		player.position.x = -2;
		player.position.y = exitsIndex;
		pieces.Add(player);
	}

	public int GetValue(Vector2Int position)
	{
		if (IsValidPosition(position))
		{
			return board[position.y, position.x];
		}
		return BoardValues.Invalid;
	}

	bool IsValidPosition(Vector2Int position)
	{
		if (position.x < 0 || position.x >= board.GetLength(1))
		{
			return false;
		}

		if (position.y < 0 || position.y >= board.GetLength(0))
		{
			return false;
		}

		return true;
	}

	bool CanPlace(Piece piece)
	{
		foreach (Vector2Int position in GetPositions(piece))
		{
			if (GetValue(position) != BoardValues.Empty)
			{
				return false;
			}
		}
		return true;
	}

	public bool Place(Piece piece)
	{
		if (CanPlace(piece))
		{
			int pieceIndex = pieces.Count;
			pieces.Add(piece);

			foreach (Vector2Int position in GetPositions(piece))
			{
				board[position.y, position.x] = BoardValues.PieceStart + pieceIndex;
			}
			return true;
		}
		return false;
	}

	public IEnumerable<Vector2Int> GetPositions(Piece piece)
	{
		for (int i = 0; i < piece.length; ++i)
		{
			Vector2Int pos = piece.position + (piece.vertical ? Vector2Int.down : Vector2Int.right);
			yield return pos;
		}
	}
	
	bool IsValidInput(int width, int height, int exitsIndex)
	{
		string output = "";
		if (width < 3)
		{
			output += "invalid width (" + width + "), ";
		}
		if (height < 3)
		{
			output += "invalid height (" + height + "), ";
		}
		if (exitsIndex < 0 || exitsIndex >= height)
		{
			output += "invalid exitsIndex (" + exitsIndex + "), ";
		}

		if (output != "")
		{
			Debug.LogError("Board: " + output);
			return false;
		}
		return true;
	}

	public int[,] GetArray()
	{
		return board;
	}

	public IEnumerable<Board> EnumerateMoveStates(int pieceIndex)
	{
		// check forward moves
		int numSpaces = 1;
		bool backward = false;
		while (CanMovePiece(pieceIndex, backward, numSpaces))
		{
			Board newBoard = new Board(this);
			newBoard.MovePiece(pieceIndex, backward, numSpaces);
			++numSpaces;
			yield return newBoard;
		}
		
		// check backward moves
		numSpaces = 1;
		backward = true;
		while (CanMovePiece(pieceIndex, backward, numSpaces))
		{
			Board newBoard = new Board(this);
			newBoard.MovePiece(pieceIndex, backward, numSpaces);
			++numSpaces;
			yield return newBoard;
		}

		yield return null;
	}

	bool CanMovePiece(int pieceIndex, bool backward, int length)
	{
		if (pieceIndex < 0 || pieceIndex >= pieces.Count)
		{
			return false;
		}

		Piece piece = pieces[pieceIndex];
		Vector2Int nextPosition;
		if (backward)
		{
			nextPosition = piece.GetBackwardMovePosition(length);
		}
		else
		{
			nextPosition = piece.GetForwardMovePosition(length);
		}
		return IsValidPosition(nextPosition);
	}

	void MovePiece(int pieceIndex, bool backward, int numSpaces)
	{
		if (pieceIndex < 0 || pieceIndex >= pieces.Count)
		{
			return;
		}

		Piece piece = pieces[pieceIndex];

		// clear old spaces
		foreach (var pos in GetPositions(piece))
		{
			board[pos.y, pos.x] = BoardValues.Empty;
		}

		// update position
		if (backward)
		{
			piece.MoveBackward(numSpaces);
		}
		else
		{
			piece.MoveForward(numSpaces);
		}

		// mark new spaces
		foreach (var pos in GetPositions(piece))
		{
			board[pos.y, pos.x] = pieceIndex;
		}
	}
}

public class BoardGenerator
{
	public static Board GenerateRandom(int width, int height, int exitIndex)
	{
		Board board = new Board(width, height, exitIndex);
		FillBoardRandom(board);
		return board;
	}

	public static void FillBoardRandom(Board board)
	{
		int length = 2;
		bool vertical = true;

		Piece piece = new Piece(length, vertical);
		board.Place(piece);

		// TODO
	}
}


public class BoardSolver
{
	class BoardState
	{
		public BoardState parent;
		public Board state;

		public BoardState(Board board, BoardState parent)
		{
			this.state = board;
			this.parent = parent;
		}
	}

	public static List<Board> Solve(Board board)
	{
		// create initial board state
		BoardState initialState = new BoardState(board, null);
		Stack<BoardState> states = new Stack<BoardState>();
		states.Push(initialState);

		foreach (var boardstate in states)
		{
			Board state = boardstate.state;
			for (int i = 0; i < state.pieces.Count; ++i)
			{
				foreach (Board nextState in state.EnumerateMoveStates(i))
				{
					states.Push(new BoardState(nextState, boardstate));
				}
			}
		}

		Debug.LogError("Number of initial states: " + states.Count);
		
		return null;
	}
}



public class GridlockController : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			int width = 4;
			int height = 4;
			int exitIndex = 0;
			int numAttempts = 1;

			for (int i = 0; i < numAttempts; ++i)
			{
				Board board = BoardGenerator.GenerateRandom(width, height, exitIndex);
				List<Board> solution = BoardSolver.Solve(board);
				if (solution != null)
				{
					PrintSolution(solution);
				}
			}
		}
	}

	void PrintSolution(List<Board> solution)
	{

	}
}
