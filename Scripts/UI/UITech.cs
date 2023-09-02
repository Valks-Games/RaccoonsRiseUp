namespace RRU;

public partial class UITech : SubViewport
{
    public static bool TechNodeActive { get; set; }

    [Export] int techNodeSpacing;

    [Export] TechDataService techData;
    [Export] TechNodeDetails detailsView;

    Camera2D camera;
    ColorRect overlay; // transparent overlay
    GTween tweenOverlayColor;
    GTween tweenCamera;

    public override void _Ready()
    {
        camera = GetNode<Camera2D>("Camera2D");

        CreateTransparentOverlay();
        AddTechNodes();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
        {
            if (!TechNodeActive)
                return;

            if (key.Keycode == Key.Escape)
            {
                UITechNode activeTechNode = FindActiveTechNode();
                DeactivateTechNode(activeTechNode);
            }
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (!TechNodeActive || mouse.IsLeftClickReleased())
                return;

            UITechNode activeTechNode = FindActiveTechNode();
            Vector2 globalMousePos = activeTechNode.GetGlobalMousePosition();

            if (!activeTechNode.GetRect().HasPoint(globalMousePos))
            {
                // Clicked outside of active tech node
                DeactivateTechNode(activeTechNode);
            }
        }
    }

    List<UITechNode> techNodes = new();

    UITechNode FindActiveTechNode()
    {
        foreach (var techNode in techNodes)
            if (techNode.IsActive)
                return techNode;

        return null;
    }

    void AddTechNodes()
    {
        ReadOnlySpan<TechUpgradeInfo> upgrades = default;
        techData.GetAllUpgrades(ref upgrades);

        for (int i = 0; i < upgrades.Length; ++i)
        {
            AddTech(upgrades[i]);
        }
    }

    void CreateTransparentOverlay()
    {
        overlay = new ColorRect();
        overlay.ZIndex = 10;

        // I am using a very large size because changing the size dynamically
        // with the camera zoom was not working out for me
        // Also adding the overlay to a CanvasLayer did not work for me because
        // apparently ZIndex does not hop over to things not in the CanvasLayer
        overlay.Size = DisplayServer.WindowGetSize() * 10;

        overlay.Color = new Color(0, 0, 0, 0);
        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        camera.AddChild(overlay);
        overlay.Position -= overlay.Size / 2;
    }

    void DeactivateTechNode(UITechNode activeTechNode)
    {
        activeTechNode.IsActive = false;
        activeTechNode.Deactivate();

        TechNodeActive = false;

        tweenOverlayColor = new GTween(overlay);
        tweenOverlayColor.Animate("color", new Color(0, 0, 0, 0), 0.2);

        HideDetails();
    }

    void AddTech(TechUpgradeInfo info)
    {
        UITechNode techNode = Prefabs.TechNode.Instantiate<UITechNode>();
        AddChild(techNode);

        techNode.Setup(info);

        techNode.OnClicked += (TechUpgradeInfo upgradeInfo) =>
            OnTechNodeClicked(techNode, upgradeInfo);

        techNodes.Add(techNode);

        // Open the details view when a tech node has been activated
        techNode.OnClicked += detailsView.SetUpgradeInfo;
        techData.ResearchStateUpdated += techNode.OnResearchStateChanged;

        // Must do this after AddChild(...) otherwise techNode.Size will
        // not be accurate
        var offset = techNode.Size / 2;
        var spacing = Vector2.One * techNodeSpacing;

        techNode.Position =
            info.Position * (techNode.Size + spacing) - offset;

        // Set node state
        TechNodeState nodeState = techData.IsResearched(info.Id) ?
            TechNodeState.Researched : TechNodeState.Locked;

        if (nodeState == TechNodeState.Locked && techData.IsUnlocked(info.Id))
            nodeState = TechNodeState.Unlocked;

        techNode.SetResearchState(nodeState);
    }

    void OnTechNodeClicked(UITechNode techNode, TechUpgradeInfo techInfo)
    {
        // Camera moves to the tech node that was clicked
        tweenCamera = new GTween(camera);

        Vector2 positionF = techInfo.Position;

        tweenCamera.Animate(
            prop: Control.PropertyName.Position.ToString(),
            finalValue: positionF,
            duration: 0.5f)
        .SetTrans(Tween.TransitionType.Sine);

        // Animate overlay to a nice dark transparent background
        tweenOverlayColor = new GTween(overlay);
        tweenOverlayColor.Animate("color", new Color(0, 0, 0, 0.8f), 0.2);
    }

    async void HideDetails()
    {
        // The tech description will blink without waiting for one frame when
        // switching between tech nodes. An alternative to waiting for one frame
        // would be to use CallDeferred(...) on HideDetails()
        await GUtils.WaitOneFrame(this);

        // No need to hide the details view if the user only switches context
        if (IsInstanceValid(FindActiveTechNode()))
            return;

        detailsView.OnHideRequested();
    }
}
