using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    private const float FoundationHeight = 0.125f;
    private const float FoundationSocketSnapRadius = 0.75f;

    public float buildDistance = 6f;
    public float foundationGridSize = 2f;
    public float socketFocusThreshold = 0.55f;

    private Camera playerCamera;
    private PlayerInventory inventory;
    private Transform buildRoot;
    private BuildPieceType selectedPieceType = BuildPieceType.Foundation;
    private BuildPiece previewPiece;
    private BuildPieceType previewPieceType;
    private BuildSocket currentSocket;
    private Pose currentPlacementPose;
    private bool canPlaceCurrentPiece;
    private bool buildModeEnabled;
    private int foundationRotationSteps;

    public void Configure(Camera targetCamera, PlayerInventory playerInventory, Transform targetBuildRoot)
    {
        playerCamera = targetCamera;
        inventory = playerInventory;
        buildRoot = targetBuildRoot;
    }

    public string GetHudDisplay()
    {
        if (!buildModeEnabled)
        {
            return "Construcción: B abrir";
        }

        string affordText = inventory != null && BuildingCatalog.CanAfford(inventory, selectedPieceType) ? "Listo" : "Faltan materiales";
        string pieceName = BuildingCatalog.GetDefinition(selectedPieceType).DisplayName;

        return "Modo construcción\n" +
               $"Pieza: {pieceName}\n" +
               $"Coste: {BuildingCatalog.GetCostLabel(selectedPieceType)}\n" +
               $"Estado: {affordText}\n" +
               "B salir | 1 Foundation | 2 Wall | 3 Doorway | R rotar | Click izq colocar";
    }

    void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        HandleBuildModeInput();

        if (!buildModeEnabled || playerCamera == null)
        {
            HidePreview();
            return;
        }

        UpdatePreview();

        if (canPlaceCurrentPiece && Input.GetMouseButtonDown(0))
        {
            PlaceCurrentPiece();
        }
    }

    public static Vector3 SnapToFoundationGrid(Vector3 worldPosition, float gridSize, float foundationCenterHeight)
    {
        float snappedX = Mathf.Round(worldPosition.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(worldPosition.z / gridSize) * gridSize;
        return new Vector3(snappedX, foundationCenterHeight, snappedZ);
    }

    private void HandleBuildModeInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildModeEnabled = !buildModeEnabled;
        }

        if (!buildModeEnabled)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedPieceType = BuildPieceType.Foundation;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedPieceType = BuildPieceType.Wall;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedPieceType = BuildPieceType.Doorway;
        }

        if (Input.GetKeyDown(KeyCode.R) && selectedPieceType == BuildPieceType.Foundation)
        {
            foundationRotationSteps = (foundationRotationSteps + 1) % 4;
        }
    }

    private void UpdatePreview()
    {
        EnsurePreviewPiece();

        if (!TryGetPlacementPose(selectedPieceType, out Pose placementPose, out BuildSocket targetSocket))
        {
            HidePreview();
            canPlaceCurrentPiece = false;
            return;
        }

        currentSocket = targetSocket;
        currentPlacementPose = placementPose;
        canPlaceCurrentPiece = ValidatePlacement(placementPose, targetSocket);

        previewPiece.gameObject.SetActive(true);
        previewPiece.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        previewPiece.SetPreviewState(canPlaceCurrentPiece);
    }

    private bool TryGetPlacementPose(BuildPieceType pieceType, out Pose placementPose, out BuildSocket targetSocket)
    {
        targetSocket = null;

        if (pieceType == BuildPieceType.Foundation)
        {
            if (!TryGetInitialFoundationPose(out placementPose, out Vector3 groundHitPoint))
            {
                return false;
            }

            if (TryFindFoundationSocketNearPoint(groundHitPoint, out targetSocket))
            {
                placementPose = new Pose(targetSocket.transform.position, targetSocket.transform.rotation);
            }

            return true;
        }
        else if (TryFindBestSocket(pieceType, out targetSocket))
        {
            placementPose = new Pose(targetSocket.transform.position, targetSocket.transform.rotation);
            return true;
        }

        placementPose = default;
        return false;
    }

    private bool TryGetInitialFoundationPose(out Pose placementPose, out Vector3 groundHitPoint)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float enter) || enter > buildDistance)
        {
            placementPose = default;
            groundHitPoint = default;
            return false;
        }

        groundHitPoint = ray.GetPoint(enter);
        Quaternion rotation = Quaternion.Euler(0f, foundationRotationSteps * 90f, 0f);

        placementPose = new Pose(new Vector3(groundHitPoint.x, FoundationHeight, groundHitPoint.z), rotation);
        return true;
    }

    private bool TryFindFoundationSocketNearPoint(Vector3 groundHitPoint, out BuildSocket bestSocket)
    {
        bestSocket = null;
        float bestDistance = FoundationSocketSnapRadius;

        BuildSocket[] sockets = FindObjectsByType<BuildSocket>(FindObjectsSortMode.None);
        foreach (BuildSocket socket in sockets)
        {
            if (socket == null || !socket.CanAttach(BuildPieceType.Foundation))
            {
                continue;
            }

            Vector2 groundPoint = new Vector2(groundHitPoint.x, groundHitPoint.z);
            Vector2 socketPoint = new Vector2(socket.transform.position.x, socket.transform.position.z);
            float planarDistance = Vector2.Distance(groundPoint, socketPoint);

            if (planarDistance > bestDistance)
            {
                continue;
            }

            bestDistance = planarDistance;
            bestSocket = socket;
        }

        return bestSocket != null;
    }

    private bool TryFindBestSocket(BuildPieceType pieceType, out BuildSocket bestSocket)
    {
        bestSocket = null;
        float bestScore = float.MinValue;
        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 cameraForward = playerCamera.transform.forward;

        BuildSocket[] sockets = FindObjectsByType<BuildSocket>(FindObjectsSortMode.None);
        foreach (BuildSocket socket in sockets)
        {
            if (socket == null || !socket.CanAttach(pieceType))
            {
                continue;
            }

            float distance = Vector3.Distance(cameraPosition, socket.transform.position);
            if (distance > buildDistance)
            {
                continue;
            }

            Vector3 direction = (socket.transform.position - cameraPosition).normalized;
            float focusDot = Vector3.Dot(cameraForward, direction);
            if (focusDot < socketFocusThreshold)
            {
                continue;
            }

            float score = focusDot * 100f - distance * 10f;
            if (score > bestScore)
            {
                bestScore = score;
                bestSocket = socket;
            }
        }

        return bestSocket != null;
    }

    private bool ValidatePlacement(Pose placementPose, BuildSocket targetSocket)
    {
        if (targetSocket != null)
        {
            return targetSocket.CanAttach(selectedPieceType);
        }

        if (selectedPieceType != BuildPieceType.Foundation)
        {
            return false;
        }

        BuildPiece[] pieces = FindObjectsByType<BuildPiece>(FindObjectsSortMode.None);
        foreach (BuildPiece piece in pieces)
        {
            if (piece == null || piece == previewPiece || piece.IsPreview || piece.pieceType != BuildPieceType.Foundation)
            {
                continue;
            }

            if (Vector3.Distance(piece.transform.position, placementPose.position) < BuildPiece.FoundationSize * 0.9f)
            {
                return false;
            }
        }

        return true;
    }

    private void PlaceCurrentPiece()
    {
        if (inventory == null || !BuildingCatalog.TryConsumePlacementCost(inventory, selectedPieceType))
        {
            return;
        }

        BuildPiece placedPiece = BuildingPieceFactory.CreatePiece(selectedPieceType, false, buildRoot);
        placedPiece.transform.SetPositionAndRotation(currentPlacementPose.position, currentPlacementPose.rotation);

        if (currentSocket != null)
        {
            currentSocket.SetOccupied(placedPiece);
            MarkReverseFoundationConnection(placedPiece, currentSocket);
        }
    }

    private void MarkReverseFoundationConnection(BuildPiece placedPiece, BuildSocket sourceSocket)
    {
        if (placedPiece == null || sourceSocket == null || placedPiece.pieceType != BuildPieceType.Foundation)
        {
            return;
        }

        BuildSocket reverseSocket = placedPiece.FindSocketNear(sourceSocket.Owner.transform.position, 0.25f);
        if (reverseSocket != null)
        {
            reverseSocket.SetOccupied(sourceSocket.Owner);
        }
    }

    private void EnsurePreviewPiece()
    {
        if (previewPiece != null && previewPieceType == selectedPieceType)
        {
            return;
        }

        if (previewPiece != null)
        {
            Destroy(previewPiece.gameObject);
        }

        previewPieceType = selectedPieceType;
        previewPiece = BuildingPieceFactory.CreatePiece(selectedPieceType, true, buildRoot);
        previewPiece.gameObject.SetActive(false);
    }

    private void HidePreview()
    {
        if (previewPiece != null)
        {
            previewPiece.gameObject.SetActive(false);
        }
    }
}
