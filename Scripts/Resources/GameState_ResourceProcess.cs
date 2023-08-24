namespace RRU;

public sealed partial class GameState
{
    // Change this depending on the needs of the project, but this should
    // be enough for the time being
    // Consider increasing it if
    // (tech upgrades count + job type count + structure type count) > current modifier limit
    const int MAX_MODIFIERS = 64;

    public void ProcessResourceTick(double delta)
    {
        Span<IResourceModifier> modifiers = 
            new IResourceModifier[MAX_MODIFIERS];

        int modifierIdx = 0;

        // Add job + structure modifiers
        ReadOnlySpan<JobData> jobs = JobData;

        for (int i = 0; i < jobs.Length; ++i)
        {
            AppendToSpan(jobs[i], delta, ref modifiers, ref modifierIdx);
        }

        // TODO: Add structure-related instructions once it is implemented

        // Add upgrade modifiers
        Span<TechUpgradeInfo> techUpgrades = default;
        dataService.GetResearchedUpgrades(ref techUpgrades);

        for (int i = 0; i < techUpgrades.Length; ++i)
        {
            ReadOnlySpan<ResourceModifierDefinition> modifierDefs = 
                techUpgrades[i].Modifiers;

            for (int j = 0; j < modifierDefs.Length; ++j)
            {
                AppendToSpan(
                    modifierDefs[j], 
                    delta,
                    ref modifiers, 
                    ref modifierIdx);
            }
        }

        modifiers = modifiers[..modifierIdx];
        EvaluateModifiers(modifiers, delta);
    }

    /// <summary>
    /// Performs the actual evaluation of resource modifiers.
    /// </summary>
    /// <param name="modifiers"></param>
    /// <param name="delta"></param>
    void EvaluateModifiers(Span<IResourceModifier> modifiers, double delta)
    {
        ReadOnlySpan<ResourceType> resourceTypes = default;
        GetResourceTypes(ref resourceTypes);

        Span<ResourceMultiplier> multipliers =
            stackalloc ResourceMultiplier[resourceTypes.Length];

        ProcessMultiplicatives(multipliers, modifiers, delta);
        ProcessAdditives(multipliers, modifiers, delta);

        UpdateResources();
    }

    /// Passes ///

    /// <summary>
    /// <para>
    /// * Multiplier pass *
    /// </para>
    /// <para>
    /// Iterates through the modifiers list and accumulates the multipliers 
    /// of each resource types.
    /// </para>
    /// </summary>
    void ProcessMultiplicatives(
        Span<ResourceMultiplier> multipliers,
        Span<IResourceModifier> modifiers,
        double delta)
    {
        // Set default multipliers
        for (int i = 0; i < resourceTypes.Length; ++i)
        {
            multipliers[i] = new(
                resource: resourceTypes[i],
                multiplier: 1.0
            );
        }

        // Accumulate modifiers from all sources
        for (int i = 0; i < modifiers.Length; ++i)
        {
            ResourceModifier modifier = default;
            modifiers[i].ModifierGet(this, ref modifier);

            if (modifier.Type != ResourceModifierType.Multiplicative)
                continue;

            for (int j = 0; j < multipliers.Length; ++j)
            {
                if (multipliers[j].Resource != modifier.Resource)
                    continue;

                multipliers[j].Multiplier += modifier.Amount;
                break;
            }
        }
    }

    /// <summary>
    /// <para>
    /// + Additive Pass +
    /// </para>
    /// <para>
    /// Iterates through the active modifier list and adds their value to 
    /// the total resource counts. (Affected by multipliers.)
    /// </para>
    /// </summary>
    void ProcessAdditives(
        Span<ResourceMultiplier> multipliers,
        Span<IResourceModifier> modifiers,
        double delta)
    {
        for (int i = 0; i < modifiers.Length; ++i)
        {
            ResourceModifier modifier = default;
            modifiers[i].ModifierGet(this, ref modifier);

            if (modifier.Type != ResourceModifierType.Additive)
                continue;

            double modTotal = 
                GetMultiplier(multipliers, modifier.Resource) * modifier.Amount;

            Resources[modifier.Resource] =
                Mathf.Max(0.0f, Resources[modifier.Resource] + modTotal);
        }
    }

    /// Helpers ///

    void AppendToSpan(
        IResourceModifier modifier,
        double delta,
        ref Span<IResourceModifier> modifiers,
        ref int index)
    {
        // Only allow active modifiers
        if (!modifier.ModifierIsActive(this, delta))
            return;

        modifiers[index] = modifier;
        index ++;
    }

    /// <summary>
    /// Returns the accumulated multiplier for a given resource type.
    /// </summary>
    double GetMultiplier(Span<ResourceMultiplier> multipliers, ResourceType type)
    {
        for (int j = 0; j < multipliers.Length; ++j)
        {
            if (type != multipliers[j].Resource)
                continue;

            return multipliers[j].Multiplier;
        }

        return 1.0;
    }

    struct ResourceMultiplier
    {
        public ResourceType Resource { get; private set; }
        public double Multiplier { get; set; }

        public ResourceMultiplier(ResourceType resource, double multiplier)
        {
            Resource = resource;
            Multiplier = multiplier;
        }
    }
}

/// <summary>
/// This interface must be implemented, if one wishes to make changes to the game state's resources.
/// </summary>
public interface IResourceModifier
{
    public bool ModifierIsActive(GameState context, double delta);
    public void ModifierGet(GameState context, ref ResourceModifier info);
}

public enum ResourceModifierType
{
    Additive,
    Multiplicative
}

public ref struct ResourceModifier
{
    public readonly ResourceType Resource;
    public readonly ResourceModifierType Type;
    public readonly double Amount;

    public ResourceModifier(
        ResourceType resource,
        ResourceModifierType type,
        double amount)
    {
        Resource = resource;
        Type = type;
        Amount = amount;
    }
}
