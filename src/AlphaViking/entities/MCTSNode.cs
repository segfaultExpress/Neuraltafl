using NeuralTaflGame;

namespace NeuralTaflAi
{
    /// <summary>
    /// The MCTS Node class wraps a Board object and adds values such as:
    /// Children of action board states, number of times visited, the "values" of the node and edge
    /// Used by the MCTS class to train the neural network and to guess board actions 
    /// </summary>
    public class MCTSNode
    {

        // Parent node passes the edge between the itself and the current node, the latter of which stores it
        public double edgeProb {get; set;}
        // The sum of the values gotten from its children
        public double valueSum {get; set;}
        // The number of visits from the MCTS to this node
        public int visits {get; set;}
        // The board and its rules, from pieces to player turn
        public Board boardState {get; set;}

        // The children of this node
        public Dictionary<int, MCTSNode> children {get; set;}
        // A node is initialized unexpanded, then gets expanded on its first visit
        public Boolean isExpanded {get; set;}

        // A slight deviation from alphazero: They save computation by passing back [action, node] in python
        // Then apply the action in the search tree. We will instead store the id, then activate this node
        // when the leaf node is SELECTED
        public int actionId;
        public Boolean isActivated {get; set;}

        public MCTSNode(Board boardState, double edgeProb = 0.0, int valueSum = 0, int visits = 0, int actionId = -1)
        {
            this.edgeProb = edgeProb;
            this.valueSum = valueSum;
            this.visits = visits;
            this.boardState = boardState;

            this.children = new Dictionary<int, MCTSNode>();

            this.isExpanded = false; // Initialized node is not expanded by default
            this.isActivated = actionId == -1; // A null pass indicates that this is already active
        
            this.actionId = actionId;
        }

        /// <summary>
        /// The MCTS Node class wraps a Board object and adds values such as:
        /// Children of action board states, number of times visited, the "values" of the node and edge
        /// Used by the MCTS class to train the neural network and to guess board actions 
        /// </summary>
        /// <returns>A boolean of whether the activation was successful or not</returns>
        public Boolean activate()
        {
            if (this.isActivated)
                return this.isActivated;

            this.isActivated = this.boardState.movePiece(this.actionId); // There's always the possibility it fails

            return this.isActivated;
        }

        /// <summary>
        /// Returns the MCTS Node that is the best UCB-score child from this branch
        /// </summary>
        /// <returns>The Node that is the best (UCB-scored) child</returns>
        public MCTSNode getBestChild()
        {
            if (!isExpanded)
                return null;

            double bestScore = Double.NegativeInfinity;
            MCTSNode bestChild = children.Values.First();
            
            foreach (MCTSNode child in children.Values)
            {

                double score = _ucbScore(child);

                if (bestScore < score)
                {
                    bestChild = child;
                    bestScore = score;
                }
            }

            return bestChild;
        }

        /// <summary>
        /// This tree node has unexpanded subnodes which (probably) continue the game, and this game "reveals" (creates) them
        /// </summary>
        /// <param name="nnet">The neural network by which to judge the newly expanded nodes</param>
        /// <returns>void</returns>
        public void expand(NNet nnet = null)
        {
            if (!isActivated) // Only activated nodes can be expanded
            {
                Console.WriteLine("Error: Only activated nodes can be expanded! Please execute this node's move first!");
                return;
            }

            NNData data;
            if (nnet == null)
            {
                // NN is null, just make a flat policy
                double[] simPolicies = boardState.getValidMove1DArray().Select(x => (double) x).ToArray();
                data = new NNData(boardState.getBoard1DArray(), simPolicies, 0.0);
            }
            else
            {
                data = nnet.predict(boardState);
            }

            int[] validMoves = boardState.getValidMove1DArray();


            double[] validPolicies = data.policy.Select((value, index) => validMoves[index] * value).ToArray();
            // Renormalize the policies subject to the masking
            double sum = validPolicies.Sum();
            double[] normValidPolicies = validPolicies.Select(value => value / sum).ToArray();

            int actionIdChild = -1;
            foreach (double probOfNode in normValidPolicies)
            {
                actionIdChild++;
                // Skip invalid nodes
                if (probOfNode == 0)
                    continue;
            
                Board newBoard = boardState.clone();
                
                /*
                if (!newBoard.MovePiece(actionId)) // Move piece, it SHOULD succeed if "validMoves" is working properly
                {
                    Console.WriteLine("Error: Piece has valid move id: {0} but failed!", actionId);
                    continue;
                }
                */

                MCTSNode newNode = new MCTSNode(newBoard, edgeProb: probOfNode, actionId: actionIdChild);
                children.Add(actionIdChild, newNode);
            
            }

            isExpanded = true;
        }
        
        /// <summary>
        /// A scoring algorithm to determine exploring a child node, based on probability weighed by how many current visits
        /// </summary>
        /// <param name="child">The child to judge</param>
        /// <returns>The UCB score</returns>
        private double _ucbScore(MCTSNode child)
        {
            // The edge probability between this and the child, multiplied by sqrt of how many visits to this node over the child's node
            // It's weighing it by how much the NN liked it, weighed by if we've already visited it a ton
            double priorScore = child.edgeProb * Math.Sqrt(visits) / (child.visits + 1);

            // Think of valueSum of the child directly corresponding to "counterplay", a high valueSum means its direct
            // children did VERY well against its board state, indicating there's a problem with this action
            double score = -1 * (child.visits != 0 ? child.valueSum / child.visits : 0);
            
            return score + priorScore;
        }
    }
}