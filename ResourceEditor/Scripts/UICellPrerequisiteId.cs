namespace RRU.ResourceEditor;

public sealed partial class UICellPrerequisiteId : BaseEditParamsCell
{
    [Export] EditorTechUpgradeDiscoverability discoverability;

    OptionButton fieldPrerequisiteId;
    int lastIdx;

    public override void Initialise()
    {
        // Get
        fieldPrerequisiteId = GetNode<OptionButton>("%Id");
        fieldPrerequisiteId.Clear();

        // Init
        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        for (int i = 0; i < upgrades.Length; ++i)
        {
            fieldPrerequisiteId.AddItem(upgrades[i].Id);
        }

        // Bind
        fieldPrerequisiteId.ItemSelected += OnPrerequisiteChanged;
    }

    public void Sync(string identifier)
    {
        int idx = IndexForText(identifier);

        fieldPrerequisiteId.Select(idx);
        lastIdx = idx;
    }

    public override void UpdateSource()
    {
        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        listView.EditorPerformWrite(GetIndex(), upgrades[lastIdx]);
    }

    int IndexForText(string value)
    {
        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        for (int i = 0; i < upgrades.Length; ++i)
        {
            if (upgrades[i].Id != value)
                continue;

            return i;
        }

        return 0;
    }

    /// Events ///

    void OnPrerequisiteChanged(long index)
    {
        UpdateSource();
    }
}
