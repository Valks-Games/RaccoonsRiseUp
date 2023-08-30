namespace RRU.ResourceEditor;

public sealed partial class UICellHarvest : BaseEditParamsCell
{
    [Export] GameState gameState;

    OptionButton comboboxResource;
    UIStepperControl stepperRate;
    UIStepperControl stepperAmount;

    StructureHarvestInfo info;

    public override void Initialise()
    {
        // Get
        comboboxResource = GetNode<OptionButton>("%Resource");
        stepperRate = GetNode<UIStepperControl>("%Rate");
        stepperAmount = GetNode<UIStepperControl>("%Amount");

        // Init
        ReadOnlySpan<ResourceType> resourceTypes = default;
        gameState.GetResourceTypes(ref resourceTypes);

        comboboxResource.Clear();

        for (int i = 0; i < resourceTypes.Length; ++i)
        {
            comboboxResource.AddItem(resourceTypes[i].ToString(), i);
        }

        // Bind
        comboboxResource.ItemSelected += OnTypeChanged;
        stepperRate.OnValueChanged += OnRateChanged;
        stepperAmount.OnValueChanged += OnAmountChanged;
    }

    public override void UpdateSource()
    {
        listView.EditorPerformWrite(GetIndex(), info);
    }

    public async void Sync(StructureHarvestInfo info)
    {
        this.info = info;

        await GUtils.WaitOneFrame(this);

        comboboxResource.Select((int) info.HarvestType);

        stepperRate.SetValue(info.HarvestRate, true);
        stepperAmount.SetValue(info.HarvestAmount, true);
    }

    /// Events ///

    void OnTypeChanged(long selectedIdx)
    {
        info.HarvestType = (ResourceType) selectedIdx;
        UpdateSource();
    }

    void OnRateChanged(float rate)
    {
        info.HarvestRate = stepperRate.Value;
        GD.Print(rate);
        UpdateSource();
    }

    void OnAmountChanged(float amount)
    {
        info.HarvestAmount = stepperAmount.Value;
        UpdateSource();
    }
}