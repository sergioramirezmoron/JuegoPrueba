using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactionDistance = 2.5f;
    public float interactionRadius = 0.5f;
    public float focusDotThreshold = 0.35f;

    private Camera playerCamera;
    private TextMeshProUGUI promptText;
    private PlayerInventory inventory;
    private PlayerVitals vitals;

    public PlayerInventory Inventory => inventory;
    public PlayerVitals Vitals => vitals;

    public void Configure(Camera targetCamera, TextMeshProUGUI interactionPrompt, PlayerInventory playerInventory, PlayerVitals playerVitals)
    {
        playerCamera = targetCamera;
        promptText = interactionPrompt;
        inventory = playerInventory;
        vitals = playerVitals;

        ClearPrompt();
    }

    void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            ClearPrompt();
            return;
        }

        IInteractable interactable = FindInteractable();
        if (interactable == null || !interactable.CanInteract(this))
        {
            ClearPrompt();
            return;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = $"[E] {interactable.GetInteractionPrompt()}";
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            interactable.Interact(this);
        }
    }

    private IInteractable FindInteractable()
    {
        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 cameraForward = playerCamera.transform.forward;

        IInteractable bestInteractable = null;
        float bestScore = float.MinValue;
        HashSet<MonoBehaviour> evaluatedComponents = new HashSet<MonoBehaviour>();

        RaycastHit[] sphereHits = Physics.SphereCastAll(
            cameraPosition,
            interactionRadius,
            cameraForward,
            interactionDistance);

        foreach (RaycastHit hit in sphereHits)
        {
            TryRegisterInteractable(hit.collider, cameraPosition, cameraForward, evaluatedComponents, ref bestInteractable, ref bestScore);
        }

        Vector3 overlapCenter = cameraPosition + cameraForward * Mathf.Min(1.1f, interactionDistance * 0.65f);
        Collider[] nearbyColliders = Physics.OverlapSphere(overlapCenter, interactionRadius * 1.6f);

        foreach (Collider collider in nearbyColliders)
        {
            TryRegisterInteractable(collider, cameraPosition, cameraForward, evaluatedComponents, ref bestInteractable, ref bestScore);
        }

        return bestInteractable;
    }

    private void TryRegisterInteractable(
        Collider collider,
        Vector3 cameraPosition,
        Vector3 cameraForward,
        HashSet<MonoBehaviour> evaluatedComponents,
        ref IInteractable bestInteractable,
        ref float bestScore)
    {
        MonoBehaviour[] components = collider.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component == null || !evaluatedComponents.Add(component) || component is not IInteractable interactable)
            {
                continue;
            }

            Vector3 candidatePoint = collider.ClosestPoint(cameraPosition);
            if (!TryCalculateCandidateScore(cameraPosition, cameraForward, candidatePoint, interactionDistance, focusDotThreshold, out float score))
            {
                continue;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestInteractable = interactable;
            }
        }
    }

    public static bool TryCalculateCandidateScore(
        Vector3 origin,
        Vector3 forward,
        Vector3 candidatePoint,
        float maxDistance,
        float minFocusDot,
        out float score)
    {
        Vector3 direction = candidatePoint - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            direction = forward;
            distance = 0f;
        }
        else
        {
            direction /= distance;
        }

        if (distance > maxDistance + 0.75f)
        {
            score = float.MinValue;
            return false;
        }

        float focusDot = Vector3.Dot(forward, direction);
        if (focusDot < minFocusDot)
        {
            score = float.MinValue;
            return false;
        }

        float distanceScore = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, maxDistance + 0.75f));
        score = focusDot * 100f + distanceScore * 25f;
        return true;
    }

    private void ClearPrompt()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.text = string.Empty;
        promptText.gameObject.SetActive(false);
    }
}
