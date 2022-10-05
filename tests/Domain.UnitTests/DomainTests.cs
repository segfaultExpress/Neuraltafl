using NeuralTaflGame;
using FluentAssertions;
using NUnit.Framework;

namespace Domain.UnitTests;

public class DomainTests
{
    [Test]
    public void ShouldCreateBoard()
    {
        Board defaultBoard = new Board();

        // Test that all structs are created and correct
        defaultBoard.PieceList.Count().Should().Be(13 + 24 + 4);
    }

    [Test]
    public void ShouldCreateCustomBoard()
    {

        // This should be a VALID board with:
        // 2 player 2 piece2
        // 2 player 1 pieces
        // no victor
        Board newBoard = new Board(new int[][]
        {
            new int[] { 1, 2, 1 },
            new int[] { 0, 3, 0 }
        });

        List<Piece> piecesP1 = newBoard.GetOwnerPieces(0);
        List<Piece> piecesP2 = newBoard.GetOwnerPieces(1);

        piecesP1.Count().Should().Be(2);
        piecesP2.Count().Should().Be(2);

        newBoard.NCols.Should().Be(3);
        newBoard.NRows.Should().Be(2);

        newBoard.ValidateBoard().Should().Be(true);
    }

    [Test]
    public void CheckValidBoardState()
    {
        // Should be capable of telling if a board state is invalid

        Board newBoardBadArray = new Board(new int[][]
        {
        });

        newBoardBadArray.ValidateBoard().Should().Be(false);

        Board newBoardNoKing = new Board(new int[][]
        {
            new int[] { 1, 2, 1 },
            new int[] { 0, 0, 0 }
        });

        newBoardNoKing.ValidateBoard().Should().Be(false);
    }

    [Test]
    public void CheckPieceNotCapturedOnInit()
    {
        // Boards should be initialized without captures
        Board board = new Board(new int[][]
        {
            new int[] { 1, 2, 1 },
            new int[] { 0, 3, 0 }
        });

        // 0,1 should not be captured
        board.GetOwnerPieces(1).Count().Should().Be(2);
    }

    [Test]
    public void CheckPieceMoved()
    {
        Board board = new Board(new int[][]
        {
            new int[] { 0, 1, 0 },
            new int[] { 0, 0, 5 },
            new int[] { 2, 0, 1 },
            new int[] { 0, 0, 3 }
        });

        Piece piece = board.GetPiece(0, 1);
        board.MovePiece(piece, 2, 1);
        
        piece.row.Should().Be(2);
        piece.column.Should().Be(1);

        // Piece can move through the throne
        Piece piece2 = board.GetPiece(2, 2);
        board.MovePiece(piece, 0, 2);
        piece.row.Should().Be(0);
        piece.column.Should().Be(2);

        // King can move onto the throne
        Piece kingPiece = board.GetPiece(3, 2);
        board.MovePiece(kingPiece, 1, 2);
        kingPiece.row.Should().Be(1);
        kingPiece.column.Should().Be(2);
    }

    [Test]
    public void ShouldTurnValueBeRespected()
    {
        // TODO: Fix this test case
        // Player 1 should not be able to make two turns in a row
        Board board = new Board(new int[][]
        {
            new int[] { 1, 2, 1 },
            new int[] { 0, 3, 0 }
        });

        board.PlayerTurn.Should().Be(0);

        Piece piece = board.GetPiece(0, 0);
        Boolean pieceMoved = board.MovePiece(piece, 1, 0);
        pieceMoved.Should().Be(true);

        board.PlayerTurn.Should().Be(1);
        Boolean pieceMoved2 = board.MovePiece(piece, 0, 0);
        pieceMoved2.Should().Be(false);
        
    }

    [Test]
    public void CheckPieceInvalidMove()
    {
        // TODO: Fix this test case, currently there are moves that are valid that shouldn't be

        Board board = new Board(new int[][]
        {
            new int[] { 1, 0, 0 },
            new int[] { 0, 0, 0 },
            new int[] { 1, 1, 0 },
            new int[] { 0, 0, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 0, 0, 0 },
            new int[] { 1, 5, 0 },
        });

        // Can not move piece onto itself
        Piece piece = board.GetPiece(0, 0);
        board.MovePiece(piece, 0, 0);
        board.PlayerTurn.Should().Be(0); // Turn did not pass, move is invalid
        
        // Can not move piece out of bounds
        board.MovePiece(piece, 0, 5);
        piece.row.Should().Be(0);
        piece.column.Should().Be(0);
        
        // Can not move diagonally
        board.MovePiece(piece, 1, 1);
        piece.row.Should().Be(0);
        piece.column.Should().Be(0);

        // Can not move piece onto another piece
        board.MovePiece(piece, 2, 0);
        piece.row.Should().Be(0);
        piece.column.Should().Be(0);
        
        // Can not move piece THROUGH another piece
        board.MovePiece(piece, 3, 0);
        piece.row.Should().Be(0);
        piece.column.Should().Be(0);
        
        // Can not move piece THROUGH another piece (horizontal)
        Piece piece2 = board.GetPiece(2, 0);
        board.MovePiece(piece2, 2, 0);
        piece2.row.Should().Be(2);
        piece2.column.Should().Be(0);
        
        // Piece can not move onto another team's piece
        board.MovePiece(piece2, 4, 0);
        piece2.row.Should().Be(2);
        piece2.column.Should().Be(0);
        
        // Piece can not move THROUGH another team's piece
        board.MovePiece(piece2, 5, 0);
        piece2.row.Should().Be(2);
        piece2.column.Should().Be(0);

        // Piece can not move onto a throne
        Piece piece3 = board.GetPiece(6, 0);
        board.MovePiece(piece3, 6, 1);
        piece3.row.Should().Be(6);
        piece3.column.Should().Be(0);
    }

    [Test]
    public void ShouldPieceCapture()
    {
        Board board = new Board(new int[][]
        {
            new int[] { 0, 1, 0 },
            new int[] { 0, 0, 0 },
            new int[] { 0, 2, 3 },
            new int[] { 0, 1, 0 }
        });

        Piece piece = board.GetPiece(0, 1);
        board.MovePiece(piece, 1, 1);
        
        // There is now only the king for 1
        board.GetOwnerPieces(1).Count().Should().Be(1);

        // Also need to test double capture
        Board boardDoubleCap = new Board(new int[][]
        {
            new int[] { 0, 1, 0 },
            new int[] { 0, 2, 0 },
            new int[] { 1, 0, 3 },
            new int[] { 0, 2, 0 },
            new int[] { 0, 1, 0 }
        });

        Piece pieceDoubleCap = boardDoubleCap.GetPiece(2, 0);
        boardDoubleCap.MovePiece(pieceDoubleCap, 2, 1);
        
        // There is now only the king for 1
        boardDoubleCap.GetOwnerPieces(1).Count().Should().Be(1);
    }

    [Test]
    public void ShouldPieceNotCaptureFalsePositive()
    {
        Board board = new Board(new int[][]
        {
            new int[] { 0, 1, 0 },
            new int[] { 0, 2, 0 },
            new int[] { 1, 1, 3 },
            new int[] { 0, 0, 0 }
        });

        Piece piece = board.GetPiece(2, 0);
        board.MovePiece(piece, 1, 0);
        
        // The piece moves up but should not capture
        board.GetOwnerPieces(1).Count().Should().Be(2);
    }

    [Test]
    public void CheckPieceIsRemoved()
    {
        // If 1 captures the 2 below, the other 2 should not be able to recapture the 1
        Board board = new Board(new int[][]
        {
            new int[] { 0, 1, 2, 0 },
            new int[] { 0, 0, 0, 0 },
            new int[] { 0, 2, 3, 2 },
            new int[] { 0, 1, 0, 0 }
        });

        int numPiecesP2 = board.GetOwnerPieces(1).Count();

        Piece piece1 = board.GetPiece(0, 1);
        board.MovePiece(piece1, 1, 1);
        // One piece removed
        board.GetOwnerPieces(1).Count().Should().Be(numPiecesP2 - 1);

        // P2 will now make what should be a blunder follow-up, which should not recapture
        int numPiecesP1 = board.GetOwnerPieces(0).Count();
        Piece piece2 = board.GetPiece(0, 2);
        board.MovePiece(piece2, 0, 1);
        board.GetOwnerPieces(0).Count().Should().Be(numPiecesP1);

    }

    [Test]
    public void CheckCapturedKingWin()
    {
        Board board = new Board(new int[][]
        {
            new int[] { 0, 0, 0 },
            new int[] { 1, 0, 0 },
            new int[] { 1, 3, 1 },
            new int[] { 0, 1, 0 }
        });

        Piece piece = board.GetPiece(1, 0);
        board.MovePiece(piece, 1, 1);

        board.CheckForWinner().Should().Be(0);
    }

    [Test]
    public void CheckKingEscapeWin()
    {
        Board board = new Board(new int[][]
        {
            new int[] { 0, 0, 0 },
            new int[] { 1, 0, 0 },
            new int[] { 1, 0, 3 },
            new int[] { 0, 1, 0 }
        });

        Piece piece = board.GetPiece(2, 2);
        board.MovePiece(piece, 0, 2);

        board.CheckForWinner().Should().Be(1);
    }

    [Test]
    public void CheckFortWin()
    {
        // TODO: Fix this test case
        Board board = new Board(new int[][]
        {
            new int[] { 0, 0, 0 },
            new int[] { 1, 2, 0 },
            new int[] { 1, 2, 3 },
            new int[] { 0, 2, 0 },
            new int[] { 1, 0, 2 },
            new int[] { 0, 1, 0 }
        });

        board.CheckForWinner().Should().Be(-1);

        Piece piece = board.GetPiece(1, 1);
        board.MovePiece(piece, 1, 2);

        board.CheckForWinner().Should().Be(1);


        Board boardHorizontalFort = new Board(new int[][]
        {
            new int[] { 1, 1, 1, 1, 1, 1 },
            new int[] { 1, 1, 0, 0, 0, 1 },
            new int[] { 0, 0, 2, 2, 2, 0 },
            new int[] { 0, 2, 3, 0, 0, 0 }
        });

        boardHorizontalFort.CheckForWinner().Should().Be(-1);

        Piece piece2 = boardHorizontalFort.GetPiece(4, 2);
        boardHorizontalFort.MovePiece(piece, 4, 3);

        boardHorizontalFort.CheckForWinner().Should().Be(1);

        Board boardHorizontalLongFort = new Board(new int[][]
        {
            new int[] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            new int[] { 1, 1, 0, 0, 0, 1, 0, 2, 0, 0, 0 },
            new int[] { 0, 0, 2, 2, 2, 2, 2, 0, 2, 0, 0 },
            new int[] { 0, 2, 3, 0, 0, 0, 0, 0, 0, 2, 0 }
        });

        boardHorizontalLongFort.CheckForWinner().Should().Be(-1);

        Piece piece3 = boardHorizontalLongFort.GetPiece(7, 1);
        boardHorizontalLongFort.MovePiece(piece, 7, 2);

        boardHorizontalLongFort.CheckForWinner().Should().Be(1);
    }

    [Test]
    public void CheckShieldWallCapture()
    {
        // TODO: Fix this test case
        Board board = new Board(new int[][]
        {
            new int[] { 0, 0, 3, 0 },
            new int[] { 1, 0, 1, 0 },
            new int[] { 1, 0, 1, 2 },
            new int[] { 0, 0, 1, 2 },
            new int[] { 1, 0, 0, 1 },
            new int[] { 0, 0, 1, 0 }
        });

        int countP2 = board.GetOwnerPieces(1).Count();
        Piece piece = board.GetPiece(1, 2);
        board.MovePiece(piece, 1, 3);
        int countP2After = board.GetOwnerPieces(1).Count();

        countP2After.Should().Be(countP2 - 2);

        Board boardHorizontalShieldWall = new Board(new int[][]
        {
            new int[] { 1, 1, 1, 1, 1, 1 },
            new int[] { 1, 1, 3, 0, 0, 1 },
            new int[] { 0, 0, 2, 2, 2, 0 },
            new int[] { 0, 2, 1, 1, 0, 0 }
        });

        int countP1 = board.GetOwnerPieces(1).Count();
        Piece piece2 = boardHorizontalShieldWall.GetPiece(4, 2);
        boardHorizontalShieldWall.MovePiece(piece2, 4, 3);
        int countP1After = board.GetOwnerPieces(1).Count();

        countP1After.Should().Be(countP1 - 2);

        Board boardHorizontalLongFort = new Board(new int[][]
        {
            new int[] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            new int[] { 1, 1, 3, 0, 0, 1, 0, 2, 0, 0, 0 },
            new int[] { 0, 0, 2, 2, 2, 2, 2, 0, 2, 0, 0 },
            new int[] { 0, 2, 1, 1, 1, 1, 1, 1, 1, 2, 0 }
        });

        boardHorizontalLongFort.CheckForWinner().Should().Be(-1);

        int countP1Long = board.GetOwnerPieces(1).Count();
        Piece piece3 = boardHorizontalLongFort.GetPiece(7, 1);
        boardHorizontalLongFort.MovePiece(piece3, 7, 2);
        int countP1AfterLong = board.GetOwnerPieces(1).Count();

        countP1After.Should().Be(countP1 - 7);

        Board boardHorizontalLongFortGap = new Board(new int[][]
        {
            new int[] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            new int[] { 1, 1, 3, 0, 0, 1, 0, 2, 0, 0, 0 },
            new int[] { 0, 0, 2, 2, 2, 2, 2, 0, 2, 0, 0 },
            new int[] { 0, 2, 1, 0, 1, 1, 1, 1, 1, 2, 0 }
        });

        boardHorizontalLongFortGap.CheckForWinner().Should().Be(-1);

        int countP1LongGap = board.GetOwnerPieces(1).Count();
        Piece piece4 = boardHorizontalLongFortGap.GetPiece(7, 1);
        boardHorizontalLongFortGap.MovePiece(piece4, 7, 2);
        int countP1AfterLongGap = board.GetOwnerPieces(1).Count();

        countP1After.Should().Be(countP1 - 6); // ??? or equal? You choose.

        // Also - double shield wall capture, is it possible, and if so this needs a test case
    }

}
