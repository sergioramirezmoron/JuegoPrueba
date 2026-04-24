public class BuildPieceDefinition
{
    public BuildPieceDefinition(BuildPieceType pieceType, string displayName, BuildCostEntry[] costs)
    {
        PieceType = pieceType;
        DisplayName = displayName;
        Costs = costs;
    }

    public BuildPieceType PieceType { get; }

    public string DisplayName { get; }

    public BuildCostEntry[] Costs { get; }
}
