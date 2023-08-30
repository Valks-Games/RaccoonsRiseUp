using System.Runtime.InteropServices;

namespace RRU.ResourceEditor;

[GlobalClass]
public sealed partial class ResourceEditorMain : Control, IDataListViewDataProvider, IEditorContextWriter, IEditorTechUpgradeDiscoverability
{
    [Export(PropertyHint.File)] string pathResourceGameState;
    [Export(PropertyHint.File)] string pathResourceTechDataService;
    [Export(PropertyHint.File)] string pathResourceStructureDataService;
    [Export] EditorTechUpgradeDiscoverability upgradesDiscoverability;

    GameState gameState;
    TechDataService techDataService;
    StructureDataService structureDataService;

    UIDataListView techUpgradesListView;
    UIDataListView structuresListView;
    UIDataListView jobsListView;
    UIDataInspector inspector;

    List<StructureDataInfo> mutableStructures;
    List<TechUpgradeInfo> mutableUpgrades;
    List<JobData> mutableJobs;

    public override void _Ready()
    {
        gameState = ResourceLoader
        .Load<GameState>(pathResourceGameState);

        techDataService = ResourceLoader
            .Load<TechDataService>(pathResourceTechDataService);

        structureDataService = ResourceLoader
            .Load<StructureDataService>(pathResourceStructureDataService);

        // Get
        inspector = GetNode<UIDataInspector>("%Inspector");
        techUpgradesListView = GetNode<UIDataListView>("%Tech Upgrades");
        structuresListView = GetNode<UIDataListView>("%Structures");
        jobsListView = GetNode<UIDataListView>("%Jobs");

        // Init
        techUpgradesListView.DataProvider = this;
        structuresListView.DataProvider = this;
        jobsListView.DataProvider = this;

        InitialUpdate();

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

        for (int i = 0; i < jobTypes.Length; ++i)
        {
            bool hasData = false;

            for (int j = 0; j < jobData.Length; ++j)
            {
                if (jobData[j].Job != jobTypes[i])
                    continue;

                hasData = true;

                mutableJobs.Add(jobData[j]);
                break;
            }

            if (hasData)
                continue;

            mutableJobs.Add(new()
            {
                Job = jobTypes[i]
            });
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

    async void InitialUpdate()
    {
        await GUtils.WaitOneFrame(this);

        techUpgradesListView.Update();
        structuresListView.Update();
        jobsListView.Update();
    }

    void WriteToDisk()
    {
        gameState._SetJobData(mutableJobs.ToArray());
        techDataService._SetUpgrades(mutableUpgrades.ToArray());
        structureDataService._SetStructures(mutableStructures.ToArray());

        ResourceSaver.Save(gameState, pathResourceGameState);
        ResourceSaver.Save(techDataService, pathResourceTechDataService);
        ResourceSaver.Save(structureDataService, pathResourceStructureDataService);
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
            data = mutableJobs[idx];
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
            return mutableJobs.Count;
        }

        return 0;
    }

    public ListItemInfo DataProviderGetItem(UIDataListView listView, int index)
    {
        if (listView == techUpgradesListView)
        {
            TechUpgradeInfo upgrade = mutableUpgrades[index];
            return new(upgrade.DisplayName, upgrade.Icon);
        }
        else if (listView == structuresListView)
        {
            StructureDataInfo structure = mutableStructures[index];
            return new(structure.DisplayName, structure.Icon);
        }
        else if (listView == jobsListView)
        {
            return new(mutableJobs[index].Job.ToString());
        }

        return new(null);
    }

    public void DataProviderCreateNewItem(UIDataListView listView)
    {
        if (listView == techUpgradesListView)
        {
            mutableUpgrades.Add(new()
            {
                Id = "",
                DisplayName = "New Upgrade",
                UpgradeCost = new ResourceRequirement[0],
                RequiredUpgradeIds = new string[0],
                Modifiers = new ResourceModifierDefinition[0]
            });
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
        if (data is TechUpgradeInfo upgradeInfo)
        {
            mutableUpgrades[index] = upgradeInfo;
            techUpgradesListView.Update();
        }
        else if (data is StructureDataInfo structureInfo)
        {
            mutableStructures[index] = structureInfo;
            structuresListView.Update();
        }
        else if (data is JobData jobData)
        {
            mutableJobs[index] = jobData;
        }

        WriteToDisk();
    }

    /// Upgrade Discoverability ///

    public void EditorListTechUpgradeIds(ref Span<TechUpgradeInfo> upgrades)
    {
        upgrades = CollectionsMarshal.AsSpan(mutableUpgrades);
    }
}