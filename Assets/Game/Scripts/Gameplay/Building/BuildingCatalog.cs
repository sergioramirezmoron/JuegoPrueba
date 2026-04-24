using System.Text;

public static class BuildingCatalog
{
    private static readonly BuildPieceDefinition FoundationDefinition = new BuildPieceDefinition(
        BuildPieceType.Foundation,
        "Foundation",
        new[]
        {
            new BuildCostEntry(ResourceType.Wood, 4),
            new BuildCostEntry(ResourceType.Scrap, 2)
        });

    private static readonly BuildPieceDefinition WallDefinition = new BuildPieceDefinition(
        BuildPieceType.Wall,
        "Wall",
        new[]
        {
            new BuildCostEntry(ResourceType.Wood, 3)
        });

    private static readonly BuildPieceDefinition DoorwayDefinition = new BuildPieceDefinition(
        BuildPieceType.Doorway,
        "Doorway",
        new[]
        {
            new BuildCostEntry(ResourceType.Wood, 2),
            new BuildCostEntry(ResourceType.Scrap, 1)
        });

    public static BuildPieceDefinition GetDefinition(BuildPieceType pieceType)
    {
        switch (pieceType)
        {
            case BuildPieceType.Foundation:
                return FoundationDefinition;
            case BuildPieceType.Wall:
                return WallDefinition;
            case BuildPieceType.Doorway:
                return DoorwayDefinition;
            default:
                return FoundationDefinition;
        }
    }

    public static bool CanAfford(PlayerInventory inventory, BuildPieceType pieceType)
    {
        BuildPieceDefinition definition = GetDefinition(pieceType);

        foreach (BuildCostEntry cost in definition.Costs)
        {
            if (inventory.GetResourceCount(cost.ResourceType) < cost.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryConsumePlacementCost(PlayerInventory inventory, BuildPieceType pieceType)
    {
        if (!CanAfford(inventory, pieceType))
        {
            return false;
        }

        BuildPieceDefinition definition = GetDefinition(pieceType);
        foreach (BuildCostEntry cost in definition.Costs)
        {
            inventory.TryConsumeResource(cost.ResourceType, cost.Amount);
        }

        return true;
    }

    public static string GetCostLabel(BuildPieceType pieceType)
    {
        BuildPieceDefinition definition = GetDefinition(pieceType);
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < definition.Costs.Length; i++)
        {
            BuildCostEntry cost = definition.Costs[i];
            builder.Append(GetResourceDisplayName(cost.ResourceType));
            builder.Append(": ");
            builder.Append(cost.Amount);

            if (i < definition.Costs.Length - 1)
            {
                builder.Append(" | ");
            }
        }

        return builder.ToString();
    }

    public static string GetResourceDisplayName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                return "Madera";
            case ResourceType.Scrap:
                return "Chatarra";
            case ResourceType.Food:
                return "Comida";
            default:
                return resourceType.ToString();
        }
    }
}
