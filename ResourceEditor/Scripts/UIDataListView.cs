namespace RRU.ResourceEditor;

public sealed partial class UIDataListView : Control
{
    public event Action<int> OnIndexSelected;

    public IDataListViewDataProvider DataProvider;

    Label labelTitle;
    ItemList listView;
    Button buttonAdd;
    Button buttonRemove;

    int lastSelectedIdx = -1;

    public async override void _Ready()
    {
        // Get
        labelTitle = GetNode<Label>("%Title");
        listView = GetNode<ItemList>("%Data");
        buttonAdd = GetNode<Button>("%Add");
        buttonRemove = GetNode<Button>("%Remove");

        // Bind
        buttonAdd.Pressed += OnAddItem;
        buttonRemove.Pressed += OnRemoveItem;

        listView.ItemSelected += OnItemSelected;
    }

    void OnItemSelected(long idx)
    {
        if (idx < 0 || idx > DataProvider.DataProviderGetCount(this))
            return;

        lastSelectedIdx = (int) idx;
        OnIndexSelected?.Invoke(lastSelectedIdx);
    }

    /// List Interface ///

    public int GetSelectedIndex()
    {
        return lastSelectedIdx;
    }

    public void Update()
    {
        listView.Clear();

        for (int i = 0, l = DataProvider.DataProviderGetCount(this); i < l; ++i)
        {
            ListItemInfo info = DataProvider.DataProviderGetItem(this, i);
            listView.AddItem(info.Title, info.Icon);
        }
    }

    /// Events ///

    void OnAddItem()
    {
        DataProvider.DataProviderCreateNewItem(this);
        Update();
    }

    void OnRemoveItem()
    {
        if (lastSelectedIdx == -1)
            return;

        DataProvider.DataProviderRemoveItem(this, lastSelectedIdx);
        lastSelectedIdx = -1;
        Update();
    }
}

public interface IDataListViewDataProvider
{
    int DataProviderGetCount(UIDataListView listView);
    ListItemInfo DataProviderGetItem(UIDataListView listView, int index);

    void DataProviderCreateNewItem(UIDataListView listView);
    void DataProviderRemoveItem(UIDataListView listView, int index);
}

public ref struct ListItemInfo
{
    public readonly string Title;
    public readonly Texture2D Icon;

    public ListItemInfo(string title, Texture2D icon = null)
    {
        Title = title;
        Icon = icon;
    }
}
