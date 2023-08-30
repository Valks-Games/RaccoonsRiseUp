namespace RRU.ResourceEditor;

public partial class UIInspectorStructure : BaseInspectorView, IEditParamsListViewSyncDataProvider
{
    UIEditParamsListView listCost;
    UIEditParamsListView listHarvest;

    UIImageWell iconView;
    LineEdit fieldIdentifier;
    LineEdit fieldName;

    UIIconBrowser iconBrowser;
    StructureDataInfo currentData;

    List<ResourceRequirement> mutableCost;
    List<StructureHarvestInfo> mutableHarvest;

    public override void _Ready()
    {
        // Get
        listCost = GetNode<UIEditParamsListView>("%CostList");
        listHarvest = GetNode<UIEditParamsListView>("%HarvestList");

        iconView = GetNode<UIImageWell>("%Icon");

        fieldIdentifier = GetNode<LineEdit>("%Identifier");
        fieldName = GetNode<LineEdit>("%Name");

        iconBrowser = GetNode<UIIconBrowser>("%IconBrowser");

        // Init
        listCost.DataProvider = this;
        listHarvest.DataProvider = this;

        // Bind
        fieldIdentifier.TextSubmitted += OnIdentifierChanged;
        fieldName.TextSubmitted += OnNameChanged;

        iconBrowser.PopupHide += OnIconSelectionAborted;
        iconBrowser.OnModalStarted += OnModalStarted;
        iconBrowser.OnIconSelected += OnIconSelected;
    }

    public override async void Configure(Resource data, int index, IEditorContextWriter writer)
    {
        base.Configure(data, index, writer);

        currentData = (StructureDataInfo) data;

        mutableCost = new(currentData.Cost);
        mutableHarvest = new(currentData.HarvestInfo);

        await GUtils.WaitOneFrame(this);

        listCost.Update();
        listHarvest.Update();

        fieldIdentifier.Text = currentData.Identifier;
        fieldName.Text = currentData.DisplayName;

        iconView.SetImage(currentData.Icon);
    }

    public override void Finalise()
    {
        currentData.Cost = mutableCost.ToArray();
        currentData.HarvestInfo = mutableHarvest.ToArray();

        writer.EditorPerformWrite(index, currentData);
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
        currentData.Icon = icon;
        Finalise();
    }

    void OnIdentifierChanged(string value)
    {
        fieldIdentifier.ReleaseFocus();
        currentData.Identifier = value;
        Finalise();
    }

    void OnNameChanged(string value)
    {
        fieldName.ReleaseFocus();
        currentData.DisplayName = value;
        Finalise();
    }

    /// Data Provider ///

    public int DataProviderGetInitialCount(UIEditParamsListView listView)
    {
        if (listView == listCost)
        {
            return mutableCost.Count;
        }
        else if (listView == listHarvest)
        {
            return mutableHarvest.Count;
        }

        return 0;
    }

    public void DataProviderConfigureCell(UIEditParamsListView listView, BaseEditParamsCell cell, int index)
    {
        if (listView == listCost)
        {
            UICellCost costCell = (UICellCost) cell;
            costCell.Sync(mutableCost[index]);
        }
        else if (listView == listHarvest)
        {
            UICellHarvest harvestCell = (UICellHarvest) cell;
            harvestCell.Sync(mutableHarvest[index]);
        }
    }

    public void DataProviderRequestAdd(UIEditParamsListView listView)
    {
        if (listView == listCost)
        {
            ResourceRequirement requirement = new()
            {
                Type = ResourceType.Wood,
                Amount = 1
            };

            mutableCost.Add(requirement);
        }
        else if (listView == listHarvest)
        {
            StructureHarvestInfo harvest = new()
            {
                HarvestType = ResourceType.Wood,
                HarvestRate = 0.01f,
                HarvestAmount = 1f
            };

            mutableHarvest.Add(harvest);
        }

        listView.Update();
    }

    public void DataProviderRequestRemove(UIEditParamsListView listView, int index)
    {
        if (listView == listCost)
        {
            mutableCost.RemoveAt(index);
        }
        else if (listView == listHarvest)
        {
            mutableHarvest.RemoveAt(index);
        }

        listView.Update();
    }

    public void DataProviderRequestContextWrite<[MustBeVariant] DataType>(
        UIEditParamsListView listView,
        int index,
        DataType data)
    {
        if (data is ResourceRequirement requirement)
        {
            mutableCost[index] = requirement;
        }
        else if (data is StructureHarvestInfo info)
        {
            mutableHarvest[index] = info;
        }
    }
}
