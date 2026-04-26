using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    private const string BuildModeHint = "ConstrucciÃ³n: B abrir";

    private PlayerVitals vitals;
    private PlayerInventory inventory;
    private BuildingSystem buildingSystem;
    private TextMeshProUGUI promptTemplate;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI inventoryText;
    private TextMeshProUGUI buildText;

    public void Configure(PlayerVitals playerVitals, PlayerInventory playerInventory, BuildingSystem playerBuildingSystem, TextMeshProUGUI interactionPromptTemplate)
    {
        vitals = playerVitals;
        inventory = playerInventory;
        buildingSystem = playerBuildingSystem;
        promptTemplate = interactionPromptTemplate;

        if (promptTemplate == null)
        {
            return;
        }

        statusText = statusText != null ? statusText : CreateTextClone("StatusText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), TextAlignmentOptions.TopLeft, 28f);
        inventoryText = inventoryText != null ? inventoryText : CreateTextClone("InventoryText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), TextAlignmentOptions.TopRight, 24f);
        buildText = buildText != null ? buildText : CreateTextClone("BuildText", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), TextAlignmentOptions.Bottom, 22f);
        buildText.rectTransform.pivot = new Vector2(0.5f, 0f);
        buildText.rectTransform.sizeDelta = new Vector2(760f, 180f);
    }

    void LateUpdate()
    {
        if (vitals != null && statusText != null)
        {
            statusText.text = vitals.GetStatusDisplay();
        }

        if (inventory != null && inventoryText != null)
        {
            inventoryText.text = inventory.GetInventoryDisplay();
        }

        if (buildText != null)
        {
            buildText.text = buildingSystem != null ? buildingSystem.GetHudDisplay() : BuildModeHint;
        }
    }

    private TextMeshProUGUI CreateTextClone(string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, TextAlignmentOptions alignment, float fontSize)
    {
        TextMeshProUGUI clone = Instantiate(promptTemplate, promptTemplate.transform.parent);
        clone.name = objectName;
        clone.gameObject.SetActive(true);
        clone.text = string.Empty;
        clone.alignment = alignment;
        clone.fontSize = fontSize;

        RectTransform rectTransform = clone.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(anchorMax.x, anchorMin.y == 1f ? 1f : 0f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(420f, 160f);

        return clone;
    }
}
