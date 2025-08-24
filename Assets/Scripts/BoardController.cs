using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Material lightSquareMaterial;
    [SerializeField] private Material darkSquareMaterial;
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private PiecePrefabs piecePrefabs;
    [SerializeField] private GameObject validMoveIndicator;

    [Header("Settings")]
    [SerializeField] private float yOffset = 0.02f;
    [SerializeField] private float indicatorYOffset = 0.05f;

    private Dictionary<Vector2Int, GameObject> pieceObjects = new Dictionary<Vector2Int, GameObject>();
    private GameObject selectedSquareIndicatorInstance;
    private List<GameObject> validMoveIndicatorInstances = new List<GameObject>();
    private ChessEngine chessEngine;

    public void Initialize(ChessEngine engine)
    {
        chessEngine = engine;
        GenerateBoardVisuals(8, 8);
        GeneratePieceVisuals();
    }

    private void GenerateBoardVisuals(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);

                Material materialToUse = (x + y) % 2 == 0 ? darkSquareMaterial : lightSquareMaterial;
                if (materialToUse != null && tile.GetComponent<Renderer>() != null)
                {
                    tile.GetComponent<Renderer>().material = materialToUse;
                }
            }
        }
    }

    private void GeneratePieceVisuals()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = chessEngine.Board[x, y];
                if (piece != null)
                {
                    CreateVisualPiece(piece);
                }
            }
        }
    }

    private void CreateVisualPiece(ChessPiece piece)
    {
        GameObject prefab = piecePrefabs.GetPrefab(piece.Type, piece.Color);
        if (prefab != null)
        {
            Vector2Int position = new Vector2Int(piece.X, piece.Y);

            // CORREÇÃO: Definir rotação correta para as peças ficarem em pé
            Quaternion rotation = Quaternion.Euler(0, 0, 0); // ou Quaternion.identity

            GameObject pieceObject = Instantiate(
                prefab,
                new Vector3(piece.X, yOffset, piece.Y),
                Quaternion.Euler(-90, 0, 0),
                transform
            );
            pieceObjects.Add(position, pieceObject);
        }
    }

    // Método original do Git - recebe x, y separados (linha 41)
    public void HighlightSquare(int x, int y)
    {
        HideHighlight();

        if (highlightPrefab != null)
        {
            Vector3 position = new Vector3(x, indicatorYOffset, y);
            selectedSquareIndicatorInstance = Instantiate(highlightPrefab, position, Quaternion.identity, transform);
        }
    }

    // Método original do Git - mostra movimentos válidos (linha 42)
    public void ShowValidMoves(List<Vector2Int> moves)
    {
        HideValidMoves();

        if (validMoveIndicator == null)
        {
            Debug.LogWarning("ValidMoveIndicator não está configurado!");
            return;
        }

        Debug.Log($"Mostrando {moves.Count} movimentos válidos");

        foreach (Vector2Int move in moves)
        {
            Vector3 position = new Vector3(move.x, indicatorYOffset, move.y);
            GameObject indicator = Instantiate(validMoveIndicator, position, Quaternion.identity, transform);
            validMoveIndicatorInstances.Add(indicator);
        }
    }

    // Método original do Git - esconde highlight (linha 32)
    public void HideHighlight()
    {
        if (selectedSquareIndicatorInstance != null)
        {
            Destroy(selectedSquareIndicatorInstance);
            selectedSquareIndicatorInstance = null;
        }
    }

    // Método original do Git - esconde movimentos válidos (linha 33)
    public void HideValidMoves()
    {
        foreach (GameObject indicator in validMoveIndicatorInstances)
        {
            if (indicator != null) Destroy(indicator);
        }
        validMoveIndicatorInstances.Clear();
    }

    // Método original do Git - move peças visualmente (linha 27)
    public void MoveVisualPiece(Vector2Int from, Vector2Int to)
    {
        if (pieceObjects.TryGetValue(to, out GameObject capturedPiece))
        {
            Destroy(capturedPiece);
            pieceObjects.Remove(to);
        }

        if (pieceObjects.TryGetValue(from, out GameObject pieceToMove))
        {
            pieceToMove.transform.position = new Vector3(to.x, yOffset, to.y);
            pieceObjects.Remove(from);
            pieceObjects[to] = pieceToMove;
        }
    }
}