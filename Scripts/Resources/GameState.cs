namespace RRU;

/// <summary>
/// This resource's purpose is to allow communication between various parts
/// of the application without needing direct references to one another.
/// </summary>
[GlobalClass]
public sealed partial class GameState : Resource
{
    public event Action<ResourceDict> ResourcesChanged;
    public event Action<JobDict> JobsChanged;
    public event Action<StructureDict> StructuresChanged;

    [Export] public JobData[] JobData;
    [Export] TechDataService techDataService;
    [Export] StructureDataService structureDataService;

    public int Raccoons { get; set; } = 30;

    public JobDict Jobs { get; set; }
    public StructureDict Structures { get; set; }
    public ResourceDict Resources { get; set; }

    readonly JobType[] jobTypes;
    readonly ResourceType[] resourceTypes;

    public GameState()
    {
        Jobs = new();
        Resources = new();
        Structures = new();

        // Performance cost should be negligible since initializing all the
        // dictionaries like this should only be called once.
        // Using this way should be more future-proof as the previous
        // implementation requires someone to update each dictionary
        // whenever a new type is added.

        jobTypes = (JobType[]) Enum.GetValues(typeof(JobType));
        resourceTypes = (ResourceType[]) Enum.GetValues(typeof(ResourceType));

        for (int i = 0; i < jobTypes.Length; i++)
            Jobs[jobTypes[i]] = 0;

        for (int i = 0; i < resourceTypes.Length; i++)
            Resources[resourceTypes[i]] = 0;
    }

    public void UpdateResources() => ResourcesChanged?.Invoke(Resources);
    public void UpdateJobs() => JobsChanged?.Invoke(Jobs);
    public void UpdateStructures() => StructuresChanged?.Invoke(Structures);

    /// Resources ///

    public void GetResourceTypes(ref ReadOnlySpan<ResourceType> types) =>
        types = resourceTypes;

    public double GetResourceCount(ResourceType type)
    {
        if (!Resources.ContainsKey(type))
            return 0.0;

        return Resources[type];
    }

    public bool HasResource(ResourceType type, double amount)
    {
        if (!Resources.ContainsKey(type))
            return false;

        return Resources[type] >= amount;
    }

    public void AddResource(ResourceType type, double amount)
    {
        Resources[type] += amount;
        UpdateResources();
    }

    public void TakeResource(ResourceType type, double amount)
    {
        if (!Resources.ContainsKey(type))
            return;

        Resources[type] = Mathf.Max(0, Resources[type] - amount);
        UpdateResources();
    }

    /// Jobs ///

    public void GetJobTypes(ref ReadOnlySpan<JobType> types) =>
        types = jobTypes;

    public bool AddJob(JobType job)
    {
        if (Raccoons <= 0)
        {
            UpdateJobs();
            return false;
        }

        Jobs[job]++;
        Raccoons--;

        UpdateJobs();
        return true;
    }

    public bool RemoveJob(JobType job)
    {
        if (Jobs[job] <= 0)
            return false;

        Jobs[job]--;
        Raccoons++;

        UpdateJobs();
        return true;
    }

    /// Structures ///

    public int GetStructureCount(StringName id)
    {
        if (!Structures.TryGetValue(id, out int currentCount))
            return 0;

        return currentCount;
    }

    public void AddStructure(StringName id, int count)
    {
        if (!Structures.TryGetValue(id, out int currentCount))
        {
            Structures[id] = count;
            UpdateStructures();

            return;
        }

        Structures[id] = Math.Max(0, currentCount + count);
        UpdateStructures();
    }

    public void RemoveStructure(StringName id, int count)
    {
        if (!Structures.ContainsKey(id))
            return;

        Structures[id] = Math.Max(0, Structures[id] - count);
        UpdateStructures();
    }

    /// Upgrades ///

    public bool CanPurchaseUpgrade(TechUpgradeInfo upgrade)
    {
        if (upgrade == null)
            return false;

        // Check if all the required materials are present in the inventory
        return HasResources(upgrade.UpgradeCost);
    }

    public void ConsumeUpgradeMaterials(TechUpgradeInfo upgrade)
    {
        if (upgrade == null)
            return;

        ConsumeResources(upgrade.UpgradeCost);
    }

    /// Resource Requirements ///

    /// <summary>
    /// Returns true if all resource requirements are satisfied.
    /// </summary>
    /// <returns></returns>
    public bool HasResources(ReadOnlySpan<ResourceRequirement> requirements)
    {
        for (int i = 0; i < requirements.Length; ++i)
        {
            if (HasResource(requirements[i].Type, requirements[i].Amount))
                continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Consumes the materials specified in the requirements collection.
    /// </summary>
    public void ConsumeResources(ReadOnlySpan<ResourceRequirement> requirements)
    {
        for (int i = 0; i < requirements.Length; ++i)
        {
            Resources[requirements[i].Type] -= requirements[i].Amount;
        }

        UpdateResources();
    }
}
