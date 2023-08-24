using ENet;

namespace RRU;

public sealed partial class TechNodeDetails : Control
{
    [Export] GameState gameState;
    [Export] TechDataService dataService;
    [Export] HSplitContainer splitView;

    Tween tween;

    TextureRect icon;
    Label labelType;
    Label labelDescription;
    Label labelStatus;
    Button buttonResearch;

    Label modifiersLabel;
    Control modifiersView;

    Label costLabel;
    Control costView;

    Control prerequisiteView;
    Label prerequisiteLabel;
    Control requirementsView;

    TechInfo info;
    bool wasVisible;

    float updateTick;

    public override void _Ready()
    {
        icon = GetNode<TextureRect>("%Icon");
        labelType = GetNode<Label>("%Type");
        labelDescription = GetNode<Label>("%Description");

        modifiersLabel = GetNode<Label>("%ModifiersLabel");
        modifiersView = GetNode<Control>("%Modifiers");

        costLabel = GetNode<Label>("%CostLabel");
        costView = GetNode<Control>("%Cost");

        prerequisiteLabel = GetNode<Label>("%PrerequisiteLabel");
        prerequisiteView = GetNode<Control>("%Prerequisites");
        requirementsView = GetNode<Control>("%Requirements");

        labelStatus = GetNode<Label>("%ResearchState");
        buttonResearch = GetNode<Button>("%BtnResearch");
        buttonResearch.Pressed += OnResearchPressed;

        SetResearchState(false);
        Modulate = Colors.Transparent;
    }

    public override void _Process(double delta)
    {
        if (info == null)
            return;

        updateTick += (float) delta;

        if (updateTick < 0.5f)
            return;

        // Periodically check for material requirements;
        UpdateResearchButtonState(dataService.IsResearched(info.Id));
        updateTick = 0.0f;
    }

    /// Helpers ///

    void UpdateResearchButtonState(bool isResearched)
    {
        TechUpgradeInfo upgradeInfo = dataService.GetInfoForId(info?.Id);

        bool isLocked = !dataService.IsUnlocked(info?.Id);
        bool canPurchase = gameState.CanPurchaseUpgrade(upgradeInfo);

        buttonResearch.Disabled = isResearched || isLocked || !canPurchase;

        if (isResearched)
            return;

        costView.Modulate = canPurchase ? Colors.SpringGreen : Colors.Salmon;
    }

    void SetResearchState(bool isResearched)
    {
        UpdateResearchButtonState(isResearched);
        labelStatus.Text = isResearched ? Tr("RESEARCHED") : Tr("NOT_RESEARCHED");
    }

    void ClearListView(Control view)
    {
        for (int i = view.GetChildCount(); i --> 0;)
            view.GetChild(i).QueueFree();
    }

    void AppendListItem(Control view, string text)
    {
        GLabel label = new(text) {
            HorizontalAlignment = HorizontalAlignment.Left
        };

        view.AddChild(label);
    }

    void UpdateDetails()
    {
        TechUpgradeInfo upgradeInfo = dataService.GetInfoForId(info.Id);

        ReadOnlySpan<string> requirements = upgradeInfo.RequiredUpgradeIds;
        ReadOnlySpan<ResourceModifierDefinition> modifiers = upgradeInfo.Modifiers;
        ReadOnlySpan<ResourceRequirement> cost = upgradeInfo.UpgradeCost;

        prerequisiteView.Visible = requirements.Length > 0;

        // Clear requirements
        if (requirements.Length > 0)
        {
            // Iterate from count to zero
            ClearListView(requirementsView);

            prerequisiteLabel.Text = requirements.Length > 1 ?
                "Required Upgrades" : "Required Upgrade";
        }

        // Clear cost
        if (cost.Length > 0)
        {
            ClearListView(costView);

            costLabel.Text = cost.Length > 1 ?
                "Required Materials" : "Required Material";
        }

        // Clear modifiers
        ClearListView(modifiersView);

        modifiersLabel.Text = modifiers.Length > 1 ?
            "Effects" : "Effect";

        // Update details in a single pass
        int l = Math.Max(
            cost.Length,
            Math.Max(requirements.Length, modifiers.Length)
        );

        for (int i = 0; i < l; ++i)
        {
            UpdateRequirements(i, requirements);
            UpdateCost(i, cost);
            UpdateModifiers(i, modifiers);
        }
    }

    void UpdateRequirements(int i, ReadOnlySpan<string> requirements)
    {
        if (i >= requirements.Length)
            return;

        TechUpgradeInfo requirementInfo =
            dataService.GetInfoForId(requirements[i]);

        AppendListItem(
            view: requirementsView,
            text: $"* {requirementInfo.DisplayName}"
        );
    }

    void UpdateCost(int i , ReadOnlySpan<ResourceRequirement> cost)
    {
        if (i >= cost.Length)
            return;

        AppendListItem(
            view: costView,
            text: $"* {cost[i].Type} x{cost[i].Amount}"
        );
    }

    void UpdateModifiers(int i, ReadOnlySpan<ResourceModifierDefinition> modifiers)
    {
        if (i >= modifiers.Length)
            return;

        string modLabelText = null;

        switch (modifiers[i].ModType)
        {
            case ResourceModifierType.Additive:
                modLabelText =
                    $"* {modifiers[i].TargetResource} " +
                    $"Output + {modifiers[i].ModValue}";
                break;

            case ResourceModifierType.Multiplicative:
                modLabelText =
                    $"* {modifiers[i].TargetResource} " +
                    $"Output + {modifiers[i].ModValue * 100.0:0.0} %";
                break;
        }

        AppendListItem(modifiersView, modLabelText);
    }

    void SetVisibility(bool visible)
    {
        // Prevent the tween from repeating the same operation
        if (wasVisible == visible)
            return;

        // Stop the currently-running tween (if there is any)
        if (IsInstanceValid(tween) && tween.IsRunning())
            tween.Kill();

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

    public void OnShowDetailRequested(TechNodeClickedInfo info)
    {
        if (info == null)
        {
            OnHideRequested();
            return;
        }

        TechUpgradeInfo upgradeInfo =
            dataService.GetInfoForId(id: info.TechInfo.Id);

        this.info = info.TechInfo;

        SetVisibility(true);

        labelType.Text = upgradeInfo.DisplayName;

        labelDescription.Text = info.TechInfo.Data.Description;
        icon.Texture = info.TechInfo.Data.GetImage();

        SetResearchState(dataService.IsResearched(info.TechInfo.Id));
        UpdateDetails();

        updateTick = 0f;
    }

    public void OnHideRequested()
    {
        SetVisibility(false);
    }

    void OnResearchPressed()
    {
        gameState.ConsumeUpgradeMaterials(dataService.GetInfoForId(info.Id));

        dataService.Research(info.Id);
        SetResearchState(true);
    }
}
