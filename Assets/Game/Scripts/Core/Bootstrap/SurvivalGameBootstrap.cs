using TMPro;
using UnityEngine;

public static class SurvivalGameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        PlayerMovement playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        if (playerMovement == null)
        {
            return;
        }

        GameObject playerObject = playerMovement.gameObject;
        PlayerInventory inventory = GetOrAddComponent<PlayerInventory>(playerObject);
        PlayerVitals vitals = GetOrAddComponent<PlayerVitals>(playerObject);
        PlayerInteractor interactor = GetOrAddComponent<PlayerInteractor>(playerObject);
        BuildingSystem buildingSystem = GetOrAddComponent<BuildingSystem>(playerObject);

        EnsureMinimumResource(inventory, ResourceType.Food, 2);
        EnsureMinimumResource(inventory, ResourceType.Wood, 16);
        EnsureMinimumResource(inventory, ResourceType.Scrap, 8);

        vitals.BindInventory(inventory);

        TextMeshProUGUI promptText = ResolveInteractionPrompt(playerMovement);
        interactor.Configure(Camera.main, promptText, inventory, vitals);

        GameObject systemsRoot = GameObject.Find("GameplaySystems");
        if (systemsRoot == null)
        {
            systemsRoot = new GameObject("GameplaySystems");
        }

        ResourceSpawner resourceSpawner = GetOrAddComponent<ResourceSpawner>(systemsRoot);
        resourceSpawner.EnsurePrototypeResources();

        GameObject buildRoot = GameObject.Find("PlayerBase");
        if (buildRoot == null)
        {
            buildRoot = new GameObject("PlayerBase");
            buildRoot.transform.SetParent(systemsRoot.transform, false);
        }

        buildingSystem.Configure(Camera.main, inventory, buildRoot.transform);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            PlayerHud hud = GetOrAddComponent<PlayerHud>(canvas.gameObject);
            hud.Configure(vitals, inventory, buildingSystem, promptText);
        }
    }

    private static TextMeshProUGUI ResolveInteractionPrompt(PlayerMovement playerMovement)
    {
        if (playerMovement.interactText != null)
        {
            TextMeshProUGUI existingPrompt = playerMovement.interactText.GetComponent<TextMeshProUGUI>();
            if (existingPrompt != null)
            {
                return existingPrompt;
            }
        }

        TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name == "Text (TMP)")
            {
                playerMovement.interactText = text.gameObject;
                return text;
            }
        }

        return null;
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static void EnsureMinimumResource(PlayerInventory inventory, ResourceType resourceType, int minimumAmount)
    {
        int currentAmount = inventory.GetResourceCount(resourceType);
        if (currentAmount >= minimumAmount)
        {
            return;
        }

        inventory.TryAddResource(resourceType, minimumAmount - currentAmount);
    }
}
