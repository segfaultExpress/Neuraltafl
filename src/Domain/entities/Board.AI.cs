using System.Collections;

namespace NeuralTaflGame
{
    // Partial class to handle all wrapper functions needed to play the game as an AI
    public partial class Board
    {
        /// <summary>
        /// The board-level piece move, tracks all peripherals and captures, then handles administrative tasks [Given a move ID]
        /// </summary>
        /// <param name="actionId">The 1D array id of a piece move</param>
        /// <returns>Successfulness of move</returns>
        public Boolean movePiece(int actionId)
        {
            // For more documentation of how this "actionId" works, check out the function getValidMove1DArray
            // actionId is previously generated using this:
            // x1 + y1*this.NCols + x2*this.NRows^2 + y2*this.NCols^3

            int nCols3 = (int) Math.Pow(this.NCols, 3);
            int nRows2 = (int) Math.Pow(this.NRows, 2);
            int nCols1 = this.NCols;  // (int) Math.Pow(this.NCols, 1);
            int nRows0 = 1;           // (int) Math.Pow(this.NRows, 0);

            if (actionId == 187)
            {
                Console.WriteLine("WHOOPSIE");
            }

            int y2 = actionId / (nCols3);
            actionId = actionId % nCols3;

            int x2 = actionId / (nRows2);
            actionId = actionId % nRows2;

            int y1 = actionId / (nCols1);
            actionId = actionId % nCols1;

            int x1 = actionId / (nRows0);

            Piece piece = GetPiece(x1, y1);
            if (piece == null)
            {
                return false;
            }
            

            return MovePiece(piece, x2, y2);
        }

        /// <summary>
        /// Creates a SHA-512 hash for a specific board state (guaranteed unique)
        /// </summary>
        /// 
        /// <returns>SHA-512 hash for the current board state</returns>
        public int createBoardHash()
        {
            return ((IStructuralEquatable)this.BoardArray).GetHashCode(EqualityComparer<int>.Default);
        }

        /// <summary>
        /// Gets the size of the array of all possible actions, aka 0,0->0,1, 0,0->0,2...
        /// </summary>
        /// 
        /// <returns>An int corresponding to the total amount of actions possible</returns>
        public int getActionSize()
        {
            return (int) (Math.Pow(this.NCols, 2) * Math.Pow(this.NRows, 2));
        }

        /// <summary>
        /// Creates a gigantic array where each value within it refers to a different possible move.
        /// </summary>
        /// 
        /// <returns>The giant array that contains all possible moves idx</returns>
        public int[] getValidMove1DArray()
        {
            // Create a massive array for no* reason
            int[] moveArray = new int[this.NRows * this.NCols * this.NRows * this.NCols];

            List<Piece> currentPieces = GetOwnerPieces(this.PlayerTurn);

            foreach (Piece piece in currentPieces)
            {
                List<String> moves = GetValidMoves(piece);
                
                int x1 = piece.row;
                int y1 = piece.column;

                foreach (String move in moves)
                {
                    // TODO: Fix with vectors FTLOG
                    String[] strX2Y2 = new String[2] {"", ""};
                    bool charFlag = false;
                    foreach (char x in move)
                    {
                        if (x == ',')
                        {
                            charFlag = true;
                        }
                        else
                        {
                            strX2Y2[(charFlag ? 1 : 0)] += x;
                        }
                    }

                    int y2;
                    int.TryParse(strX2Y2[1], out y2);

                    int x2;
                    int.TryParse(strX2Y2[0], out x2);

                    // Think of this first in two dimensions - a value has x,y, so in an 11x11 array we would get
                    // [ 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10]
                    // [11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]
                    // ...
                    // A piece on index 12 can be represented in a 1D array as pieceExists[12] = 1. To arbitrarily get these,
                    // i.e. to get index 12 from x=1,y=1 we need (1 + 11*1 - 1). Theoretically, EVERY position like this can move
                    // to EVERY position, so it squares from here for x2, then squares again for y2. x1=1,y1=1->x2=2,y2=2 
                    // becomes (1 + 11*1 + 121*2 + 1331*2 - 1) So, is array value 2915 set to 1? No of course not, pieces don't
                    // move diagonally. But we need to store this somehow. (And moveArray[2794] = 1 btw, TODO: Figure out why)
                    int nRows2 = (int) Math.Pow(this.NRows, 2);
                    int nCols3 = (int) Math.Pow(this.NCols, 3);

                    if (x1 + y1*this.NCols + x2*nRows2 + y2*nCols3 == 187)
                    {
                        Console.WriteLine("whoopsie");
                    }

                    moveArray[x1 + y1*this.NCols + x2*nRows2 + y2*nCols3] = 1;
                }
            }

            return moveArray;
        }

        /// <summary>
        /// Creates the board as a 1D array. Requires deterministic algorithm for input/outputs
        /// </summary>
        /// 
        /// <returns>The giant array that contains every board</returns>
        public int[] getBoard1DArray()
        {
            // Voodoo translation from jagged 2D array to 1d array
            return this.BoardArray.SelectMany(a => a).ToArray();
        }


        public Board clone()
        {
            return new Board(BoardArray, PlayerTurn);
        }
    }
}