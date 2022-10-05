using System;
using System.Windows;

namespace NeuralTaflGame
{
    public class Globals
    {
        public static int[][] DEFAULT_BOARD_ARRAY = new int[][]
        {
            new int[] { 4, 0, 0, 1, 1, 1, 1, 1, 0, 0, 4 },
            new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1 },
            new int[] { 1, 0, 0, 0, 2, 2, 2, 0, 0, 0, 1 },
            new int[] { 1, 1, 0, 2, 2, 3, 2, 2, 0, 1, 1 },
            new int[] { 1, 0, 0, 0, 2, 2, 2, 0, 0, 0, 1 },
            new int[] { 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
            new int[] { 4, 0, 0, 1, 1, 1, 1, 1, 0, 0, 4 }
        };
    }
    
    public partial class Board
    {
        int[][] BoardArray;
        public int NRows {get; set;}
        public int NCols {get; set;}


        public int PlayerTurn {get; set;}

        public Piece KingPiece {get; set;}

        public Boolean BoardStateValid {get; set;}

        // Piece list stores an iterable list of pieces
        public List<Piece> PieceList {get;}

        // Piece dictionary is useful for quickly grabbing the existing piece for a row/column (blind spot of simple lists)
        public Dictionary<String, Piece> PieceDict {get;} // TODO (Matt) - Change this to Vector (requires WindowsBase)

        public Board(int[][] initBoardArray = null, int playerTurn = 0)
        {
            // See config (TODO? List of jobs for Matt) but the configuration behavior is as follows:
            // 1: Attacker piece
            // 2: Defender piece
            // 3: King, defender
            // 4: Corner pieces
            // 5: Throne, left behind by king's first move (assuming has not moved)

            if (initBoardArray == null)
                initBoardArray = Globals.DEFAULT_BOARD_ARRAY;



            this.PlayerTurn = playerTurn;

            PieceDict = new Dictionary<string, Piece>();
            PieceList = new List<Piece>();
            
            // TODO: Error handling and logging alerting the user that the board has not been created properly
            BoardStateValid = InitBoard(initBoardArray);
        }


        /// <summary>
        /// Initialization of a playable board, adds pieces and stores them in object structs for viewing later
        /// </summary>
        /// <param name="boardArray">The desired board state in a 2darray format</param>
        /// <returns>Validation of an initialized and valid board</returns>
        public Boolean InitBoard(int[][] initBoardArray)
        {
            PieceDict.Clear();
            PieceList.Clear();
            KingPiece = null;

            // Before we place anything, let's create an empty board that we can populate from the above array
            // The two SHOULD be identical afterwards (tested in unit tests)
            this.NRows = initBoardArray.Count();
            // Check later that anything other than this is invalid
            this.NCols = 1;
            if (initBoardArray.Count() > 0)
                this.NCols = initBoardArray[0].Count();

            this.BoardArray = new int[NRows][];
            for (int i = 0; i < NRows; i++)
            {
                for (int j = 0; j < NCols; j++)
                {
                    this.BoardArray[i] = new int[NCols];
                }
            }

            // Run through the board and add pieces for each non-zero board array
            int rowIdx = 0;
            foreach (int[] boardRow in initBoardArray )
            {
                int colIdx = 0;
                foreach (int boardValue in boardRow)
                {
                    if (boardValue == 0)
                    {
                        // place nothing
                        colIdx++;
                        continue;
                    }

                    int owner = 1;
                    Boolean isKing = false;
                    switch(boardValue) 
                    {
                    case 1:
                        owner = 0;
                        break;
                    case 2:
                        owner = 1;
                        break;
                    case 3:
                        owner = 1;
                        isKing = true;
                        break;
                    case 4:
                        owner = -1; // This should create some special behaviors in the piece class
                        break;
                    case 5:
                        owner = -1; // This should create some special behaviors in the piece class
                        break;
                    default:
                        // code block
                        break;
                    }

                    Piece piece = new Piece(owner: owner, column: colIdx, row: rowIdx, isKing: isKing);

                    if (isKing)
                    {
                        // track the king piece for board validation and O(1) win conditions
                        KingPiece = piece;
                    }

                    AddPiece(piece, captureAllowed: false); // A board state with [2, 1, 2] could see a capture we wouldn't immediately want

                    colIdx++;
                }
                rowIdx++;
            }

            return ValidateBoard();
        }

        /// <summary>
        /// Validate the board and make sure that it is valid. Invalid board states include - no king, sub-array uneven, TODO: think of other board states
        /// </summary>
        /// <param name="row">The row for the piece</param>
        /// <param name="col">The column for the piece</param>
        /// <returns>A list of valid moves for the selected piece, or null if the piece is unselectable (wrong owner, nonexistent)</returns>
        public Boolean ValidateBoard()
        {
            if (BoardArray.Count() < 1)
            {
                return false;
            }

            // There should be a KingPiece (future - kingpieces??)
            if (KingPiece == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check for win conditions. Win conditions include 
        /// Attacker: King removed, [pro-gamer-TODO] corners completely surrounded and no way for defense to win
        /// Defender: King in corner, less attackers than can capture the king, TODO: Fort victory
        /// </summary>
        /// <returns>-1 if no winner, else the id of the player that won</returns>
        public int CheckForWinner()
        {
            // attacker wins
            if (!PieceList.Contains(KingPiece))
            {
                // has been removed, can win
                return 0;
            }

            // defense wins
            Boolean kingInCorner = (KingPiece.row == 0 || KingPiece.row == NRows - 1) && 
                                   (KingPiece.column == 0 || KingPiece.column == NCols - 1);
            if (kingInCorner || GetOwnerPieces(0).Count() < 4)
            {
                return 1;
            }

            // TODO: Fort victory

            /* Pseudocode impl.
            Boolean kingOnSide = (KingPiece.row == 0 || KingPiece.row == NRows - 1) || 
                                 (KingPiece.column == 0 || KingPiece.column == NCols - 1);
        
            if !kingOnSide
                return // save computation

            // check first above/below forts
            Boolean kingCanMove = GetPiece(side of king) == null || GetPiece(other side of king) == null

            Boolean kingProtected = true
            kingProtected = kingProtected && GetPiece(wallRow, KingPiece.column) != null // switch boolean methodology
            
            aboveWallRow = kingRow == 0 ? 1 : NRows - 2 // CRITICAL to understand - we can check both top and bottom at the same time with this var

            leftWallCol = KingPiece.column - 1
            wallPiece = null
            while (wallPiece == null and leftWallCol > 0)
                wallPiece = GetPiece(wallRow, leftWallCol)
                aboveWallPiece = GetPiece(aboveWallRow, leftWallCol)

                if wallPiece == null and aboveWallPiece == null:
                    kingProtected = false // the fort is incomplete

                leftWallCol -= 1;

            rightWallCol = KingPiece.column + 1
            wallPiece = null
            while (wallPiece == null and rightWallCol < NCols)
                wallPiece = GetPiece(wallRow, rightWallCol)
                aboveWallPiece = GetPiece(aboveWallRow, rightWallCol)

                if wallPiece == null and aboveWallPiece == null:
                    kingProtected = false // the fort is incomplete

                rightWallCol -= 1;

            // Repeat same to check sidewall fort wins

            */


            return -1;
        }

        /// <summary>
        /// This function checks if the board has a shield wall capture
        /// </summary>
        /// <param name="???">Whatever you need. Might I suggest the piece itself, so that you can just check around that piece</param>
        /// <returns>void</returns>
        public void CheckShieldWallCapture()
        {
            // System.Console.WriteLine("Shield Wall Capture Check triggered: Good luck lmao");
            /* pseudocode implementation
            row = piece.row
            col = piece.col
            
            wallPiece = GetPiece(wallUnderOrNextToPiece) // need fancy math to make sure that you're checking the wall piece, see below
            // Find X in these cases (. indicates the "1" piece was just moved from that position)
            // [0 0 0 0 0 0]
            // [0 0 1 1 0 0]
            // [0 1 2 X 1 .]

            // [0 0 0 . 0 0]
            // [0 0 1 1 1 0]
            // [0 1 2 X 2 1]

            // Check this recursive idea out, we can pair program this
            isValidShieldWallCapture = _checkShieldWallCapture(wallPiece, GetPiece(wallLeft)) &&
                                       _checkShieldWallCapture(wallPiece, GetPiece(wallRight))

            if isValidShieldWallCapture
                foreach wallpiece
                    RemovePiece(wallPiece)

            public Boolean _checkShieldWallCapture(lastPiece, piece)
                // These are called "base cases" of recursion, they stop the otherwise unending calls of itself
                if piece == null:
                    return False // The function discovered an empty end to the fort, unprotected
                if lastPiece.owner != piece.owner:
                    return True // Fort is closed

                // A fort will have EXACTLY 1 capture for each piece except 2 for the corner. We have a corner check, now we just
                // need to make sure that this holds true
                validCapture = piece.capturedNorth || piece.capturedSouth || piece.capturedWest || piece.capturedEast;

                // Use a cool trick of passing the last piece to make sure our recursion doesn't backtrack
                leftPiece = GetPiece(left)
                if (leftPiece != lastPiece)
                    validCapture = validCapture && _checkShieldWallCapture(piece, leftPiece)

                rightPiece = GetPiece(right)
                if (rightPiece != lastPiece)
                    validCapture = validCapture && _checkShieldWallCapture(piece, rightPiece)

                return validCapture;
            */
        }

        /// <summary>
        /// This function selects a piece. I believe this may be necessary for the neural network, as well as any UI showing possible squares to move a piece
        /// </summary>
        /// <param name="row">The row for the piece</param>
        /// <param name="col">The column for the piece</param>
        /// <returns>A list of valid moves for the selected piece, or null if the piece is unselectable (wrong owner, nonexistent)</returns>
        public List<String> SelectPiece(int row, int col)
        {
            Piece piece = GetPiece(row, col);
            if (piece == null) // or other problems
                return null;
            
            return GetValidMoves(piece);
        }

        /// <summary>
        /// Gets the valid moves for a specific piece
        /// </summary>
        /// <param name="piece">The piece to get the valid moves for</param>
        /// <returns>A list of valid moves for the selected piece</returns>
        public List<String> GetValidMoves(Piece piece)
        {
            // TODO: Find actual valid moves

            List<String> validMoves = new List<String>();

            for (int i = 0; i < BoardArray.Count(); i++)
            {
                validMoves.Add(i + "," + piece.column);
            }

            for (int j = 0; j < BoardArray[0].Count(); j++)
            {
                validMoves.Add(piece.row + "," + j);
            }
            return validMoves;
        }


        /// <summary>
        /// The board-level piece move, tracks all peripherals and captures, then handles administrative tasks [Given a row and column]
        /// </summary>
        /// <param name="piece">The piece object being moved</param>
        /// <param name="row">The new row for the piece</param>
        /// <param name="col">The new column for the piece</param>
        /// <returns>Successfulness of move</returns>
        public Boolean MovePiece(Piece piece, int row, int col)
        {
            if (0 > row || row >= NRows || 0 > col || col >= NCols)
            {
                return false;
            }
            
            // TODO: use existing GetValidMoves to fix CheckPieceInvalidMove test case.
            // Should be some form of 'if (!validMoves.contains(row + "," + column) return false'

            // TODO: If it is not the player's turn, they should not be able to move a piece

            // Strange behavior: "Direct capture", a king can "capture" type 4 pieces by standing on them
            // For cool ideas in the future, just future proof in a "direct capture" flow
            Piece existingPiece = GetPiece(row, col);
            if (piece == KingPiece && existingPiece != null && existingPiece.owner == -1)
            {
                existingPiece = RemovePiece(existingPiece);
            }
            
            // quick validation, should never be true but does currently (valid moves are not defined)
            if (PieceDict.Keys.Contains(String.Format("{0},{1}", row, col)))
                return false;
            
            // Since I'm proposing the class/obj Piece, here's the implementation
            // Don't worry, there's still plenty to do in control flow, win states, capture handling, "shield wall" implementation...
            
            

            // Remove the piece from the board in order to place it in the new spot
            piece = RemovePiece(piece);

            piece.movePiece(row, col);
            piece = AddPiece(piece);

            PlayerTurn = (PlayerTurn == 1) ? 0 : 1;
            return true;
        }

        /// <summary>
        /// Gets the correct piece in O(1) time, given a position
        /// </summary>
        /// <param name="row">The row of the piece we are looking for</param>
        /// <param name="col">The column of the piece we are looking for</param>
        /// <returns>Piece, or null if no such piece exists at that pos (safely handles oob)</returns>
        public Piece GetPiece(int row, int col)
        {
            String key = String.Format("{0},{1}", row, col);
            
            if (!PieceDict.Keys.Contains(key)) // No such key
                return null;

            return PieceDict[key];
        }

        /// <summary>
        /// Gets a list of pieces owned by a specific owner/player as a list
        /// </summary>
        /// <param name="owner">The player/owner of the pieces</param>
        /// <returns>The list of pieces owned by an owner</returns>
        public List<Piece> GetOwnerPieces(int owner)
        {
            List<Piece> ownerPieces = new List<Piece>();

            foreach (Piece piece in PieceList)
            {
                if (piece.owner == owner)
                {
                    ownerPieces.Add(piece);
                }
            }

            return ownerPieces;
        }

        /// <summary>
        /// Removes a piece from the game. Used for step 1 of moving a piece, or capturing it
        /// </summary>
        /// <param name="piece">The piece to be removed</param>
        /// <returns>The removed piece</returns>
        public Piece RemovePiece(Piece piece)
        {
            int row = piece.row;
            int col = piece.column;

            PieceList.Remove(piece);
            PieceDict.Remove(row + "," + col);

            // TODO: If king and HAS NOT MOVED (may need new bool? new 2dArray code for "unmoved king"? Consider "from position" board states)
            /*
            if (piece.isKing && ???)
            {
                BoardArray[row][col] = 5;
            }
            */
            BoardArray[row][col] = 0;

            // All four squares at the old position must have their peripherals updated
            Piece northPiece = GetPiece(row + 1, col);
            Piece southPiece = GetPiece(row - 1, col);
            Piece westPiece = GetPiece(row, col - 1);
            Piece eastPiece = GetPiece(row, col + 1);

            // all old pieces will now have captured{OTHER} set to false
            // TODO: IF a throne is placed instead, these are set to true(?)
            if (northPiece != null)
                northPiece.capturedSouth = false;
            if (southPiece != null)
                southPiece.capturedNorth = false;
            if (westPiece != null)
                westPiece.capturedEast = false;
            if (eastPiece != null)
                eastPiece.capturedWest = false;

            return piece;
        }

        /// <summary>
        /// Function to handle all the administrative behaviors for a piece being added
        /// </summary>
        /// <param name="piece">The piece to be added</param>
        /// <param name="captureAllowed">Whether capturing is allowed. Set to false when initializing a board or taking back a move</param>
        /// <returns>The piece that was added</returns>
        public Piece AddPiece(Piece piece, Boolean captureAllowed = true)
        {
            int row = piece.row;
            int col = piece.column;

            Piece northPiece = GetPiece(row + 1, col);
            Piece southPiece = GetPiece(row - 1, col);
            Piece westPiece = GetPiece(row, col - 1);
            Piece eastPiece = GetPiece(row, col + 1);
            
            // Maybe pieces should handle this? But they don't have peripheral vision. 
            // Considered a Linkedlist approach but realized this isn't 2001 or an interview question
            if (northPiece != null)
            {
                // The true boolean zen - That which you capture, also captures you :-)
                Boolean captureDynamic = (northPiece.owner != piece.owner);
                northPiece.capturedSouth = captureDynamic;
                piece.capturedNorth = captureDynamic;

                if (northPiece.checkCaptured(ignoreEW: true) && captureAllowed)
                {
                    RemovePiece(northPiece);
                    piece.capturedNorth = false;
                }
            }
            if (southPiece != null)
            {
                // BTW - do you notice how sick this can be from this def? A player 3 for example could help P1 capture P2's pieces
                // (3 PLAYER TAFL)
                Boolean captureDynamic = (southPiece.owner != piece.owner); 
                southPiece.capturedNorth = captureDynamic;
                piece.capturedSouth = captureDynamic;

                if (southPiece.checkCaptured(ignoreEW: true) && captureAllowed)
                {
                    RemovePiece(southPiece);
                    piece.capturedSouth = false;
                }
            }
            if (westPiece != null)
            {
                Boolean captureDynamic = (westPiece.owner != piece.owner);
                westPiece.capturedEast = captureDynamic;
                piece.capturedWest = captureDynamic;

                if (westPiece.checkCaptured(ignoreNS: true) && captureAllowed)
                {
                    RemovePiece(westPiece);
                    piece.capturedWest = false;
                }
            }
            if (eastPiece != null)
            {
                Boolean captureDynamic = (eastPiece.owner != piece.owner);
                eastPiece.capturedWest = captureDynamic;
                piece.capturedEast = captureDynamic;

                if (eastPiece.checkCaptured(ignoreNS: true) && captureAllowed)
                {
                    RemovePiece(eastPiece);
                    piece.capturedEast = false;
                }
            }

            // TODO: The hardest part, probably - check for shield wall captures
            // I'll get you into the right function to check, your job should implement the pseudocode
            if ((piece.row == 0 || piece.row == 1 || piece.row == NRows - 1 || piece.row == NRows - 2) &&
                (piece.column == 0 || piece.column == 1 || piece.column == NCols - 1 || piece.column == NCols - 2))
            {
                CheckShieldWallCapture();
            }

            PieceList.Add(piece);
            PieceDict.Add(row + "," + col, piece);
            BoardArray[row][col] = GetArrayCode(piece);

            return piece;
        }

        /// <summary>
        /// Gets the "Array Code" of a piece:
        /// Key:
        /// 0: Empty
        /// 1: Attacker piece
        /// 2: Defender piece
        /// 3: King
        /// 4: Corner
        /// 5: Throne
        /// </summary>
        /// <param name="piece">The piece to be parsed</param>
        /// <returns>void</returns>
        public int GetArrayCode(Piece piece)
        {
            if (piece == null)
                return 0;
            else if (piece.owner == 0)
                return 1;
            else if (piece.owner == 1 && !piece.isKing)
                return 2;
            else if (piece.owner == 1 && piece.isKing)
                return 3;
            else if (piece.owner == -1 && !piece.isThrone)
                return 4;
            else if (piece.owner == -1 && piece.isThrone)
                return 5;
            else
                return -1; // Unknown
        }

        /// <summary>
        /// Function for debugging and printing a cli for the NT board
        /// </summary>
        /// 
        /// <returns>void</returns>
        public void PrintBoard()
        {
            Console.WriteLine("    A,B,C,D,E,F,G,H,I,J,K");
            int j = 0;
            foreach (int[] boardRow in BoardArray)
            {
                Console.Write(String.Format("{0}{1} [", j + 1, (j < 9) ? " " : ""));

                var i = 0;
                foreach (int boardValue in boardRow)
                {
                    if (i > 0)
                    {
                        Console.Write(",");
                    }
                    Console.Write(boardValue);
                    i++;
                }
                Console.WriteLine("]");
                j++;
            }
        }
    }
}