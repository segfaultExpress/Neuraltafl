using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NeuralTaflGame;

namespace Unitafl
{
	public class BoardU : Board
	{

		public Dictionary<Vector2, PieceU> piecesU { get; set; }
		public Dictionary<Vector2, SquareU> squaresU { get; set; }

		public Vector3 origin { get; set; }

		/// <summary>
		/// Extension of the Board dll class to wrap create/move/remove functions for Unity game logic
		/// </summary>
		/// <param name="origin">The origin location of this board to place all sub objects in the right location</param>
		public BoardU(Vector3 origin)
		{
			this.origin = origin;
			Debug.Log("extended board");
			Debug.Log(this.nCols + this.nCols);

			initSquaresU();
			initPiecesU();
		}

		/// <summary>
		/// Initialize the squares based on constants, and Board dll size
		/// </summary>
		public void initSquaresU()
		{
			squaresU = new Dictionary<Vector2, SquareU>();

			// The left edge will be the origin minus 1/2 "total width", which is n(squares) + (n-1)(offsets)
			// Same with top edge
			float totalX = this.nRows * Constants.SQUARE_SIZE + (this.nRows - 1) * Constants.OFFSET_SIZE;
			float totalZ = this.nCols * Constants.SQUARE_SIZE + (this.nCols - 1) * Constants.OFFSET_SIZE;

			Vector3 topLeftVec = new Vector3(origin.x - (totalX / 2) + (Constants.SQUARE_SIZE / 2), origin.y, origin.z - (totalZ / 2) + (Constants.SQUARE_SIZE / 2));

			Vector3 currVec = topLeftVec;

			for (int i = 0; i < this.nCols; i++)
			{
				for (int j = 0; j < this.nRows; j++)
				{
					Vector2 gridVec = new Vector2(j, i);

					SquareU newSquare = new SquareU(currVec, gridVec);
					squaresU.Add(gridVec, newSquare);

					// x += one square size and offset
					currVec = new Vector3(currVec.x + Constants.SQUARE_SIZE + Constants.OFFSET_SIZE, currVec.y, currVec.z);
				}

				// x = 0 (topLeftVec.x)
				// z += one square size and offset
				currVec = new Vector3(topLeftVec.x, currVec.y, currVec.z + Constants.SQUARE_SIZE + Constants.OFFSET_SIZE);
			}
		}

		/// <summary>
		/// Initialize the piece wrappers after the dll has created a board
		/// </summary>
		public void initPiecesU()
		{
			piecesU = new Dictionary<Vector2, PieceU>();
			foreach (Piece piece in pieceList)
			{
				int x = piece.row;
				int y = piece.column;

				Vector2 xy = new Vector2(x, y);
				piecesU.Add(xy, new PieceU(piece));
			}
		}

		/// <summary>
		/// Move a piece using Board (dll) logic, wrapping it so as to update the PieceU object and squares
		/// </summary>
		/// <param name="piece">The piece being moved</param>
		/// <param name="row">The new row to move to</param>
		/// <param name="col">The new column to move to</param>
		/// <returns>Whether the move was successful</returns>
		public bool move(PieceU piece, int row, int col)
		{
			Vector2 currentPos = new Vector2(piece.piece.row, piece.piece.column);

			bool isMoveSuccessful = movePiece(piece.piece, row, col);

			if (isMoveSuccessful)
			{
				piecesU.Remove(currentPos);

				Vector2 newPos = new Vector2(row, col);
				// There's some hacky stuff about kings capturing "win spots", to be safe, remove the new location first as well
				piecesU.Remove(newPos);
				piecesU.Add(newPos, piece);
				
				piece.update();
			}

			return isMoveSuccessful;
		}

		/// <summary>
		/// Anything can happen in the logic class, so we will compare to a prior board state and
		/// on-the-fly execute updates
		/// </summary>
		/// <param name="priorBoardState">The last board state before an update</param>
		/// <returns>A List<Vector2> that contains all the updates to be handled with a controller</returns>
		public List<Vector2> checkForUpdates(int[] priorBoardState)
		{

			List<Vector2> updates = new List<Vector2>();
			// 1 or 2 indicates that a piece was removed
			// -5 indicates a new throne, 5 indicates the removal of a throne
			// However, we aren't interested in the logic, just the display, so only focus on 1/2
			int[] boardState = getBoard1DArray();

			for (int i = 0; i < boardState.Length; i++)
			{
				int diff = priorBoardState[i] - boardState[i];

				if (diff == 1 || diff == 2)
				{
					// Unwrap the 1D to 2 using 0-10, 11-21 indexing
					updates.Add(new Vector2(i / nCols, i % nCols));
				}
			} 

			return updates;
		}

		/// <summary>
		/// Remove a piece from the BoardU dictionary
		/// </summary>
		/// <param name="piece">The piece to be removed</param>
		public void removePieceU(PieceU piece)
		{
			piecesU.Remove(new Vector2(piece.row, piece.column));
		}
	}
}
