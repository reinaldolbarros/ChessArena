using UnityEngine;

[CreateAssetMenu(fileName = "PiecePrefabs", menuName = "Chess/Piece Prefabs")]
public class PiecePrefabs : ScriptableObject
{
    [Header("White Pieces")]
    public GameObject whitePawn;
    public GameObject whiteRook;
    public GameObject whiteKnight;
    public GameObject whiteBishop;
    public GameObject whiteQueen;
    public GameObject whiteKing;

    [Header("Black Pieces")]
    public GameObject blackPawn;
    public GameObject blackRook;
    public GameObject blackKnight;
    public GameObject blackBishop;
    public GameObject blackQueen;
    public GameObject blackKing;

    public GameObject GetPrefab(PieceType type, PlayerColor color)
    {
        return type switch
        {
            PieceType.Pawn => color == PlayerColor.White ? whitePawn : blackPawn,
            PieceType.Rook => color == PlayerColor.White ? whiteRook : blackRook,
            PieceType.Knight => color == PlayerColor.White ? whiteKnight : blackKnight,
            PieceType.Bishop => color == PlayerColor.White ? whiteBishop : blackBishop,
            PieceType.Queen => color == PlayerColor.White ? whiteQueen : blackQueen,
            PieceType.King => color == PlayerColor.White ? whiteKing : blackKing,
            _ => null
        };
    }
}