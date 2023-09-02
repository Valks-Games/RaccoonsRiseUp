namespace RRU;

/// <summary>
/// This resource represents an upgrade option in the tech page
/// </summary>
[GlobalClass]
public sealed partial class TechUpgradeInfo : Resource
{
    [Export] public StringName Id;
    [Export] public string[] RequiredUpgradeIds;
    [Export] public string DisplayName;

    [Export] public Texture2D Icon;
    [Export] public string DisplayDescription;

    [Export] public Vector2I Position;

    [Export] public ResourceRequirement[] UpgradeCost;
    [Export] public ResourceModifierDefinition[] Modifiers;
}
