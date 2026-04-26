using System.Collections.Generic;
using UnityEngine;

public class BuildPiece : MonoBehaviour
{
    public const float FoundationSize = 2f;
    public const float FoundationThickness = 0.25f;
    public const float WallWidth = 2f;
    public const float WallHeight = 2.3f;
    public const float WallDepth = 0.18f;
    public const float DoorPostWidth = 0.15f;
    public const float DoorBeamThickness = 0.2f;
    public const float DoorOpeningHeight = 2.1f;
    public const float DoorOpeningWidth = WallWidth - (DoorPostWidth * 2f);

    private readonly List<Renderer> renderers = new List<Renderer>();
    private readonly List<BoxCollider> collisionBoxes = new List<BoxCollider>();
    private readonly List<BuildSocket> sockets = new List<BuildSocket>();
    private bool isPreview;

    public BuildPieceType pieceType;

    public IReadOnlyList<BoxCollider> CollisionBoxes => collisionBoxes;
    public IReadOnlyList<BuildSocket> Sockets => sockets;
    public bool IsPreview => isPreview;

    public void Initialize(BuildPieceType targetPieceType, bool isPreview)
    {
        pieceType = targetPieceType;
        this.isPreview = isPreview;
        BuildGeometry(isPreview);
    }

    public void SetPreviewState(bool isValid)
    {
        Color tint = isValid ? new Color(0.28f, 0.9f, 0.36f) : new Color(0.95f, 0.22f, 0.22f);

        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = tint;
        }
    }

    public BuildSocket FindSocketNear(Vector3 worldPosition, float maxDistance)
    {
        BuildSocket closestSocket = null;
        float closestDistance = maxDistance;

        foreach (BuildSocket socket in sockets)
        {
            float distance = Vector3.Distance(socket.transform.position, worldPosition);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestSocket = socket;
            }
        }

        return closestSocket;
    }

    private void BuildGeometry(bool isPreview)
    {
        switch (pieceType)
        {
            case BuildPieceType.Foundation:
                BuildFoundation(isPreview);
                break;
            case BuildPieceType.Wall:
                BuildWall(isPreview);
                break;
            case BuildPieceType.Doorway:
                BuildDoorway(isPreview);
                break;
        }
    }

    private void BuildFoundation(bool isPreview)
    {
        float halfFoundationSize = FoundationSize * 0.5f;
        float wallSocketHeight = (FoundationThickness * 0.5f) + (WallHeight * 0.5f);

        CreateBox("FoundationBlock", Vector3.zero, new Vector3(FoundationSize, FoundationThickness, FoundationSize), new Color(0.44f, 0.32f, 0.2f), isPreview);

        CreateSocket("FoundationNorth", new Vector3(0f, 0f, FoundationSize), Quaternion.identity, BuildSocketType.Foundation);
        CreateSocket("FoundationSouth", new Vector3(0f, 0f, -FoundationSize), Quaternion.identity, BuildSocketType.Foundation);
        CreateSocket("FoundationEast", new Vector3(FoundationSize, 0f, 0f), Quaternion.identity, BuildSocketType.Foundation);
        CreateSocket("FoundationWest", new Vector3(-FoundationSize, 0f, 0f), Quaternion.identity, BuildSocketType.Foundation);

        CreateSocket("WallNorth", new Vector3(0f, wallSocketHeight, halfFoundationSize), Quaternion.identity, BuildSocketType.Wall);
        CreateSocket("WallSouth", new Vector3(0f, wallSocketHeight, -halfFoundationSize), Quaternion.Euler(0f, 180f, 0f), BuildSocketType.Wall);
        CreateSocket("WallEast", new Vector3(halfFoundationSize, wallSocketHeight, 0f), Quaternion.Euler(0f, 90f, 0f), BuildSocketType.Wall);
        CreateSocket("WallWest", new Vector3(-halfFoundationSize, wallSocketHeight, 0f), Quaternion.Euler(0f, -90f, 0f), BuildSocketType.Wall);
    }

    private void BuildWall(bool isPreview)
    {
        CreateBox("WallPanel", Vector3.zero, new Vector3(WallWidth, WallHeight, WallDepth), new Color(0.72f, 0.56f, 0.32f), isPreview);
    }

    private void BuildDoorway(bool isPreview)
    {
        float postCenterX = (WallWidth * 0.5f) - (DoorPostWidth * 0.5f);
        float beamCenterY = (-WallHeight * 0.5f) + DoorOpeningHeight + (DoorBeamThickness * 0.5f);

        CreateBox("DoorLeftPost", new Vector3(-postCenterX, 0f, 0f), new Vector3(DoorPostWidth, WallHeight, WallDepth), new Color(0.64f, 0.48f, 0.28f), isPreview);
        CreateBox("DoorRightPost", new Vector3(postCenterX, 0f, 0f), new Vector3(DoorPostWidth, WallHeight, WallDepth), new Color(0.64f, 0.48f, 0.28f), isPreview);
        CreateBox("DoorTop", new Vector3(0f, beamCenterY, 0f), new Vector3(WallWidth, DoorBeamThickness, WallDepth), new Color(0.64f, 0.48f, 0.28f), isPreview);
    }

    private void CreateBox(string objectName, Vector3 localPosition, Vector3 localScale, Color tint, bool isPreview)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = objectName;
        box.transform.SetParent(transform, false);
        box.transform.localPosition = localPosition;
        box.transform.localScale = localScale;

        Renderer renderer = box.GetComponent<Renderer>();
        renderer.material.color = tint;
        renderers.Add(renderer);

        BoxCollider boxCollider = box.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            collisionBoxes.Add(boxCollider);
        }

        if (isPreview)
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }
    }

    private void CreateSocket(string objectName, Vector3 localPosition, Quaternion localRotation, BuildSocketType socketType)
    {
        GameObject socketObject = new GameObject(objectName);
        socketObject.transform.SetParent(transform, false);
        socketObject.transform.localPosition = localPosition;
        socketObject.transform.localRotation = localRotation;

        BuildSocket socket = socketObject.AddComponent<BuildSocket>();
        socket.Initialize(this, socketType);
        sockets.Add(socket);
    }
}
