namespace RRU.ResourceEditor;

[GlobalClass]
public abstract partial class BaseEditParamsCell : Control
{
    public static StringName KeyIndex = "index";

    protected UIEditParamsListView listView;

    public sealed override void _Ready()
    {
        Button btnDelete = GetNodeOrNull<Button>("%Delete");

        if (btnDelete == null)
            return;

        btnDelete.Pressed += Delete;
    }

    /// <summary>
    /// This is called when the list view re-creates the template cell.
    /// Perform initialisation tasks here (like you'd do on '_Ready')
    /// </summary>
    public abstract void Initialise();

    /// <summary>
    /// Make a call to 'EditorPerformWrite' to finalise the write operation.
    /// </summary>
    public abstract void UpdateSource();

    /// Helpers ///

    /// <summary>
    /// Must only be called by UIEditParamsListView
    /// </summary>
    public void SetOwner(UIEditParamsListView listView)
    {
        this.listView = listView;
    }

    public void Delete()
    {
        listView.RequestRemove(GetIndex());
    }

    public int GetIndex()
    {
        return (int) GetMeta(KeyIndex);
    }
}