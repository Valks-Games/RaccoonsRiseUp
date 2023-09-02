namespace RRU.ResourceEditor;

public sealed partial class UIInspectorJobData : BaseInspectorView
{
    [Export] GameState gameState;

    OptionButton comboboxResource;
    Label labelJob;
    UIStepperControl stepperRate;
    UIStepperControl stepperAmount;

    JobData jobData;

    public override void _Ready()
    {
        // Get
        labelJob = GetNode<Label>("%Job");
        stepperRate = GetNode<UIStepperControl>("%Rate");
        stepperAmount = GetNode<UIStepperControl>("%Amount");
        comboboxResource = GetNode<OptionButton>("%Resource");

        // Init
        ReadOnlySpan<ResourceType> resourceTypes = default;
        gameState.GetResourceTypes(ref resourceTypes);

        for (int i = 0; i < resourceTypes.Length; ++i)
        {
            comboboxResource.AddItem(resourceTypes[i].ToString());
        }

        // Bind
        comboboxResource.ItemSelected += OnResourceChanged;
        stepperRate.OnValueChanged += OnRateUpdated;
        stepperAmount.OnValueChanged += OnAmountUpdated;
    }

    public override async void Configure(Resource data, int index, IEditorContextWriter writer)
    {
        base.Configure(data, index, writer);
        jobData = (JobData) data;

        await GUtils.WaitOneFrame(this);

        labelJob.Text = jobData.Job.ToString();

        comboboxResource.Select((int) jobData.ResourceType);
        stepperRate.SetValue((float) jobData.GatherRate);
        stepperAmount.SetValue((float) jobData.GatherAmount);
    }

    public override void Finalise()
    {
        writer.EditorPerformWrite(index, jobData);
    }

    /// Events ///

    void OnResourceChanged(long selectedIdx)
    {
        jobData.ResourceType = (ResourceType) selectedIdx;
        Finalise();
    }

    void OnRateUpdated(float value)
    {
        jobData.GatherRate = value;
        Finalise();
    }

    void OnAmountUpdated(float value)
    {
        jobData.GatherAmount = value;
        Finalise();
    }
}
