
using System;

namespace NeuralTaflGame
{
    public class Piece
    {
        public int owner {get; set;}
        public int row {get; set;}
        public int column {get; set;}

        public Boolean isKing {get; set;}
        public Boolean isThrone {get; set;}

        public Piece northPiece { get; set; }
        public Piece southPiece { get; set; }
        public Piece westPiece { get; set; }
        public Piece eastPiece { get; set; }

        // Main reason for having this be a class: Quick captures without having to manually check the board
        public Boolean capturedNorth {get; set;}
        public Boolean capturedSouth {get; set;}
        public Boolean capturedWest {get; set;}
        public Boolean capturedEast {get; set;}

        // public Boolean captured {get; set;} // For now, just remove from the board state

        /// <summary>
        /// Class <c>Piece</c> models a piece under the Board class
        /// This is necessary for several reasons -
        /// Tracks number of pieces for a player in O(pieces) rather than O(n*m) time
        /// Cleanly handles king, throne and corner behaviors without hardcoding
        /// Leaves room for programming variant behaviors, 3+ player, etc.
        /// </summary>
        public Piece(int owner = 0, int column = 0, int row = 0, Boolean isKing = false, Boolean isThrone = false)
        {
            this.owner = owner;
            this.column = column;
            this.row = row;

            this.isKing = isKing;
            this.isThrone = isThrone; // TODO: Had a cool idea, an unmoved king could be both a king AND a throne, say that's key ~6. Then when it moves, isThrone = false and another piece is made

            capturedNorth = false;
            capturedSouth = false;
            capturedWest = false;
            capturedEast = false;
        }

        /// <summary>
        /// Main function for moving a piece from the piece level's perspective. Used to track
        /// various important things 
        /// </summary>
        /// <param name="row">The row to move the piece to</param>
        /// <param name="col">The column to move the piece to</param>
        /// <returns>void</returns>
        public void movePiece(int row = -1, int column = -1)
        {
            // define defaults
            if (row == -1)
                row = this.row;
            if (column == -1)
                column = this.column;

            this.row = row;
            this.column = column;
        }

        /// <summary>
        /// Handles capture booleans to raise captured flag 
        /// </summary>
        /// <param name="ignoreNS">Boolean for a EW style capture, preventing a false positive</param>
        /// <param name="ignoreEW">Boolean for a NW style capture, preventing a false positive</param>
        /// <returns>void</returns>
        public Boolean checkCaptured(Boolean ignoreNS = false, Boolean ignoreEW = false)
        {
            Boolean isCaptured = isKing ? 
                (capturedNorth && capturedSouth && capturedEast && capturedWest) : 
                ((!ignoreNS && capturedNorth && capturedSouth) || (!ignoreEW && capturedEast && capturedWest));

            return isCaptured;
        }
    }
}