namespace RRU;

/// <summary>
/// A resource defining a structure
/// </summary>
[GlobalClass]
public sealed partial class StructureDataInfo : Resource
{
    [Export] public StringName Identifier;
    [Export] public string DisplayName;
    [Export] public Texture2D Icon;

    [Export] public ResourceRequirement[] Cost;
    [Export] public StructureHarvestInfo[] HarvestInfo;

    public StructureDataInfo()
    {
        // Wait for 'HarvestInfo' resources to be ready. And yes,
        // you cannot use the 'await process frame' method on a resource object.
        // In this realm, nobody can avoid using 'CallDeferred' forever.
        CallDeferred(MethodName.BindIdentifier);
    }

    void BindIdentifier()
    {
        // Store its identifier as metadata in its associated harvest info.
        // This will be used in its IResourceModifier's 'tick' process
        // to find the correct 'count' for each structure.
        for (int i = 0; i < HarvestInfo.Length; ++i)
        {
            HarvestInfo[i].SetMeta(
                name: StructureHarvestInfo.KeyStructureId,
                value: Identifier
            );
        }
    }
}