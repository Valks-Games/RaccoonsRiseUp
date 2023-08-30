namespace RRU.ResourceEditor;

public sealed partial class UIInspectorTechUpgrades : BaseInspectorView, IEditParamsListViewSyncDataProvider
{
    UIEditParamsListView prerequisitesListView;
    UIEditParamsListView costListView;
    UIEditParamsListView modifiersListView;

    LineEdit fieldIdentifier;
    LineEdit fieldName;
    TextEdit fieldDescription;

    UIImageWell iconView;
    UIIconBrowser iconBrowser;

    Godot.Timer descriptionDebounce;

    TechUpgradeInfo upgradeInfo;

    List<string> mutablePrerequisites;
    List<ResourceRequirement> mutableCost;
    List<ResourceModifierDefinition> mutableModifiers;

    public override void _Ready()
    {
        // Get
        prerequisitesListView = GetNode<UIEditParamsListView>("%Prerequisites");
        costListView = GetNode<UIEditParamsListView>("%Cost");
        modifiersListView = GetNode<UIEditParamsListView>("%Modifiers");

        fieldIdentifier = GetNode<LineEdit>("%Identifier");
        fieldName = GetNode<LineEdit>("%Name");
        fieldDescription = GetNode<TextEdit>("%Description");

        iconView = GetNode<UIImageWell>("%Icon");
        iconBrowser = GetNode<UIIconBrowser>("%IconBrowser");

        // Init
        prerequisitesListView.DataProvider = this;
        costListView.DataProvider = this;
        modifiersListView.DataProvider = this;

        // Bind
        fieldIdentifier.TextSubmitted += OnIdentifierChanged;
        fieldName.TextSubmitted += OnNameChanged;
        fieldDescription.TextChanged += OnDescriptionChanged;

        iconBrowser.PopupHide += OnIconSelectionAborted;
        iconBrowser.OnModalStarted += OnModalStarted;
        iconBrowser.OnIconSelected += OnIconSelected;

        ConfigureTimer();
    }

    async void ConfigureTimer()
    {
        await GUtils.WaitOneFrame(this);

        descriptionDebounce = new();
        AddChild(descriptionDebounce);

        descriptionDebounce.Timeout += OnDeferredDescriptionSync;
    }

    public override async void Configure(Resource data, int index, IEditorContextWriter writer)
    {
        base.Configure(data, index, writer);
        upgradeInfo = (TechUpgradeInfo) data;

        mutablePrerequisites = new(upgradeInfo.RequiredUpgradeIds);
        mutableCost = new(upgradeInfo.UpgradeCost);
        mutableModifiers = new(upgradeInfo.Modifiers);

        prerequisitesListView.Update();
        costListView.Update();
        modifiersListView.Update();

        await GUtils.WaitOneFrame(this);

        fieldIdentifier.Text = upgradeInfo.Id;
        fieldName.Text = upgradeInfo.DisplayName;
        fieldDescription.Text = upgradeInfo.DisplayDescription;

        iconView.SetImage(upgradeInfo.Icon);
    }

    public override void Finalise()
    {
        upgradeInfo.RequiredUpgradeIds = mutablePrerequisites.ToArray();
        upgradeInfo.UpgradeCost = mutableCost.ToArray();
        upgradeInfo.Modifiers = mutableModifiers.ToArray();

        writer.EditorPerformWrite(index, upgradeInfo);
    }

    /// Events ///

    void OnIconSelectionAborted()
    {
        Visible = true;
    }

    void OnModalStarted()
    {
        Visible = false;
    }

    void OnIconSelected(Texture2D icon)
    {
        Visible = true;
        upgradeInfo.Icon = icon;
        Finalise();
    }

    void OnIdentifierChanged(string value)
    {
        upgradeInfo.Id = value;
        Finalise();
    }

    void OnNameChanged(string value)
    {
        upgradeInfo.DisplayName = value;
        Finalise();
    }

    void OnDescriptionChanged()
    {
        if (!descriptionDebounce.IsStopped())
        {
            return;
        }

        upgradeInfo.DisplayDescription = fieldDescription.Text;
        descriptionDebounce.Start(0.2f);
        Finalise();
    }

    void OnDeferredDescriptionSync()
    {
        upgradeInfo.DisplayDescription = fieldDescription.Text;
        Finalise();
    }

    /// Data Provider ///

    public int DataProviderGetInitialCount(UIEditParamsListView listView)
    {
        if (listView == prerequisitesListView)
        {
            return mutablePrerequisites.Count;
        }
        else if (listView == costListView)
        {
            return mutableCost.Count;
        }
        else if (listView == modifiersListView)
        {
            return mutableModifiers.Count;
        }

        return 0;
    }

    public void DataProviderConfigureCell(UIEditParamsListView listView, BaseEditParamsCell cell, int index)
    {
        if (listView == prerequisitesListView)
        {
            UICellPrerequisiteId prerequisiteCell = (UICellPrerequisiteId) cell;
            prerequisiteCell.Sync(mutablePrerequisites[index]);
        }
        else if (listView == costListView)
        {
            UICellCost costCell = (UICellCost) cell;
            costCell.Sync(mutableCost[index]);
        }
        else if (listView == modifiersListView)
        {
            UICellModifier modifierCell = (UICellModifier) cell;
            modifierCell.Sync(mutableModifiers[index]);
        }
    }

    public void DataProviderRequestAdd(UIEditParamsListView listView)
    {
        if (listView == prerequisitesListView)
        {
            mutablePrerequisites.Add(null);
        }
        else if (listView == costListView)
        {
            mutableCost.Add(new()
            {
                Type = ResourceType.Wood,
                Amount = 1
            });
        }
        else if (listView == modifiersListView)
        {
            mutableModifiers.Add(new()
            {
                TargetResource = ResourceType.Wood,
                ModType = ResourceModifierType.Additive,
                ModValue = 0.1
            });
        }

        listView.Update();
    }

    public void DataProviderRequestRemove(UIEditParamsListView listView, int index)
    {
        if (listView == prerequisitesListView)
        {
            mutablePrerequisites.RemoveAt(index);
        }
        else if (listView == costListView)
        {
            mutableCost.RemoveAt(index);
        }
        else if (listView == modifiersListView)
        {
            mutableModifiers.RemoveAt(index);
        }

        listView.Update();
    }

    public void DataProviderRequestContextWrite<[MustBeVariant] T>(UIEditParamsListView listView, int index, T data)
    {
        if (data is string identifier)
        {
            mutablePrerequisites[index] = identifier;
        }
        else if (data is ResourceRequirement requirement)
        {
            mutableCost[index] = requirement;
        }
        else if (data is ResourceModifierDefinition modifier)
        {
            mutableModifiers[index] = modifier;
        }
    }
}
