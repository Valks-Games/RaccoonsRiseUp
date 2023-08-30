namespace RRU.ResourceEditor;

public sealed partial class UICellCost : BaseEditParamsCell
{
    [Export] GameState gameState;

    OptionButton comboboxResource;
    UIStepperControl stepperCount;
    UIStepperControl stepperPenalty;

    ResourceRequirement requirement;

    public override void Initialise()
    {
        // Get
        comboboxResource = GetNode<OptionButton>("%ResourceType");
        stepperCount = GetNode<UIStepperControl>("%Count");
        stepperPenalty = GetNode<UIStepperControl>("%Penalty");

        // Init
        ReadOnlySpan<ResourceType> resourceTypes = default;
        gameState.GetResourceTypes(ref resourceTypes);

        comboboxResource.Clear();

        for (int i = 0; i < resourceTypes.Length; ++i)
        {
            comboboxResource.AddItem(resourceTypes[i].ToString(), i);
        }

        // Bind
        comboboxResource.ItemSelected += OnHarvestTypeChanged;
        stepperCount.OnValueChanged += OnCountChanged;
        stepperPenalty.OnValueChanged += OnPenaltyChanged;
    }

    public override void UpdateSource()
    {
        listView.EditorPerformWrite(GetIndex(), requirement);
    }

    public async void Sync(ResourceRequirement requirement)
    {
        this.requirement = requirement;

        await GUtils.WaitOneFrame(this);

        comboboxResource.Select((int) requirement.Type);

        stepperCount.SetValue(requirement.Amount, true);
        stepperPenalty.SetValue((float) requirement.PenaltyModifier, true);
    }

    /// Events ///

    void OnHarvestTypeChanged(long selection)
    {
        requirement.Type = (ResourceType) selection;
        UpdateSource();
    }

    void OnCountChanged(float value)
    {
        requirement.Amount = Mathf.RoundToInt(stepperCount.Value);
        UpdateSource();
    }

    void OnPenaltyChanged(float value)
    {
        requirement.PenaltyModifier = stepperPenalty.Value;
        UpdateSource();
    }
}