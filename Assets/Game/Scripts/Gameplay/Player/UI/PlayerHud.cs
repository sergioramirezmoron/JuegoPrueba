using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    private PlayerVitals vitals;
    private PlayerInventory inventory;
    private TextMeshProUGUI promptTemplate;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI inventoryText;

    public void Configure(PlayerVitals playerVitals, PlayerInventory playerInventory, TextMeshProUGUI interactionPromptTemplate)
    {
        vitals = playerVitals;
        inventory = playerInventory;
        promptTemplate = interactionPromptTemplate;

        if (promptTemplate == null)
        {
            return;
        }

        statusText = statusText != null ? statusText : CreateTextClone("StatusText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), TextAlignmentOptions.TopLeft, 28f);
        inventoryText = inventoryText != null ? inventoryText : CreateTextClone("InventoryText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), TextAlignmentOptions.TopRight, 24f);
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
