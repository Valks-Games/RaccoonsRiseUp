namespace RRU.ResourceEditor;

public sealed partial class UIEditParamsListView : VBoxContainer, IEditorContextWriter
{
    [Export] BaseEditParamsCell template;

    Control contentsView;
    Button btnAdd;

    public IEditParamsListViewSyncDataProvider DataProvider;

    public override void _Ready()
    {
        // Get
        contentsView = GetNode<Control>("%ContentView");
        btnAdd = GetNode<Button>("%Add");

        // Bind
        btnAdd.Pressed += OnAddPressed;

        // Init
        template.Visible = false;
    }

    /// Events ///

    void OnAddPressed()
    {
        DataProvider.DataProviderRequestAdd(this);
    }

    /// Public Methods ///

    public void Update()
    {
        Clear();

        for (int i = 0, l = DataProvider.DataProviderGetInitialCount(this); i < l; ++ i)
        {
            BaseEditParamsCell cell = (BaseEditParamsCell) template.Duplicate();

            cell.SetMeta(BaseEditParamsCell.KeyIndex, i);
            cell.SetOwner(this);

            cell.Initialise();
            cell.Visible = true;

            contentsView.AddChild(cell);

            DataProvider.DataProviderConfigureCell(this, cell, i);
        }
    }

    public void Clear()
    {
        for (int i = contentsView.GetChildCount(); i --> 0;)
        {
            Control cell = contentsView.GetChild<Control>(i);

            if (!cell.Visible)
                continue;

            cell.QueueFree();
        }
    }

    /// Helpers ///

    /// <summary>
    /// Must only be called by BaseEditParamsCell.
    /// </summary>
    public void RequestRemove(int i)
    {
        DataProvider.DataProviderRequestRemove(this, i);
    }

    /// Context Writer ///

    public void EditorPerformWrite<[MustBeVariant] T>(int index, T data)
    {
        DataProvider.DataProviderRequestContextWrite(this, index, data);
    }
}

public interface IEditParamsListViewSyncDataProvider
{
    int DataProviderGetInitialCount(UIEditParamsListView listView);
    void DataProviderConfigureCell(UIEditParamsListView listView, BaseEditParamsCell cell, int index);
    void DataProviderRequestAdd(UIEditParamsListView listView);
    void DataProviderRequestRemove(UIEditParamsListView listView, int index);
    void DataProviderRequestContextWrite<[MustBeVariant] T>(UIEditParamsListView listView, int index, T data);
}
