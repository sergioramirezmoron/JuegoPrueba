using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResourceNode : MonoBehaviour, IInteractable
{
    public ResourceType resourceType = ResourceType.Wood;
    public int amount = 2;

    void Awake()
    {
        ApplyVisuals();
    }

    void OnValidate()
    {
        amount = Mathf.Max(1, amount);
        ApplyVisuals();
    }

    public string GetInteractionPrompt()
    {
        return $"Recoger {GetDisplayName()}";
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return interactor != null && interactor.Inventory != null;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (interactor.Inventory.TryAddResource(resourceType, amount))
        {
            Destroy(gameObject);
        }
    }

    public void Configure(ResourceType targetResourceType, int targetAmount)
    {
        resourceType = targetResourceType;
        amount = Mathf.Max(1, targetAmount);
        ApplyVisuals();
    }

    private string GetDisplayName()
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                return "madera";
            case ResourceType.Scrap:
                return "chatarra";
            case ResourceType.Food:
                return "comida";
            default:
                return resourceType.ToString();
        }
    }

    private void ApplyVisuals()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Color color = resourceType switch
        {
            ResourceType.Wood => new Color(0.45f, 0.27f, 0.12f),
            ResourceType.Scrap => new Color(0.63f, 0.67f, 0.72f),
            ResourceType.Food => new Color(0.21f, 0.72f, 0.24f),
            _ => Color.white
        };

        renderer.material.color = color;
    }
}
