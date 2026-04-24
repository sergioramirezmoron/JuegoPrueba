using UnityEngine;

public static class BuildingPieceFactory
{
    public static BuildPiece CreatePiece(BuildPieceType pieceType, bool isPreview, Transform parent = null)
    {
        GameObject root = new GameObject(isPreview ? $"{pieceType}Preview" : pieceType.ToString());
        if (parent != null)
        {
            root.transform.SetParent(parent, false);
        }

        BuildPiece buildPiece = root.AddComponent<BuildPiece>();
        buildPiece.Initialize(pieceType, isPreview);

        return buildPiece;
    }
}
