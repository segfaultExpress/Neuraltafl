using System;
using UnityEngine;

using NeuralTaflGame;

namespace Unitafl
{
	public class PieceU : Piece
	{
		public Piece piece { get; set; }

		public GameObject pieceObj;

		/// <summary>
		/// Wrapper for a Piece object, contains information pertaining to the Unity game object version of the piece.
		/// Unlike board, we can't capture Piece objects before creation, so this will act as a surrogate game-facing mirror
		/// of the object
		/// TODO: Create "createPiece" function to overwrite, to complete the BoardU extension and to remove the need for "update" function below
		/// </summary>
		/// <param name="piece">The original Piece from the dll code, replicated and wrapped here</piece>
		public PieceU(Piece piece)
		{
			this.piece = piece;
			update();
			Debug.Log("extended piece");
		}
		
		/// <summary>
		/// TODO: Animate
		/// </summary>
		public void animateEntrance(int amountToDrop)
		{
			
		}

		/// <summary>
		/// Since this is a wrapper, the dll can update the piece without updating the PieceU, take that into account by calling "update" afterwards
		/// </summary>
		public void update()
		{
			this.row = this.piece.row;
			this.column = this.piece.column;
			this.owner = this.piece.owner;
			this.isKing = this.piece.isKing;
			this.isThrone = this.piece.isThrone;
			this.capturedEast = this.piece.capturedEast;
			this.capturedWest = this.piece.capturedWest;
			this.capturedNorth = this.piece.capturedNorth;
			this.capturedSouth = this.piece.capturedSouth;
		}
	}
}