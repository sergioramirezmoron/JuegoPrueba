using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int slotCapacity = 12;
    public int maxStackSize = 99;

    private readonly Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    public int GetResourceCount(ResourceType resourceType)
    {
        return resources.TryGetValue(resourceType, out int amount) ? amount : 0;
    }

    public int GetOccupiedSlotCount()
    {
        int occupiedSlots = 0;

        foreach (KeyValuePair<ResourceType, int> entry in resources)
        {
            occupiedSlots += GetRequiredStacks(entry.Value);
        }

        return occupiedSlots;
    }

    public bool TryAddResource(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int currentAmount = GetResourceCount(resourceType);
        int currentStacks = GetRequiredStacks(currentAmount);
        int requiredStacks = GetRequiredStacks(currentAmount + amount);
        int additionalStacks = requiredStacks - currentStacks;

        if (GetOccupiedSlotCount() + additionalStacks > slotCapacity)
        {
            return false;
        }

        resources[resourceType] = currentAmount + amount;
        return true;
    }

    public bool TryConsumeResource(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int currentAmount = GetResourceCount(resourceType);
        if (currentAmount < amount)
        {
            return false;
        }

        int remainingAmount = currentAmount - amount;
        if (remainingAmount == 0)
        {
            resources.Remove(resourceType);
        }
        else
        {
            resources[resourceType] = remainingAmount;
        }

        return true;
    }

    public string GetInventoryDisplay()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Inventario");
        builder.AppendLine($"Huecos: {GetOccupiedSlotCount()}/{slotCapacity}");
        builder.AppendLine($"Madera: {GetResourceCount(ResourceType.Wood)}");
        builder.AppendLine($"Chatarra: {GetResourceCount(ResourceType.Scrap)}");
        builder.AppendLine($"Comida: {GetResourceCount(ResourceType.Food)}");
        builder.AppendLine();
        builder.Append("E: recoger o usar | F: comer | B: construir");
        return builder.ToString();
    }

    private int GetRequiredStacks(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        return Mathf.CeilToInt(amount / (float)Mathf.Max(1, maxStackSize));
    }
}
