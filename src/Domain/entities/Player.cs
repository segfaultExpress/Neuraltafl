
using System;
using NeuralTaflGame.Domain.Common;

namespace NeuralTaflGame
{
    public class Player
    {
        // ID of the player, to get the list of pieces, player turn, etc.
        public int id {get; set;}

        // Boolean of whether this is an ai player
        public Boolean isAi {get; set;}

        // The instance of the board tied to the game, so that the player can play the game with player.play()
        private Board board {get; set;}

        /// <summary>
        /// Class <c>Player</c> contains player data and allows for behavior such as "play", potentially (TODO) track
        /// the score, get piece locations, and TODO
        /// </summary>
        public Player(int id, Board board, Boolean isAi = false)
        {
            this.id = id;
            this.board = board;
            this.isAi = isAi;
        }

        /// <summary>
        /// Gets a list of pieces owned by this player
        /// </summary>
        /// <returns>The list of pieces owned by the player</returns>
        public List<Piece> getPieces()
        {
            return board.GetOwnerPieces(id);
        }

        /// <summary>
        /// Play a turn, whether by user input, random, AI.
        /// </summary>
        /// <returns>void</returns>
        public void play()
        {
            if (isAi)
            {
                _playAIBad();
            }
            else
            {
                _playUser();
            }
        }

        /// <summary>
        /// Basic CLI play turn
        /// </summary>
        /// <returns>void</returns>
        private void _playUser()
        {
            Boolean validTurn = false;
            while (!validTurn)
            {
                Console.WriteLine(String.Format("Player {0}'s turn, please select a piece:", board.PlayerTurn + 1));
                
                String turn = Console.ReadLine();

                int[] turnArray = Common.convertVector(turn);

                if (turnArray == null)
                {
                    Console.WriteLine("Error! Invalid turn. Please try again:");
                    continue;
                }

                List<String> validMoves = board.SelectPiece(turnArray[0], turnArray[1]);

                if (validMoves == null)
                {
                    Console.WriteLine("Error! There appear to be no valid moves for that piece.");
                    continue;
                }
                
                Console.WriteLine("Please select a move for this piece:");
                String validMovesStr = "[";
                int i = 0;
                foreach (String location in validMoves)
                {
                    if (i > 0)
                        validMovesStr += ", ";
                    validMovesStr += Common.reverseConvertVector(location);
                    i++;
                }
                validMovesStr += "]";

                Console.WriteLine(validMovesStr);

                String moveTo = Console.ReadLine();
                int[] moveToArray = Common.convertVector(moveTo);

                if (!validMoves.Contains(moveToArray[0] + "," + moveToArray[1]))
                {
                    Console.WriteLine("Error! That is not a valid move for the piece. Please try again.");
                    continue;
                }

                Piece piece = board.GetPiece(turnArray[0], turnArray[1]);
                Boolean moveSuccessful = board.MovePiece(piece, moveToArray[0], moveToArray[1]);

                if (!moveSuccessful)
                {
                    Console.WriteLine("Error! The move did not succeed, please try again.");
                    continue;
                }

                validTurn = true;
            }
        }

        /// <summary>
        /// Basic CLI play turn
        /// </summary>
        /// <returns>void</returns>
        private void _playAIBad()
        {
            Random rnd = new Random();

            List<Piece> pieces = getPieces();

            Boolean turnValid = false;
            while (!turnValid)
            {
                // Pick a piece
                int pieceId = rnd.Next(pieces.Count() - 1);
                Piece piece = pieces[pieceId];

                // Pick a move
                List<String> validMoves = board.GetValidMoves(piece);

                int moveId = rnd.Next(validMoves.Count() - 1);
                // Don't even ask. All I had to do was try "var x = (1, 2)"
                String[] vectorValues = new String[2] {"", ""};
                bool charFlag = false;
                foreach (char x in validMoves[moveId])
                {
                    if (x == ',')
                    {
                        charFlag = true;
                    }
                    else
                    {
                        vectorValues[(charFlag ? 1 : 0)] += x;
                    }
                }

                int row;
                int.TryParse(vectorValues[0], out row);

                int col;
                int.TryParse(vectorValues[1], out col);

                // This AI is cheating lmao, display to see why
                String pieceFrom = Common.reverseConvertVector(piece.row + "," + piece.column);
                String pieceTo = Common.reverseConvertVector(validMoves[moveId]);
                Console.WriteLine(String.Format("AI: {0}->{1}", pieceFrom, pieceTo));

                turnValid = board.MovePiece(piece, row, col);
            }
        }
    }
}