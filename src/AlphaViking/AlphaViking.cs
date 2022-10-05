using System;
using NeuralTaflGame;

using System.Collections.Generic;
using System.Linq;

namespace NeuralTaflAi
{
    /// <summary>
    /// The AlphaViking class contains all the logic needed to train an AlphaZero AI to beat any human.
    ///
    /// To summarize AlphaZero AI - We will start a tree structure (MCTS) with the root node at an initial board state.
    /// We will then create a series of children for each possible future board state. These are also nodes, and
    /// the next step is to traverse to those nodes and make more children nodes. Traveling to the "best" nodes,
    /// we can theoretically create a perfect game with 100% counterplay accounted for.
    /// So, which child nodes do we focus on first/most? Well, if we knew that, we'd be great at the game already :)
    ///
    /// Instead, while we are creating this tree, there will be a grafted neural network (NNET) that will be "telling" 
    /// us to focus on certain nodes. Think of it as a black box that takes in the entire board (11x11 in tafl's case)
    /// and outputs two things: 1. How screwed are you? And 2. In this massive array [0, 0, 0, 0, 0, 0, ...],
    /// which 0 do you like the most? Show us by picking favorites, aka [0.01, 0.04, 0.23, ...].
    ///
    /// ... Wait, what? How does THAT give us good moves? Well, if we're deterministic on which 0 index refers to which
    /// move, say, which *child board state*, the network will start to train itself to pick the board it "wants", aka the
    /// one that makes the #1 variable start showing good numbers. More accurate predictors win over less accurate ones,
    /// and better nodes are chosen more frequently, so the "good play" tree is expanded with a better trained model.
    ///
    /// Eventually, we can even just create any arbitrary board state as a "root" node, load up our latest NNet, and ask:
    /// 1. How screwed are you? and 2. Which move on the array of possible moves do you like? A well trained NNet will
    /// pick the best moves, every time.
    /// </summary>
    public class AlphaVikingController
    {

        private NNet nnet {get; set;}
        private MCTS mcts {get; set;}

        private NNet pnnet {get; set;}
        private MCTS pmcts {get; set;}

        private FixedSizedQueue<NNData> trainDataHistory {get; set;}

        public AlphaVikingController()
        {
            Board rootBoard = new Board();

            this.nnet = new NNet(rootBoard);
            this.mcts = new MCTS(this.nnet);

            // As we learn, the goal is to beat a previous version of the nnet
            // A sufficiently trained net should at least go even with a previously trained version
            this.pnnet = new NNet(rootBoard);
            this.pmcts = new MCTS(this.pnnet);
        }

        /// <summary>
        /// When playing, what is the "best move"? Asking a neural network
        /// </summary>
        /// <param name="board">The board to play on</param>
        /// <returns>The action Id to play</returns>
        public int pickMove(Board board)
        {
            
            List<double> thinkArray = _think(board);

            // No experimentation, just get the max
            int actionId = thinkArray.IndexOf(thinkArray.Max());

            return actionId;
        }

        /// <summary>
        /// When playing, we should always select the best move. When training however, we can be more creative and SOMETIMES go with an overlooked move
        /// </summary>
        /// <param name="policies">The array of moves and current understanding of move quality to decide with</param>
        /// <param name="temperature">The "temperature" of the node</param>
        /// <returns>An actionId that is allowed to not be the "best move"</returns>
        private int _thinkCreative(List<double> policies, int temperature)
        {
            int actionId = 0;

            if (temperature == 0)
            {
                actionId = policies.IndexOf(policies.Max());
            }
            else if (((double) temperature) == double.PositiveInfinity)
            {
                Random random = new Random();
                actionId = random.Next(policies.Count());
            }
            else
            {
                List<double> policyTemp = (List<double>) policies.Select(x => Math.Pow(x, (1 / temperature)));
                policyTemp = (List<double>) policyTemp.Select(x => x / policyTemp.Sum());
                actionId = policyTemp.IndexOf(policyTemp.RandomElementByWeight(x => x));
            }

            return actionId;
        }

        /// <summary>
        /// The "thinking" element of the AI, which does a "run" through the MCTS, then gets the most visited nodes
        /// (most visited means that it keeps visiting this node despite the rising weight of visiting it, indicating "good" paths)
        /// </summary>
        /// <param name="board">The board to think about</param>
        /// <param name="iterations">The number of iterations to run through, defaults to Constants.NUM_TOTAL_ITERS</param>
        /// <returns>A list of action percentages</returns>
        private List<double> _think(Board board, int iterations = -1)
        {
            if (iterations <= 0)
            {
                iterations = Constants.NUM_TOTAL_ITERS;
            }

            // Expand the tree and return the root node
            MCTSNode root = this.mcts.run(board, iterations);

            double[] actionArray = new double[board.getActionSize()];
            int i = 0;
            int sum = 0;
            foreach (MCTSNode node in root.children.Values)
            {
                actionArray[i] = node.visits;
                sum += node.visits;
                i++;
            }

            List<double> actionArrayList = actionArray.Select(piVal => piVal / sum).ToList();

            return actionArrayList;
        }

        /// <summary>
        /// Train the neural network with a bunch of episode training data
        /// </summary>
        /// <returns>void</returns>
        public void train()
        {

            for (int i = 0; i < Constants.NUM_TOTAL_ITERS; i++)
            {
                Console.WriteLine("Starting iteration {0}/{1}", i, Constants.NUM_TOTAL_ITERS);

                if (!Constants.SKIP_FIRST_ITERATION || i > 1)
                {
                    FixedSizedQueue<List<NNData>> iterTrainingData = new FixedSizedQueue<List<NNData>>(Constants.HISTORY_QUEUE_SIZE);
                
                    for (int j = 0; j < Constants.NUM_EPISODES; j++)
                    {
                        iterTrainingData.Append(executeEpisode()); // Play out a game
                    }
                }

                List<NNData> trainDataList = new List<NNData>();
                foreach (NNData trainData in this.trainDataHistory)
                {
                    trainDataList.Append(trainData);
                }

                // Randomize the list
                var rnd = new Random();
                List<NNData> randomizedTrainData = (List<NNData>) trainDataList.OrderBy(item => rnd.Next());
                
                // Time to train based on how the self-play went, save a checkpoint
                this.nnet.save(file: "temp.h5");
                this.pnnet.load(file: "temp.h5");

                this.nnet.train(randomizedTrainData);

                // An additionally trained neural network now fights against its prior self
                double winrate = dualFight(this.mcts, this.pmcts);
            }
        }

        /// <summary>
        /// An "episode" is a game that we go down in self-play
        /// Then train the NN based on the outcome. Drawn, won or lost, this play allows us to
        /// test our _think algorithm and get the actual results. As far as I can tell,
        /// this works because even a poorly trained NN will reach a point in which it can see ALL outcomes
        /// and "lightning strike" its win state.
        /// </summary>
        /// <returns>A list of training data to train the neural network</returns>
        private List<NNData> executeEpisode()
        {
            Board board = new Board();

            List<NNData> trainData = new List<NNData>();

            int episodeStep = 0;
            while (-1 == board.CheckForWinner())
            {
                episodeStep++;

                // Take advantage of the "think" algorithm used when picking a move
                // (we will want to experiment more, however)
                List<double> pi = _think(board);
                
                trainData.Append(new NNData(board.getBoard1DArray(), pi.ToArray(), 0));

                MCTSNode root = this.mcts.rootNode;
                int actionId = _thinkCreative(pi, 0);

                board.movePiece(actionId);
            }

            foreach (NNData trainItem in trainData)
            {
                // Propegate back up the tree
                trainItem.v = (board.CheckForWinner() == 0 ? 1 : -1);
            }

            return trainData;
        }

        /// <summary>
        /// Instead of self-play, fight the previous version of this network. It should win a certain amount more, hopefully
        /// </summary>
        /// <param name="currMCTS">The trained MCTS tree</param>
        /// <param name="currMCTS">The previous MCTS tree to compete against</param>
        /// <returns>The win rate of the current tree search over the previous tree search</returns>
        private double dualFight(MCTS currMCTS, MCTS pastMCTS)
        {
            double numWon = 0;

            // Create a series of games where half are played with <current> as p1, half as p2
            for (int i = 0; i < Constants.DUAL_FIGHT_NUM / 2; i++) {
                // 0 implies p1 won, so add 1 (inverted)
                numWon += (_dualFightGameIter(currMCTS, pastMCTS) == 0 ? 1 : 0);
            }

            for (int i = 0; i < Constants.DUAL_FIGHT_NUM / 2; i++) {
                // 1 implies p1 won, so add 1
                numWon += _dualFightGameIter(pastMCTS, currMCTS);
            }

            return numWon / Constants.DUAL_FIGHT_NUM; 
        }

        /// <summary>
        /// Helper function for the dual fight to execute one game
        /// </summary>
        /// <param name="p1">The trained MCTS tree</param>
        /// <param name="p2">The previous MCTS tree to compete against</param>
        /// <returns>The winner of the game</returns>
        private double _dualFightGameIter(MCTS p1, MCTS p2)
        {
            Board board = new Board();

            while (-1 == board.CheckForWinner())
            {
                int actionId = pickMove(board);

                board.movePiece(actionId);
            }

            return board.CheckForWinner();
        }
    }
}