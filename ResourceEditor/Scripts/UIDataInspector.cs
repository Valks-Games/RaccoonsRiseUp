namespace RRU.ResourceEditor;

public sealed partial class UIDataInspector : Control
{
    [Export] PackedScene pkgStructureInspector;
    [Export] PackedScene pkgTechUpgradeInspector;
    [Export] PackedScene pkgJobDataInspector;

    Control contentsView;

    public override void _Ready()
    {
        contentsView = GetNode<Control>("%Contents");
    }

    public void SetContents(Resource data, int index, IEditorContextWriter writer)
    {
        for (int i = contentsView.GetChildCount(); i --> 0;)
        {
            if (contentsView.GetChild(i) is not BaseInspectorView inspector)
                continue;

            inspector.Finalise();
            inspector.QueueFree();
        }

        if (!IsInstanceValid(data))
            return;

        if (data is StructureDataInfo structure)
        {
            ConfigureInspector(pkgStructureInspector, structure, index, writer);
        }
        else if (data is TechUpgradeInfo techUpgrade)
        {
            ConfigureInspector(pkgTechUpgradeInspector, techUpgrade, index, writer);
        }
        else if (data is JobData jobData)
        {
            ConfigureInspector(pkgJobDataInspector, jobData, index, writer);
        }
    }

    void ConfigureInspector(PackedScene template, Resource data, int index, IEditorContextWriter writer)
    {
        BaseInspectorView inspector = template.Instantiate<BaseInspectorView>();
        contentsView.AddChild(inspector);

        inspector.Configure(data, index, writer);
    }
}