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
        fieldPrerequisiteId.Text = identifier;

        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        fieldPrerequisiteId.Select(FindTheAssociatedIndexForThisIdentifierInTheSourceDiscoverabilityArray(identifier, upgrades));
    }

    public override void UpdateSource()
    {
        Span<TechUpgradeInfo> upgrades = default;
        discoverability.GetTechUpgradeIds(ref upgrades);

        listView.EditorPerformWrite(GetIndex(), upgrades[fieldPrerequisiteId.Selected].Id);
    }

    int FindTheAssociatedIndexForThisIdentifierInTheSourceDiscoverabilityArray(
        StringName identifierToFindMatchesFor,
        Span<TechUpgradeInfo> sourceUpgradesArray)
    {
        for (int iterationStep = 0; iterationStep < sourceUpgradesArray.Length; ++iterationStep)
        {
            if (sourceUpgradesArray[iterationStep].Id != identifierToFindMatchesFor)
                continue;

            return iterationStep;
        }

        return 0;
    }

    /// Events ///

    void OnPrerequisiteChanged(long index)
    {
        UpdateSource();
    }
}
