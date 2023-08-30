namespace RRU.ResourceEditor;

[GlobalClass]
public abstract partial class BaseInspectorView : GridContainer
{
    protected IEditorContextWriter writer;
    protected int index;

    public virtual void Configure(Resource data, int index, IEditorContextWriter writer)
    {
        this.index = index;
        this.writer = writer;
    }

    public abstract void Finalise();
}

public interface IEditorContextWriter
{
    void EditorPerformWrite<[MustBeVariant] T>(int index, T data);
}