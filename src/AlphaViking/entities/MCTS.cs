using System;
using NeuralTaflGame;

namespace NeuralTaflAi
{
    /// <summary>
    /// The MCTS class stores all data related to a tree structure of board states
    ///
    /// It contains an associated Neural Network, which assists in both predicting node weights
    /// and searching for the next leaf node to expand the tree
    /// </summary>
    public class MCTS
    {

        // These values are Dictionary values where the key is either a State (s), or a (State, Action) (sa) vector:
        // The value is some value pertaining to a node (s)/edge (sa) weight or value.
        // 
        // State: The board state, represented as some unique value
        // Action: The action being taken
        public MCTSNode rootNode;

        public NNet nnet {get; set;}

        public MCTS(NNet nnet)
        {
            this.rootNode = new MCTSNode(new Board());

            this.nnet = nnet;
        }

        /// <summary>
        /// This will be called every "play" for an AI turn. It's goals are:
        /// SELECT: Go find a leaf node using UCB (see scoreUCB for more info)
        /// EXPAND: Expand the network to every valid move possible for an arbitrary node
        /// BACKUP: Propegate the results back up the tree
        /// </summary>
        /// <param name="board">A board to start the value from</param>
        /// <param name="iterations">The number of iterations to run through</param>
        /// <returns>The root MCTSNode, containing the rest of the tree</returns>
        public MCTSNode run(Board board = null, int iterations = 0)
        {
            if (iterations <= 0)
            {
                iterations = Constants.NUM_TOTAL_ITERS;
            }

            if (board == null)
            {
                board = new Board();
            }

            List<MCTSNode> nodeStack = new List<MCTSNode>();
            rootNode = new MCTSNode(board);
            rootNode.expand(nnet);


            for (int i = 0; i < iterations; i++)
            {
                // Reset tree and entire process
                MCTSNode node = rootNode;
                nodeStack = new List<MCTSNode>() {node};
                

                // SELECT: Go down a branch path
                MCTSNode parent = rootNode;
                while (node.isExpanded)
                {
                    parent = node; // What is this, leetcode?
                    node = node.getBestChild();
                    nodeStack.Add(node);
                }

                if (i == 150)
                {
                    Console.WriteLine("WHAT");
                }

                // In standard AlphaZero MCTS, this is when the parent board is passed to a treenode. Our trees instead have those boards already cloned
                // So instead, we will activate them here
                node.activate();

                // Check for a winner post-move
                int winner = node.boardState.checkForWinner();
                int value = 0;
                if (winner == -1)
                {
                    // EXPAND: The game has not ended, we will have to continue
                    node.expand(nnet);
                }
                else
                {
                    // Can't really "Lose" on your turn in tafl, but generally speaking check for win or loss
                    value = (winner == node.boardState.playerTurn ? 1 : -1);
                }

                // BACKUP: We have expanded a node, it's time to tell the tree what happened
                foreach (MCTSNode backNode in nodeStack)
                {
                    // value is 1 if the winner is 1, -1 if 2
                    // Therefore, when adding valueSum, if it's the same player add value, else subtract
                    Boolean samePlayerPerspective = backNode.boardState.playerTurn == node.boardState.playerTurn;
                    backNode.valueSum += (samePlayerPerspective ? value : -1*value);
                    backNode.visits++;
                }
            }

            return this.rootNode;
        }
    }
}