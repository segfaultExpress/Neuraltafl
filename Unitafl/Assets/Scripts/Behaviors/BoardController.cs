using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeuralTaflGame;

namespace Unitafl
{
    public class BoardController : MonoBehaviour
    {
        // Public assets available to the inspector
        [Space]
        [Header("Assets")]
        public GameObject basePiece;
        public GameObject squareAsset;
        public GameObject[] pieceAssets;
        private List<GameObject> loadedPieceAssets; // At runtime, load the pieces in pieceAssets to 
        public GameObject kingAsset;

        public float pieceHeightOffset;
        public float kingHeightOffset;

        public BoardU board { get; set; }

        public float pieceScale;

        [Space]
        [Header("Renderer Settings")]
        public Material whiteMaterial;
        public Material blackMaterial;
        public Material fireMaterial;
        public Material fireMaterialHighlight;

        // Assets and controllers should be 1:1, so we can store assets on the objects, 
        // and store a hashmap of the other relationship here for fast gets at a small memory cost
        private Dictionary<GameObject, PieceU> pieceRefs;

        // When a piece moves, create assets that we should remove on mouseup or cleanups
        private List<GameObject> ghostPieces;
        private List<SquareU> validSquares;
        private PieceU movingPiece;
        private SquareU closestSquare;

        // TODO: The initial load of the game will have the pieces above the board, spawn them in and plop them onto the board at random
        private List<PieceU> piecesToAnim;
        public int PieceAnimEntranceHeight = 100;

        /// <summary>
        /// On load, the board is populated with squares and pieces
        /// </summary>
        void Start()
        {
            pieceRefs = new Dictionary<GameObject, PieceU>();
            ghostPieces = new List<GameObject>();
            validSquares = new List<SquareU>();
            movingPiece = null;
            closestSquare = null;

            loadedPieceAssets = new List<GameObject>();
            GameObject kingPiece = Instantiate(kingAsset);
            kingPiece.AddComponent<MeshCollider>();
            kingPiece.SetActive(false);
            loadedPieceAssets.Add(kingPiece);
            foreach (GameObject pieceObj in pieceAssets)
            {
                GameObject newPiece = Instantiate(pieceObj);
                newPiece.AddComponent<MeshCollider>();
                newPiece.SetActive(false);
                loadedPieceAssets.Add(newPiece);
            }

            initBoard();

        }

        /// <summary>
        /// Temporary hack function built to simulate an AI playing as the defenders
        /// </summary>
        public void moveAi()
        {
            int aiPlayer = 1;

            int i = 0;
            bool moveSucceed = false;
            while (!moveSucceed && i < 100)
            {
                // Hacky ai
                List<Piece> pieces = board.getOwnerPieces(aiPlayer);
                Piece piece = pieces[Random.Range(0, pieces.Count)];
                
                movingPiece = board.piecesU[new Vector2(piece.row, piece.column)];

                List<string> validMoves = board.getValidMoves(piece);

                var moveIdx = validMoves[Random.Range(0, validMoves.Count)];
                var moveIdxArray = moveIdx.Split(',');
                int x = int.Parse(moveIdxArray[0]);
                int y = int.Parse(moveIdxArray[1]);

                closestSquare = board.squaresU[new Vector2(x, y)];

                moveSucceed = move(isAi: true);
                i++;
            };
        }

        /// <summary>
        /// Initialize the board by creating squares, pieces, and the backend logic
        /// </summary>
        public void initBoard()
        {
            board = new BoardU(new Vector3(0, 0, 0));

            // Initialize all objects that need to be initialized to play the game
            foreach (SquareU square in board.squaresU.Values)
            {
                Debug.Log(string.Format("Creating Square at {0}", square.grid));
                GameObject squareObj = Instantiate(squareAsset, transform);
                squareObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                squareObj.transform.localPosition = square.loc;
                // squareObj.SetActive(false); // Hide these objects and only activate when moving a piece (partical effect)

                square.squareObj = squareObj;
            }
            
            // Just realized there's no quick way of identifying corners or thrones... whoops
            // Until refactoring time, just use the board array
            int[] boardArray = board.getBoard1DArray();

            // Game logic is handled by the ddl, we just need to handle graphical effects and triggers
            foreach (Vector2 xy in board.piecesU.Keys)
            {
                PieceU piece = board.piecesU[xy];
                SquareU square = board.squaresU[xy];

                if (boardArray[piece.row * board.nCols + piece.column] == 4)
                {
                    continue; // The above is a hack to find if it's a corner piece or throne (TODO: pls fix)
                }

                piece.pieceObj = createPiece(piece, square);
            }
        }

        /// <summary>
        /// Create a piece in the game
        /// </summary>
        /// <param name="piece">A PieceU object that contains game data to portray in the game board</param>
        /// <param name="square">A SquareU object which is where the piece should be placed above</param>
        /// <param name="isGhost">Ghost pieces can be used to show fake piece locations on the board</param>
        /// <returns>A GameObject that pertains to the created piece</returns>
        private GameObject createPiece(PieceU piece, SquareU square, bool isGhost = false)
        {
                Vector3 loc = getPieceLocationOnSquare(piece, square);

                // Get the piece asset
                GameObject asset;
                if (piece.isKing)
                {
                    asset = loadedPieceAssets[0];
                }
                else
                {
                    asset = loadedPieceAssets[Random.Range(1, pieceAssets.Length)];
                }

                // Place the piece in the world
                Debug.Log(string.Format("Creating Piece at {0}", square.grid));
                GameObject pieceObj = Instantiate((isGhost ? piece.pieceObj : basePiece), square.squareObj.transform);
                pieceObj.transform.localPosition = loc;
                pieceObj.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                pieceObj.transform.localScale = new Vector3(pieceScale, pieceScale, pieceScale);

                if (isGhost)
                {
                    pieceObj.GetComponent<Renderer>().material = fireMaterial;
                }
                else
                {
                    pieceObj.GetComponent<MeshCollider>().sharedMesh = asset.GetComponent<MeshCollider>().sharedMesh;
                    pieceObj.GetComponent<MeshFilter>().mesh = asset.GetComponent<MeshFilter>().mesh;

                    pieceObj.GetComponent<Renderer>().material = (piece.owner == 1 ? whiteMaterial : blackMaterial);
                    pieceObj.AddComponent<SphereCollider>();
                    pieceObj.SetActive(true);

                    pieceRefs.Add(pieceObj, piece);
                }

                return pieceObj;
        }

        /// <summary>
        /// Remove a piece from the board
        /// </summary>
        /// <param name="piece">A PieceU object that contains game data to remove</param>
        private void removePiece(PieceU piece)
        {
            board.removePieceU(piece);

            StartCoroutine(animateDestroyedPiece(piece));

        }

        /// <summary>
        /// Helper function where, given a piece and a square, returns the piece's location on the board
        /// Currently, it is a child of a parent square, so it should be a vector of type (0, ~0.5, 0)
        /// </summary>
        /// <param name="piece">A PieceU object that contains game data</param>
        /// <param name="square">A SquareU object the piece is placed under</param>
        /// <returns>A Vector3 of game location</returns>
        private Vector3 getPieceLocationOnSquare(PieceU piece, SquareU square)
        {
            float offset = (piece.isKing ? kingHeightOffset : pieceHeightOffset);

            return new Vector3(0, offset, 0);
        }

        /// <summary>
        /// Selects a piece, raising a flag for highlight data
        /// </summary>
        /// <param name="gameObject">A game object, piece, to be selected</param>
        public void selectPiece(GameObject gameObject)
        {
            PieceU piece = pieceRefs[gameObject];

            List<string> validMoves = board.selectPiece(piece.row, piece.column);

            movingPiece = piece;

            // eminate outwards
            StartCoroutine(animateActivateSquares(piece, validMoves));
        }

        /// <summary>
        /// Unselects a piece, lowering the game update "selection" flag
        /// </summary>
        public void unSelectPiece()
        {
            validSquares.Clear();

            foreach (GameObject ghostPiece in ghostPieces)
            {
                Destroy(ghostPiece);
            }

            foreach (SquareU square in board.squaresU.Values)
            {
                square.activate(false);
            }
            movingPiece = null;
            closestSquare = null;
        }

        /// <summary>
        /// Animates piece entrances
        /// </summary>
        IEnumerator animatePiecesEntrance()
        {

            while (piecesToAnim.Count > 0)
            {
                int pieceIdx = Random.Range(0, piecesToAnim.Count);
                PieceU piece = piecesToAnim[pieceIdx];
                
                piece.animateEntrance(PieceAnimEntranceHeight);

                piecesToAnim.Remove(piece);
                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Tries to move a piece given a (TODO: Add via params) piece (movingPiece) and a square (closestSquare)
        /// </summary>
        /// <param name="isAi">Hack to move via AI vs as a user</param>
        /// <returns>The success of the move</returns>
        public bool move(bool isAi = false)
        {
            int[] preMoveArray = board.getBoard1DArray();

            Vector2 movingTo = closestSquare.grid;

            bool isMoveSuccess = board.move(movingPiece, (int) movingTo.x, (int) movingTo.y);

            if (isMoveSuccess)
            {
                movingPiece.pieceObj.transform.SetParent(closestSquare.squareObj.transform);
                movingPiece.pieceObj.transform.Rotate(new Vector3(0f, 0f, 0f));
                movingPiece.pieceObj.transform.localPosition = getPieceLocationOnSquare(movingPiece, closestSquare);

                // Check for an end of the game, can avoid a lot of post-checking
                int winner = board.checkForWinner();
                if (winner != -1)
                {
                    endGame(winner);
                }


                // Select if a piece needs to be removed (may need dict, string->list e.g. "removals"->[(1, 1), (2, 2)])
                List<Vector2> updates = board.checkForUpdates(preMoveArray);

                Debug.Log(updates);

                foreach (Vector2 update in updates)
                {
                    Debug.Log("Update: " + update);
                    if (board.piecesU.ContainsKey(update))
                    {
                        removePiece(board.piecesU[update]);
                    }
                }
            }

            unSelectPiece();

            if (isMoveSuccess && !isAi)
                moveAi();
                
            return isMoveSuccess;
        }

        /// <summary>
        /// Updates which pieces have special material or activation changes
        /// </summary>
        public void updateHighlight()
        {
            if (movingPiece == null)
                return;

            SquareU prevClosestSquare = closestSquare;

            float closestDist = Mathf.Infinity;
            
            SquareU pieceOrigin = board.squaresU[new Vector2(movingPiece.row, movingPiece.column)];

            // First try the origin, to prevent a forced move
            closestDist = Vector3.Distance(movingPiece.pieceObj.transform.position, pieceOrigin.squareObj.transform.position);
            closestSquare = pieceOrigin;

            foreach (SquareU square in validSquares)
            {
                // calculate the distance for every piece (on HUUUUUGE boards this would be a problem to update every frame, here it's 22 calcs worst case)
                float dist = Vector3.Distance(movingPiece.pieceObj.transform.position, square.squareObj.transform.position);

                if (closestDist > dist)
                {
                    closestDist = dist;
                    closestSquare = square;
                }
            }

            if (prevClosestSquare == closestSquare)
                return;

            if (prevClosestSquare != null && prevClosestSquare != pieceOrigin)
                prevClosestSquare.highlight(false);
            
            if (closestSquare != null && closestSquare != pieceOrigin)
                closestSquare.highlight();
        }

        /// <summary>
        /// Animates a piece destruction
        /// </summary>
        /// <param name="piece">The piece to be destroyed</param>
        IEnumerator animateDestroyedPiece(PieceU piece)
        {
            // This is an amateur-hour ""animation"" just to make it more than a pop out of existence
            Renderer rend = piece.pieceObj.GetComponent<Renderer>();
            if (rend != null)
                rend.material = fireMaterialHighlight;

            yield return new WaitForSeconds(0.08f);

            if (rend != null)
                rend.material = fireMaterial;

            yield return new WaitForSeconds(0.08f);

            Destroy(piece.pieceObj);
        }
        
        /// <summary>
        /// Animates square highlighting to show "active" squares
        /// </summary>
        /// <param name="piece">The piece to be destroyed</param>
        /// <param name="validMoves">A list of valid moves to be highlighted</param>
        IEnumerator animateActivateSquares(PieceU piece, List<string> validMoves)
        {
            int row = piece.row;
            int col = piece.column;

            for (int i = 1; i < board.nCols; i++)
            {
                if (validMoves.Contains((row - i) + "," + col))
                    _animActivateSquare(piece, board.squaresU[new Vector2(row - i, col)]);
                if (validMoves.Contains((row + i) + "," + col))
                    _animActivateSquare(piece, board.squaresU[new Vector2(row + i, col)]);
                if (validMoves.Contains(row + "," + (col - i)))
                    _animActivateSquare(piece, board.squaresU[new Vector2(row, col - i)]);
                if (validMoves.Contains(row + "," + (col + i)))
                    _animActivateSquare(piece, board.squaresU[new Vector2(row, col + i)]);

                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Helper function to animate a square highlighting to show "active" squares
        /// </summary>
        /// <param name="piece">The piece to be destroyed</param>
        /// <param name="square">A square to be activated</param>
        private void _animActivateSquare(PieceU piece, SquareU square)
        {
            if (movingPiece == null)
                return;
            square.activate();
            validSquares.Add(square);
            ghostPieces.Add(createPiece(piece, square, isGhost: true));
        }
    
        /// <summary>
        /// TODO: The function to call when the game is over, cleans up the pieces and displays a winner
        /// </summary>
        /// <param name="winner">The winner to display</param>
        public void endGame(int winner)
        {
            Debug.Log("I win!!!!");
        }

        /// <summary>
        /// Function called every frame to update animations
        /// </summary>
        void Update()
        {
            // Update the highlight of the closest location to move a piece to
            updateHighlight();
        }
    }
}
