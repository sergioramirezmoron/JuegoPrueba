using UnityEngine;

public class BuildSocket : MonoBehaviour
{
    public BuildSocketType socketType;

    private BuildPiece owner;

    public BuildPiece Owner => owner;

    public bool IsOccupied { get; private set; }

    public void Initialize(BuildPiece owningPiece, BuildSocketType targetSocketType)
    {
        owner = owningPiece;
        socketType = targetSocketType;
        IsOccupied = false;
    }

    public bool CanAttach(BuildPieceType pieceType)
    {
        if (IsOccupied)
        {
            return false;
        }

        switch (socketType)
        {
            case BuildSocketType.Foundation:
                return pieceType == BuildPieceType.Foundation;
            case BuildSocketType.Wall:
                return pieceType == BuildPieceType.Wall || pieceType == BuildPieceType.Doorway;
            default:
                return false;
        }
    }

    public void SetOccupied(BuildPiece connectedPiece)
    {
        IsOccupied = connectedPiece != null;
    }
}
