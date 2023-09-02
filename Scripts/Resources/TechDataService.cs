namespace RRU;

[GlobalClass]
public sealed partial class TechDataService : Resource
{
    public event Action<TechDataService, StringName, bool> ResearchStateUpdated;

    // Note that renaming this will cause the resource to get reset
    // Not worth the hassle
    [Export] TechUpgradeInfo[] _upgrades;

    readonly HashSet<StringName> researchedUpgrades;

    public TechDataService()
    {
        researchedUpgrades = new();
    }

    /// <summary>
    /// (Must only be called by the Resource Editor.)
    /// </summary>
    public void _SetUpgrades(TechUpgradeInfo[] upgrades)
    {
        _upgrades = upgrades;
    }

    /// Upgrades ///

    public void Research(StringName id)
    {
        researchedUpgrades.Add(id);
        ResearchStateUpdated.Invoke(this, id, true);
    }

    public void Unresearch(StringName id)
    {
        if (!researchedUpgrades.Contains(id))
            return;

        researchedUpgrades.Remove(id);
        ResearchStateUpdated.Invoke(this, id, false);
    }

    public bool IsResearched(StringName id) =>
        researchedUpgrades.Contains(id);

    public bool IsUnlocked(StringName id)
    {
        if (id == null)
            return false;

        TechUpgradeInfo info = GetInfoForId(id);

        Debug.Assert(
            condition: info != null,
            message: $"Invalid upgrade ID: {id}"
        );

        ReadOnlySpan<string> prerequisiteIds = info.RequiredUpgradeIds;

        for (int i = 0; i < prerequisiteIds.Length; ++i)
        {
            if (IsResearched(prerequisiteIds[i]))
                continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Writes to a span pointing to a list of all available tech upgrades.
    /// </summary>
    public void GetAllUpgrades(ref ReadOnlySpan<TechUpgradeInfo> upgrades)
    {
        upgrades = _upgrades;
    }

    /// <summary>
    /// Writes to a span pointing to a list of researched tech upgrades
    /// </summary>
    public void GetResearchedUpgrades(ref Span<TechUpgradeInfo> upgrades)
    {
        ReadOnlySpan<TechUpgradeInfo> allUpgrades = _upgrades;

        Span<TechUpgradeInfo> researched = new TechUpgradeInfo[allUpgrades.Length];
        int researchIdx = 0;

        for (int i = 0; i < allUpgrades.Length; ++i)
        {
            if (!researchedUpgrades.Contains(allUpgrades[i].Id))
                continue;

            researched[researchIdx] = allUpgrades[i];
            researchIdx++;
        }

        upgrades = researched[..researchIdx];
    }

    public TechUpgradeInfo GetInfoForId(StringName id)
    {
        if (id == null)
            return null;

        ReadOnlySpan<TechUpgradeInfo> upgrades = _upgrades;

        for (int i = 0; i < upgrades.Length; ++i)
        {
            if (upgrades[i].Id != id)
                continue;

            return upgrades[i];
        }

        return null;
    }

    public void Reset() => researchedUpgrades.Clear();

    /// Serialisation ///

    /// <summary>
    /// Returns an array containing all currently-researched upgrade options.
    /// </summary>
    public string[] Serialise()
    {
        Span<string> upgrades = new string[_upgrades.Length];
        int upgradeIdx = 0;

        foreach (StringName id in researchedUpgrades)
        {
            upgrades[upgradeIdx] = id;
            upgradeIdx++;
        }

        return upgrades[..upgradeIdx].ToArray();
    }

    public void Deserialise(string[] serialisedArray)
    {
        ReadOnlySpan<string> ids = serialisedArray;

        researchedUpgrades.Clear();

        for (int i = 0; i < ids.Length; ++i)
            researchedUpgrades.Add(ids[i]);
    }
}
