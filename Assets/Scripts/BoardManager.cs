using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class BoardManager : MonoBehaviour, IBoardItemListener, IChessBoardRefereeDelegate
{
	// the actual gameEngine
	public readonly IChessGameReferee Referee = new ClassicChessReferee();
	
	// The size of a square
	private const double ChessTileSize = 1.0f;

	// List of available Assets to draw the pieces
	public List<GameObject> ChessmanPrefabs = new List<GameObject>();
	
	// List of gameObject actually on the board (associated with their position for easy picking)
	public Dictionary<int, GameObject> OnBoardObjects = new Dictionary<int, GameObject>();

	private GameObject DraggingChessman { get; set; }
	private Square DraggingStartSquare { get; set; }
	
	
	public Square MoveOriginSquare { get; set; }
	
	// Use this for initialization
	void Start ()
	{
		
		InitBoard();
		Referee.Delegate = this;
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonUp(0) && DraggingChessman != null) // user drop
		{
			Debug.Log("Dropping");
			
			var square = GetSelectedSquare();

			if (square == null || square.Equals(DraggingStartSquare)) // fake move
			{
				Debug.Log("fake move");
				DraggingChessman.transform.position = GetCenterPosition(DraggingStartSquare);
				DraggingChessman = null;
				DraggingStartSquare = null;
			}
			else if(Referee.GetMove(DraggingStartSquare, square)  != null)
			{
				Debug.Log("Drop");
				OnBoardObjects[DraggingStartSquare.Row * 8 + DraggingStartSquare.Column] = null;
				Drop(DraggingChessman, square);
			}
			else
			{
				Debug.Log("illegal move");
				DraggingChessman.transform.position = GetCenterPosition(DraggingStartSquare);
				DraggingChessman = null;
				DraggingStartSquare = null;
			}
			DraggingChessman = null;
			DraggingStartSquare = null;

		}
		else if (Input.GetMouseButton(0) && DraggingChessman != null)
		{
			Debug.Log("Dragging");
			var pos = GetMousePosition();
			if (pos.HasValue)
			{
				DraggingChessman.transform.position = pos.Value;
			}
		}
		else if(Input.GetKeyDown(KeyCode.Space))
		{
			var angle = 270;
			transform.Rotate(new Vector3(0, angle, 0));
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			var angle = 90;
			transform.Rotate(new Vector3(0, angle, 0));
			transform.Translate(new Vector3(-8, 0, 0));
		}
//		else if (Input.GetKeyDown(KeyCode.RightArrow))
//		{
//			var angle = 270;
//			transform.Rotate(new Vector3(0, angle, 0));
//			transform.Translate(new Vector3(0, 0, 0));
//		}
//		
	}
	
	/// <summary>
	/// Draw the original board with the chessman properly set
	/// </summary>
	private void InitBoard()
	{
		Referee.ResetBoard();
		
		for (int row = 0; row < Referee.BoardSize; row++)
		{
			for (int column = 0; column < Referee.BoardSize; column++)
			{
				var square = new Square(row + 1, column + 1);
				
				var piece = Referee[square];

				if (piece != null)
				{
					Spawn(piece.Type, piece.Color, square);
				}
			}
		}
	}
	
	/// <summary>
	/// Add a given chessman to the board
	/// </summary>
	/// <param name="piece"></param>
	/// <param name="playerColor"></param>
	/// <param name="row"></param>
	/// <param name="column"></param>
	private void Spawn(PieceType piece, PlayerColor playerColor, Square square)
	{
		var idx = (int) piece + (playerColor == PlayerColor.White ? 6 : 0);
		var position = Vector3.forward * (float)(square.Row - ChessTileSize/2) + Vector3.right * (float) (square.Column - ChessTileSize/2);
		var prefab = Instantiate(ChessmanPrefabs[idx], position, Quaternion.identity);
		prefab.transform.SetParent(transform);

		prefab.GetComponent<BoardItem>().MouseEventReceived = this;
			
		OnBoardObjects[square.Row * 8 + square.Column] = prefab;
		prefab.GetComponent<BoardItem>().CurrentSquare = square;
	}

	private void Drop(GameObject prefab, Square square)
	{
		// Remove the previous 
		RemoveElementAt(square);
		
		var position = Vector3.forward * (float)(square.Row - ChessTileSize/2) + Vector3.right * (float) (square.Column - ChessTileSize/2);
		prefab.transform.position = position;
		
		OnBoardObjects[square.Row * 8 + square.Column] = prefab;
		prefab.GetComponent<BoardItem>().CurrentSquare = square;
		
		var mv = Referee.GetMove(DraggingStartSquare, square);
		Referee.DoMove(mv);
	}

	private Vector3 GetCenterPosition(Square square)
	{
		return Vector3.forward * (float)(square.Row - ChessTileSize/2) + Vector3.right * (float) (square.Column - ChessTileSize/2);
	}
	private GameObject GetGameObject(Square square)
	{
		
		GameObject selectedGameObject = null;
			
		OnBoardObjects.TryGetValue(square.Row * 8 + square.Column, out selectedGameObject);

		return selectedGameObject;
	}
	private Vector3? GetMousePosition()
	{
		if(!Camera.main) return null;

		RaycastHit hit;
		
		// Get the layer for the chess plane
		var layerMask = LayerMask.GetMask("ChessPlane");
		
		
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, layerMask))
		{
			return hit.point;
		}

		return null;
	}
	private Square GetSelectedSquare()
	{
		if(!Camera.main) return null;

		RaycastHit hit;
		
		// Get the layer for the chess plane
		var layerMask = LayerMask.GetMask("ChessPlane");
		
		
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, layerMask))
		{
			return new Square((int) hit.point.z + 1, (int) hit.point.x + 1);
		}

		return null;
	}

	public void OnMouseEventReceived(object sender, MouseEvent element)
	{
		switch (element)
		{
			case MouseEvent.Down:
								
				var square = ((BoardItem)sender).CurrentSquare;
				
				if (!Referee.CanMove(square)) return;
			
				DraggingChessman = GetGameObject(square);
				
				if (DraggingChessman)
				{
					DraggingStartSquare = square;
				}
				break;
			
			case MouseEvent.Up:
				
				break;
		}
	}

	public void OnGameStatusChanged(GameState newState)
	{
		
	}

	public void OnSpecialUpdateRequired(Square squareToRefresh)
	{
		
		RemoveElementAt(squareToRefresh);
		
		var piece = Referee[squareToRefresh];

		if (piece != null)
		{
			Spawn(piece.Type, piece.Color, squareToRefresh);
		}
		
	}
	
	private void RemoveElementAt(Square square)
	{
		var previous = GetGameObject(square);
		if(previous != null) Destroy(previous);
		OnBoardObjects[square.Row * 8 + square.Column] = null;
	}
}
