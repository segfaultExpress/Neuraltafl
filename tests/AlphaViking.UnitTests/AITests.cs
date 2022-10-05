using NeuralTaflAi;
using NeuralTaflGame;
using FluentAssertions;
using NUnit.Framework;

namespace AlphaVikingUnitTests;
public class AITests
{

    [Test]
    public void ShouldCreateBoard()
    {
        Board defaultBoard = new Board();

        // Test that all structs are created and correct
        defaultBoard.PieceList.Count().Should().Be(13 + 24 + 4);
    }
}
