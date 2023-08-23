namespace RRU;

/// <summary>
/// A resource that describes the amount of materials a structure 'harvests' and what.
/// </summary>
[GlobalClass]
public sealed partial class StructureHarvestInfo : Resource, IResourceModifier
{
    public static StringName KeyStructureId => "sId";

    [Export] public ResourceType HarvestType;
    [Export] public float HarvestRate;
    [Export] public float HarvestAmount;

    double elapsedTime;

    /// Resource Modifier ///

    public bool ModifierIsActive(GameState context, double delta)
    {
        elapsedTime += delta;

        if (elapsedTime < HarvestRate)
            return false;

        elapsedTime = 0.0f;
        return true;
    }

    public void ModifierGet(GameState context, ref ResourceModifier modifier)
    {
        int count = context.GetStructureCount(
            id: (StringName) GetMeta(KeyStructureId)
        );

        modifier = new(
            resource: HarvestType,
            type: ResourceModifierType.Additive,
            amount: HarvestAmount * count
        );
    }
}