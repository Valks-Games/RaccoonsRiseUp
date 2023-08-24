namespace RRU;

public class TechNodeClickedInfo
{
    public Vector2 Position { get; set; }
    public TechInfo TechInfo { get; set; }
}

public partial class UITechNode : Control
{
    static Color RESEARCHED_COLOUR => new(0.3f, 1.0f, 0.3f, 0.5f);
    static Color LOCKED_COLOUR => new(1.0f, 0.3f, 0.3f, 0.35f);

    public event Action<TechNodeClickedInfo> ClickedOnNode;

    const int DESCRIPTION_FONT_SIZE = 32;
    const int DESCRIPTION_OFFSET = 125;

    public bool IsActive { get; set; }

    TextureRect textureRect;
    TechNodeState nodeState;
    GTween tweenScale;
    TechInfo info;

    public override void _Ready()
    {
        nodeState = TechNodeState.Locked;
        textureRect = GetNode<TextureRect>("%TextureRect");
        PivotOffset += Size / 2;

        MouseEntered += OnHoverEnter;
        MouseExited += OnHoverExit;
        GuiInput += OnGuiInput;
    }

    public void Setup(TechInfo info)
    {
        this.info = info;
        textureRect.Texture = info.Data.GetImage();
    }

    public void SetResearchState(TechNodeState state)
    {
        nodeState = state;

        switch (state)
        {
            case TechNodeState.Locked:
                textureRect.Modulate = LOCKED_COLOUR;
                break;

            case TechNodeState.Unlocked:
                textureRect.Modulate = Colors.White;
                break;

            case TechNodeState.Researched:
                textureRect.Modulate = RESEARCHED_COLOUR;
                break;
        }
    }

    public void Deactivate()
    {
        ZIndex = 0;

        AnimateScale(1,
            zindex: 0,
            duration: 0.2);
    }

    void AnimateScale(float scale, int zindex, double duration = 0.1)
    {
        ZIndex = zindex;

        tweenScale = new GTween(this);
        tweenScale.Animate("scale", Vector2.One * scale, duration)
            .SetTrans(Tween.TransitionType.Sine);
    }

    /// Signal Handlers ///

    public void OnResearchStateChanged(TechDataService service, StringName id, bool isResearched)
    {
        if (nodeState == TechNodeState.Researched)
            return;

        bool isUnlocked = service.IsUnlocked(info.Id);

        SetResearchState(
            isUnlocked ? TechNodeState.Unlocked : TechNodeState.Locked);

        if (info.Id != id || !isResearched)
            return;

        SetResearchState(TechNodeState.Researched);
    }

    void OnHoverEnter()
    {
        if (UITech.TechNodeActive)
            return;

        AnimateScale(1.05f,
            zindex: 100,
            duration: 0.1);
    }

    void OnHoverExit()
    {
        if (UITech.TechNodeActive)
            return;

        AnimateScale(1,
            zindex: 0,
            duration: 0.1);
    }

    void OnGuiInput(InputEvent @event)
    {
        if (IsActive ||
            @event is not InputEventMouseButton mouse ||
            UITech.TechNodeActive ||
            !mouse.IsLeftClickPressed())
        {
            return;
        }

        IsActive = true;
        UITech.TechNodeActive = true;

        AnimateScale(
            scale: 1.5f,
            zindex: 100,
            duration: 0.2
        );

        ClickedOnNode?.Invoke(new TechNodeClickedInfo
        {
            Position = Position + Size / 2,
            TechInfo = info
        });

        GetViewport().SetInputAsHandled();
    }
}

public enum TechNodeState
{
    Locked,
    Unlocked,
    Researched
}
