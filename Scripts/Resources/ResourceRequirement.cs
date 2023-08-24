namespace RRU;

[GlobalClass]
public sealed partial class ResourceRequirement : Resource
{
    [Export] public ResourceType Type;
    [Export] public int Amount;

    /// <summary>
    /// How much should this requirement scale up, if the player has more than one structure in their inventory?
    /// </summary>
    [Export] public double PenaltyModifier = 1.0;
}
