[System.Serializable]
public class BuildCostEntry
{
    public BuildCostEntry(ResourceType resourceType, int amount)
    {
        ResourceType = resourceType;
        Amount = amount;
    }

    public ResourceType ResourceType { get; }

    public int Amount { get; }
}
