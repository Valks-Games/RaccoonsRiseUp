namespace RRU;

public sealed partial class TechNodeDetails : Control
{
    const string TEXT_STATUS_LEARNED = "Learned";
    const string TEXT_STATUS_NOT_LEARNED = "Not Learned";

    const string TEXT_REQUIRED_SINGULAR = "Required Upgrade";
    const string TEXT_REQUIRED_PLURAL = "Required Upgrades";

    [Export] TechDataService dataService;
    [Export] HSplitContainer splitView;

    Tween tween;

    TextureRect icon;
    Label labelType;
    Label labelDescription;
    Label labelStatus;
    Button buttonLearn;

    Control prerequisiteView;
    Label prerequisiteLabel;
    Control requirementsView;

    TechInfo info;
    bool wasVisible;

    public override void _Ready()
    {
        prerequisiteLabel = GetNode<Label>("%PrerequisiteLabel");
        prerequisiteView = GetNode<Control>("%Prerequisites");
        requirementsView = GetNode<Control>("%Requirements");
        labelDescription = GetNode<Label>("%Description");
        labelStatus = GetNode<Label>("%LearnState");
        icon = GetNode<TextureRect>("%Icon");
        labelType = GetNode<Label>("%Type");

        buttonLearn = GetNode<Button>("%BtnLearn");
        buttonLearn.Pressed += OnLearnPressed;

        SetLearnState(false);
        Modulate = Colors.Transparent;
    }

    /// Helpers ///

    void SetLearnState(bool isLearned)
    {
        bool isLocked = !dataService.IsUnlocked(info?.Id);

        buttonLearn.Disabled = isLearned || isLocked;
        labelStatus.Text = isLearned ? TEXT_STATUS_LEARNED : TEXT_STATUS_NOT_LEARNED;
    }

    void UpdateDetails()
    {
        TechUpgradeInfo upgradeInfo = dataService.GetInfoForId(info.Id);
        ReadOnlySpan<string> requirements = upgradeInfo.RequiredUpgradeIds;

        prerequisiteView.Visible = requirements.Length > 0;

        if (requirements.Length < 1)
            return;

        // Clear requirements
        for (int i = requirementsView.GetChildCount(); i --> 0;)
        {
            requirementsView
                .GetChild(i)
                .QueueFree();
        }

        prerequisiteLabel.Text = requirements.Length > 1 ? TEXT_REQUIRED_PLURAL : TEXT_REQUIRED_SINGULAR;

        // Update requirements
        for (int i = 0; i < requirements.Length; ++ i)
        {
            TechUpgradeInfo requirementInfo = dataService.GetInfoForId(requirements[i]);

            requirementsView.AddChild(
                new Label()
                {
                    Text = $"* {requirementInfo.DisplayName}"
                }
            );
        }
    }

    void SetVisibility(bool visible)
    {
        // Prevent the tween from repeating the same operation
        if (wasVisible == visible)
            return;

        // Stop the currently-running tween (if there is any)
        if (IsInstanceValid(tween) && tween.IsRunning())
        {
            tween.Kill();
        }

        tween = CreateTween();
        tween.SetParallel(true);
        wasVisible = visible;

        // Slide
        tween.TweenProperty(
            @object: splitView,
            property: SplitContainer.PropertyName.SplitOffset.ToString(),
            finalVal: visible ? -300 : 0,
            duration: 0.25f
        );

        // Fade
        Modulate = visible ? Colors.Transparent : Colors.White;

        tween.TweenProperty(
            @object: this,
            property: CanvasItem.PropertyName.Modulate.ToString(),
            finalVal: visible ? Colors.White : Colors.Transparent,
            duration: visible ? 0.8f : 0.15f
        );
    }

    /// Signal Handlers ///

    public void OnShowDetailRequested(TechInfo info)
    {
        if (info == null)
        {
            OnHideRequested();
            return;
        }

        TechUpgradeInfo upgradeInfo = dataService.GetInfoForId(id: info.Id);
        this.info = info;

        SetVisibility(true);

        labelType.Text = upgradeInfo.DisplayName;

        labelDescription.Text = info.Data.Description;
        icon.Texture = info.Data.GetImage();

        SetLearnState(dataService.IsLearned(info.Id));
        UpdateDetails();
    }

    public void OnHideRequested()
    {
        SetVisibility(false);
    }

    void OnLearnPressed()
    {
        dataService.Learn(info.Id);
        SetLearnState(true);
    }
}