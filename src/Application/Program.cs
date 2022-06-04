using System;
using NeuralTaflGame;

namespace NeuralTaflProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new NeuralTaflGame.Board();

            // Create the players
            Player player1 = new Player(0, board);
            Player player2 = new Player(1, board, isAi: true);

            while (board.checkForWinner() == -1)
            {
                Console.WriteLine("Board state:");
                board.printBoard();

                // TODO: Generalize for more players, array of players and some getCurrentPlayer() func
                if (board.playerTurn == player1.id)
                {
                    player1.play();
                }
                else
                {
                    player2.play();
                }
            }
            // Declare a winner!
            Console.WriteLine(String.Format("Player {0} wins!", board.checkForWinner() + 1));
        }
    }
}