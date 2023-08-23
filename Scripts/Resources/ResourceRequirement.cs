namespace RRU;

[GlobalClass]
public sealed partial class ResourceRequirement : Resource
{
    [Export] public ResourceType Type;
    [Export] public int Amount;

    /// <summary>
    /// How much of the penalty should be used when calculating its updated cost.
    /// </summary>
    [Export] public double PenaltyModifier = 1.0;
}
