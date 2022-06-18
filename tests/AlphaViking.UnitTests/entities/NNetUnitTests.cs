using NeuralTaflAi;
using NeuralTaflGame;
using FluentAssertions;
using NUnit.Framework;

namespace AlphaVikingUnitTests;

public class NNetUnitTests
{

    Board rootBoard;
    NNet testNet;
    
    List<NNData> trainNNData;
    List<Board> trainBoards;

    [SetUp]
    public void SetupNN()
    {
        // Create a board to start a neural network with (gives dimensions)
        rootBoard = new Board();

        // Create an initial neural network given a standard board
        testNet = new NNet(rootBoard);
    }

    public void createTrainData()
    {
        // Valid moves
        var validMoves = rootBoard.getValidMove1DArray();
        
        // Create x "moves" to train the nn, and then add policies and values to train with
        int numMoves = 10;

        trainNNData = new List<NNData>();
        trainBoards = new List<Board>();

        int indexOfMove = -1;

        for (int i = 0; i < numMoves; i++)
        {
            Board board = rootBoard.clone();
            indexOfMove = Array.IndexOf(validMoves, 1, indexOfMove + 1);

            board.movePiece(indexOfMove);

            double[] fakePolicy = new double[validMoves.Count()];

            // Let's just say that the next best move in these cases is some algorithm, 3 * i + 20
            fakePolicy[3*i + 20] = 1;

            // Moves get worse as you go along
            double value = 1.0 - 0.1 * i;

            trainNNData.Add(new NNData(boardArray: board.getBoard1DArray(), policy: fakePolicy, v: value));

            trainBoards.Add(board);
        }
    }

    [Test]
    public void ShouldCreateNNet()
    {
        // This test fails if the environment is not setup correctly. Eventually (TODO) this will pass in a fresh install
        // Until then, Python3.7 needs to be installed and on the Path variable. Once this is done, run ONE OF THE TWO
        // commands in the installation folder:

        // [.\python -m pip install "tensorflow-gpu==1.15.0"] or [.\python -m pip install "tensorflow-cpu==1.15.0"]
        
        // This is empty since we simply need [Setup] to complete correctly
    }

    [Test]
    public void ShouldNNetTrainAndPredict()
    {
        // Often used function to generate global trainNNData & trainBoards
        createTrainData();

        testNet.train(trainNNData);

        // Hard to test a valid prediction with REAL test data, since NNs can sometimes have crazy reactions to external data
        // However, a predictable result would be to pass back in the boards we made
        NNData prediction1 = testNet.predict(trainBoards[0]);
        NNData prediction2 = testNet.predict(trainBoards[1]);
        NNData prediction3 = testNet.predict(trainBoards[2]);

        // Better fit data will have better results than this
        int bestMove1 = Array.IndexOf(prediction1.policy, prediction1.policy.Max());
        ((bestMove1 - 20) % 3).Should().Be(0);
        prediction1.v.Should().BeGreaterThan(0.9);

        int bestMove2 = Array.IndexOf(prediction2.policy, prediction2.policy.Max());
        ((bestMove2 - 20) % 3).Should().Be(0);
        prediction2.v.Should().BeGreaterThan(0.9);

        int bestMove3 = Array.IndexOf(prediction3.policy, prediction3.policy.Max());
        ((bestMove3 - 20) % 3).Should().Be(0);
        prediction3.v.Should().BeGreaterThan(0.9);
    }

    [Test]
    public void ShouldNNetLoadSaveWeights()
    {
        // Often used function to generate global trainNNData & trainBoards
        createTrainData();

        testNet.train(trainNNData);

        // Save the weights into a file
        testNet.save();

        // Load the weights into a second file
        NNet newNet = new NNet(new Board());
        newNet.load();

        // These should now be identical
        NNData predictionOld = testNet.predict(trainBoards[0]);
        NNData predictionNew = newNet.predict(trainBoards[0]);

        predictionOld.v.Should().Be(predictionNew.v);
        predictionOld.policy.Sum().Should().Be(predictionNew.policy.Sum());

    }
}
