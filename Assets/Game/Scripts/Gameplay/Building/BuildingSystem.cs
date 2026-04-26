using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    private const float FoundationHeight = 0.125f;
    private const float FoundationSocketSnapRadius = 0.75f;
    private const float PlacementOverlapInset = 0.01f;

    public float buildDistance = 6f;
    public float foundationGridSize = 2f;
    public float socketFocusThreshold = 0.55f;
    public float foundationRotationSpeed = 120f;
    public float foundationWheelStep = 15f;

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
    private bool infiniteBuildEnabled;
    private float foundationYaw;

    public bool InfiniteBuildEnabled => infiniteBuildEnabled;

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

        string affordText = infiniteBuildEnabled || (inventory != null && BuildingCatalog.CanAfford(inventory, selectedPieceType)) ? "Listo" : "Faltan materiales";
        string pieceName = BuildingCatalog.GetDefinition(selectedPieceType).DisplayName;
        string costLabel = infiniteBuildEnabled ? "Infinito" : BuildingCatalog.GetCostLabel(selectedPieceType);
        string modeLabel = infiniteBuildEnabled ? "ON" : "OFF";

        return "Modo construcción\n" +
               $"Pieza: {pieceName}\n" +
               $"Coste: {costLabel}\n" +
               $"Estado: {affordText}\n" +
               $"ConstrucciÃ³n infinita: {modeLabel}\n" +
               "B salir | F6 infinito | 1 Foundation | 2 Wall | 3 Doorway | Q/E o rueda rotar | Click izq colocar";
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

    public static Quaternion CreateFoundationRotation(float yawDegrees)
    {
        return Quaternion.Euler(0f, NormalizeFoundationYaw(yawDegrees), 0f);
    }

    public void ToggleInfiniteBuild()
    {
        infiniteBuildEnabled = !infiniteBuildEnabled;
    }

    private void HandleBuildModeInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildModeEnabled = !buildModeEnabled;
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            ToggleInfiniteBuild();
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

        if (selectedPieceType == BuildPieceType.Foundation)
        {
            float yawDelta = 0f;

            if (Input.GetKey(KeyCode.Q))
            {
                yawDelta -= foundationRotationSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.E))
            {
                yawDelta += foundationRotationSpeed * Time.deltaTime;
            }

            float mouseWheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(mouseWheel) > Mathf.Epsilon)
            {
                yawDelta += mouseWheel * foundationWheelStep;
            }

            if (Mathf.Abs(yawDelta) > Mathf.Epsilon)
            {
                foundationYaw = NormalizeFoundationYaw(foundationYaw + yawDelta);
            }
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
        Quaternion rotation = CreateFoundationRotation(foundationYaw);

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
            return targetSocket.CanAttach(selectedPieceType) && !HasStructureOverlap(placementPose);
        }

        if (selectedPieceType != BuildPieceType.Foundation)
        {
            return false;
        }

        return !HasStructureOverlap(placementPose);
    }

    private void PlaceCurrentPiece()
    {
        if (!infiniteBuildEnabled && (inventory == null || !BuildingCatalog.TryConsumePlacementCost(inventory, selectedPieceType)))
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

    private bool HasStructureOverlap(Pose placementPose)
    {
        if (previewPiece == null)
        {
            return false;
        }

        foreach (BoxCollider collisionBox in previewPiece.CollisionBoxes)
        {
            if (collisionBox == null)
            {
                continue;
            }

            Vector3 halfExtents = GetPlacementHalfExtents(collisionBox);
            Vector3 worldCenter = GetPlacementWorldCenter(placementPose, collisionBox);
            Quaternion worldRotation = placementPose.rotation * collisionBox.transform.localRotation;
            Collider[] overlaps = Physics.OverlapBox(worldCenter, halfExtents, worldRotation, ~0, QueryTriggerInteraction.Ignore);

            foreach (Collider overlap in overlaps)
            {
                if (overlap == null)
                {
                    continue;
                }

                BuildPiece overlapPiece = overlap.GetComponentInParent<BuildPiece>();
                if (overlapPiece == null || overlapPiece == previewPiece || overlapPiece.IsPreview)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    private static Vector3 GetPlacementWorldCenter(Pose placementPose, BoxCollider collisionBox)
    {
        Vector3 scaledCenter = Vector3.Scale(collisionBox.center, collisionBox.transform.localScale);
        Vector3 localCenter = collisionBox.transform.localPosition + scaledCenter;
        return placementPose.position + (placementPose.rotation * localCenter);
    }

    private static Vector3 GetPlacementHalfExtents(BoxCollider collisionBox)
    {
        Vector3 scaledSize = Vector3.Scale(collisionBox.size, collisionBox.transform.localScale);
        Vector3 halfExtents = scaledSize * 0.5f;

        return new Vector3(
            Mathf.Max(0.01f, halfExtents.x - PlacementOverlapInset),
            Mathf.Max(0.01f, halfExtents.y - PlacementOverlapInset),
            Mathf.Max(0.01f, halfExtents.z - PlacementOverlapInset));
    }

    private static float NormalizeFoundationYaw(float yawDegrees)
    {
        return Mathf.Repeat(yawDegrees, 360f);
    }
}
