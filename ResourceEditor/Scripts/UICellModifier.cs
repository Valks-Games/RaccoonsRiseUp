namespace RRU.ResourceEditor;

public partial class UICellModifier : BaseEditParamsCell
{
    [Export] GameState gameState;

    OptionButton fieldTarget;
    OptionButton fieldModType;
    UIStepperControl fieldAmount;

    ResourceModifierDefinition modifier;

    public override void Initialise()
    {
        // Get
        fieldTarget = GetNode<OptionButton>("%Target");
        fieldModType = GetNode<OptionButton>("%ModType");
        fieldAmount = GetNode<UIStepperControl>("%ModAmount");

        // Init
        ReadOnlySpan<ResourceType> resourceTypes = default;
        ReadOnlySpan<ResourceModifierType> modTypes = default;

        gameState.GetResourceTypes(ref resourceTypes);
        modTypes = (ResourceModifierType[]) Enum.GetValues(typeof(ResourceModifierType));

        for (int i = 0; i < resourceTypes.Length; ++i)
        {
            fieldTarget.AddItem(resourceTypes[i].ToString());
        }

        for (int i = 0; i < modTypes.Length; ++i)
        {
            fieldModType.AddItem(modTypes[i].ToString());
        }

        // Bind
        fieldTarget.ItemSelected += OnTargetChanged;
        fieldModType.ItemSelected += OnModTypeChanged;
        fieldAmount.OnValueChanged += OnModAmountChanged;
    }

    public void Sync(ResourceModifierDefinition modifier)
    {
        this.modifier = modifier;

        fieldTarget.Select((int) modifier.TargetResource);
        fieldModType.Select((int) modifier.ModType);
        fieldAmount.SetValue((float) modifier.ModValue, true);
    }

    public override void UpdateSource()
    {
        listView.EditorPerformWrite(GetIndex(), modifier);
    }

    /// Events ///

    void OnTargetChanged(long value)
    {
        modifier.TargetResource = (ResourceType) value;
        UpdateSource();
    }

    void OnModTypeChanged(long value)
    {
        modifier.ModType = (ResourceModifierType) value;
        UpdateSource();
    }

    void OnModAmountChanged(float value)
    {
        modifier.ModValue = value;
        UpdateSource();
    }
}
