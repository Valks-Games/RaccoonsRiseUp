using System.Runtime.InteropServices;

namespace RRU.ResourceEditor;

public sealed partial class ResourceEditorMain : Control, IDataListViewDataProvider, IEditorContextWriter, IEditorTechUpgradeDiscoverability
{
    [Export] GameState gameState;
    [Export] TechDataService techDataService;
    [Export] StructureDataService structureDataService;

    [Export] EditorTechUpgradeDiscoverability upgradesDiscoverability;

    UIDataListView techUpgradesListView;
    UIDataListView structuresListView;
    UIDataListView jobsListView;
    UIDataInspector inspector;

    List<StructureDataInfo> mutableStructures;
    List<TechUpgradeInfo> mutableUpgrades;
    List<JobData> mutableJobs;

    public override void _Ready()
    {
        // Get
        inspector = GetNode<UIDataInspector>("%Inspector");
        techUpgradesListView = GetNode<UIDataListView>("%Tech Upgrades");
        structuresListView = GetNode<UIDataListView>("%Structures");
        jobsListView = GetNode<UIDataListView>("%Jobs");

        // Init
        techUpgradesListView.DataProvider = this;
        structuresListView.DataProvider = this;
        jobsListView.DataProvider = this;

        ReadOnlySpan<StructureDataInfo> structures = default;
        ReadOnlySpan<TechUpgradeInfo> upgrades = default;

        structureDataService.GetStructures(ref structures);
        techDataService.GetAllUpgrades(ref upgrades);

        mutableStructures = new();
        mutableUpgrades = new();
        mutableJobs = new();

        for (int i = 0; i < structures.Length; ++ i)
        {
            mutableStructures.Add(structures[i]);
        }

        for (int i = 0; i < upgrades.Length; ++ i)
        {
            mutableUpgrades.Add(upgrades[i]);
        }

        ReadOnlySpan<JobType> jobTypes = default;
        ReadOnlySpan<JobData> jobData = default;
        gameState.GetJobTypes(ref jobTypes);
        gameState.GetJobData(ref jobData);

        Span<JobType> unassignedJobs =
            stackalloc JobType[jobTypes.Length];

        for (int i = 0; i < jobData.Length; ++i)
        {
            mutableJobs.Add(jobData[i]);
        }

        upgradesDiscoverability.BecomeResponder(this);

        // Bind
        techUpgradesListView.OnIndexSelected += (int idx)
            => OnItemSelected(techUpgradesListView, idx);

        structuresListView.OnIndexSelected += (int idx)
            => OnItemSelected(structuresListView, idx);

        jobsListView.OnIndexSelected += (int idx)
            => OnItemSelected(jobsListView, idx);
    }

    void WriteToDisk()
    {
        // TODO: Implement Disk Write
    }

    void OnItemSelected(UIDataListView listView, int idx)
    {
        Resource data = null;

        if (listView == techUpgradesListView)
        {
            data = mutableUpgrades[idx];
        }
        else if (listView == structuresListView)
        {
            data = mutableStructures[idx];
        }
        else if (listView == jobsListView)
        {
            ReadOnlySpan<JobData> jobs = default;
            gameState.GetJobData(ref jobs);

            data = jobs[idx];
        }

        inspector.SetContents(data, idx, this);
    }

    /// Data List Provider ///

    public int DataProviderGetCount(UIDataListView listView)
    {
        if (listView == techUpgradesListView)
        {
            return mutableUpgrades.Count;
        }
        else if (listView == structuresListView)
        {
            return mutableStructures.Count;
        }
        else if (listView == jobsListView)
        {
            ReadOnlySpan<JobType> jobs = default;
            gameState.GetJobTypes(ref jobs);

            return jobs.Length;
        }

        return 0;
    }

    public ListItemInfo DataProviderGetItem(UIDataListView listView, int index)
    {
        if (listView == techUpgradesListView)
        {
            TechUpgradeInfo upgrade = mutableUpgrades[index];
            TechInfo info = TechInfo.FromType(upgrade.Id, upgrade.UpgradeType);

            return new(upgrade.DisplayName, info.Data.GetImage());
        }
        else if (listView == structuresListView)
        {
            StructureDataInfo structure = mutableStructures[index];
            return new(structure.DisplayName, structure.Icon);
        }
        else if (listView == jobsListView)
        {
            ReadOnlySpan<JobType> jobs = default;
            gameState.GetJobTypes(ref jobs);

            return new(jobs[index].ToString());
        }

        return new(null);
    }

    public void DataProviderCreateNewItem(UIDataListView listView)
    {
        if (listView == techUpgradesListView)
        {
        }
        else if (listView == structuresListView)
        {
            mutableStructures.Add(new()
            {
                Identifier = "",
                DisplayName = "New Structure",
                Cost = new ResourceRequirement[0],
                HarvestInfo = new StructureHarvestInfo[0]
            });
        }
    }

    public void DataProviderRemoveItem(UIDataListView listView, int index)
    {
        if (listView == techUpgradesListView)
        {
            mutableUpgrades.RemoveAt(index);
        }
        else if (listView == structuresListView)
        {
            mutableStructures.RemoveAt(index);
        }
    }

    /// Context Writer ///

    public void EditorPerformWrite<[MustBeVariant] T>(int index, T data)
    {
        if (data is StructureDataInfo structureInfo)
        {
            mutableStructures[index] = structureInfo;
            structuresListView.Update();
        }
    }

    /// Upgrade Discoverability ///

    public void EditorListTechUpgradeIds(ref Span<TechUpgradeInfo> upgrades)
    {
        upgrades = CollectionsMarshal.AsSpan(mutableUpgrades);
    }
}
