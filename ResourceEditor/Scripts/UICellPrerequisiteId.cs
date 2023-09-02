namespace RRU.ResourceEditor;

public sealed partial class UICellPrerequisiteId : BaseEditParamsCell
{
    [Export] EditorTechUpgradeDiscoverability discoverability;

    OptionButton fieldPrerequisiteId;

    public override void Initialise()
    {
        // Get
        fieldPrerequisiteId = GetNode<OptionButton>("%Id");
        fieldPrerequisiteId.Clear();

        // Bind
        fieldPrerequisiteId.ItemSelected += OnPrerequisiteChanged;
    }

    public void Sync(StringName ownerId, StringName identifier)
    {
        fieldPrerequisiteId.Text = identifier;

        // Populate ID selection
        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        for (int i = 0; i < upgrades.Length; ++i)
        {
            if (upgrades[i].Id == ownerId)
                continue;

            fieldPrerequisiteId.AddItem(upgrades[i].Id);

            if (upgrades[i].Id != identifier)
                continue;

            fieldPrerequisiteId.Select(i);
        }
    }

    public override void UpdateSource()
    {
        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        listView.EditorPerformWrite(GetIndex(), upgrades[fieldPrerequisiteId.Selected].Id);
    }

    /// Events ///

    void OnPrerequisiteChanged(long index)
    {
        UpdateSource();
    }
}
