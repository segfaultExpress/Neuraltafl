using System;
using NeuralTaflGame;

namespace NeuralTaflProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new NeuralTaflGame.Board();

            while (board.checkForWinner() == -1)
            {
                Console.WriteLine("Board state:");
                board.printBoard();

                Boolean validTurn = false;
                while (!validTurn)
                {
                    Console.WriteLine(String.Format("Player {0}'s turn, please select a piece:", board.playerTurn + 1));
                    String turn = Console.ReadLine();

                    int[] turnArray = convertTurn(turn);

                    if (turnArray == null)
                    {
                        Console.WriteLine("Error! Invalid turn. Please try again:");
                        continue;
                    }

                    List<String> validMoves = board.selectPiece(turnArray[0], turnArray[1]);

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
                        validMovesStr += reverseConvertTurn(location);
                        i++;
                    }
                    validMovesStr += "]";

                    Console.WriteLine(validMovesStr);

                    String moveTo = Console.ReadLine();
                    int[] moveToArray = convertTurn(moveTo);

                    if (!validMoves.Contains(moveToArray[0] + "," + moveToArray[1]))
                    {
                        Console.WriteLine("Error! That is not a valid move for the piece. Please try again.");
                        continue;
                    }

                    Piece piece = board.getPiece(turnArray[0], turnArray[1]);
                    Boolean moveSuccessful = board.movePiece(piece, moveToArray[0], moveToArray[1]);

                    if (!moveSuccessful)
                    {
                        Console.WriteLine("Error! The move did not succeed, please try again.");
                        continue;
                    }

                    validTurn = true;
                }
            }
            // Declare a winner!
            Console.WriteLine(String.Format("Player {0} wins!", board.checkForWinner()));
        }

        public static String reverseConvertTurn(String turn)
        {
            // Quick dirty way to display the potential moves
            String[] turnValues = turn.Split(",");
            
            int col;
            int.TryParse(turnValues[1], out col);
            char colAscii = (char) (col + 65);

            int row;
            int.TryParse(turnValues[0], out row);
            row++;

            return colAscii + "" + row;

        }

        public static int[] convertTurn(String turn)
        {
            // I'm not going to do extensive validation on these, since hopefully we graduate from text input pretty quickly in the dev cycle
            if (turn.Count() == 0)
                return null;

            // This is actually a really funny college-level assignment
            int col = (int) Convert.ToChar(turn[0]) - 65;
            col = (col > 26) ? col - 32 : col; // lowercase inputs

            int row;
            bool success = int.TryParse(turn.Substring(1), out row);
            if (!success)
                return null;
            row--;

            int[] turnArray = new int[2];
            turnArray[0] = row;
            turnArray[1] = col;

            return turnArray;
        }
    }
}