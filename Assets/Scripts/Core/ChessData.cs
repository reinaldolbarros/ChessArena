public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }
public enum PlayerColor { White, Black }

public class ChessPiece
{
    public PieceType Type { get; set; }
    public PlayerColor Color { get; }
    public int X { get; set; }
    public int Y { get; set; }

    public ChessPiece(PieceType type, PlayerColor color, int x, int y)
    {
        Type = type;
        Color = color;
        X = x;
        Y = y;
    }
}