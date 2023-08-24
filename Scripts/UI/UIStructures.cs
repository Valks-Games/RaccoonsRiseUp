namespace RRU;

public partial class UIStructures : Node
{
    [Export] GameState gameState;
    [Export] StructureDataService dataService;

    GridContainer gridContainer;

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("GridContainer");

        ReadOnlySpan<StructureDataInfo> structureData = default;
        dataService.GetStructures(ref structureData);

        for (int i = 0; i < structureData.Length; ++i)
        {
            AddStructure(structureData[i]);
        }

        gameState.ResourcesChanged += OnResourcesChanged;
    }

    public override void _Notification(int what)
    {
        if (what != GodotObject.NotificationPredelete)
            return;

        gameState.ResourcesChanged -= OnResourcesChanged;
    }

    public void OnResourcesChanged(ResourceDict _)
    {
        ReadOnlySpan<StructureDataInfo> data = default;
        dataService.GetStructures(ref data);

        for (int i = 0, l = gridContainer.GetChildCount(); i < l; ++i)
        {
            var structure = gridContainer.GetChildOrNull<UIStructure>(i);
            structure?.Update(data[i]);
        }
    }

    void AddStructure(StructureDataInfo info)
    {
        var structure = Prefabs.Structure.Instantiate<UIStructure>();
        gridContainer.AddChild(structure);

        structure.Configure(info);
    }
}
