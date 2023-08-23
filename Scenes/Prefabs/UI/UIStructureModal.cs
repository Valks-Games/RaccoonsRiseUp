namespace RRU;

public partial class UIStructureModal : Panel
{
    static StringName PropPanel
        => "theme_override_styles/panel";

    /// args: 0 => success
    public event Action<bool> OnClosed;
    static event Action<UIStructureModal> OnRequestExclusive;

    Label labelTitle;

    Button buttonConfirm;
    Button buttonAbort;

    StyleBoxFlat stylebox;
    Color activeColour;
    Color inactiveColour;
    Tween tween;

    bool lastVisible;

    public override void _Ready()
    {
        // Init //

        Visible = false;

        // Stylebox config
        stylebox = (StyleBoxFlat) Get(PropPanel);
        stylebox = (StyleBoxFlat) stylebox.Duplicate();

        activeColour = stylebox.BgColor;

        inactiveColour = stylebox.BgColor;
        inactiveColour.A = 0.0f;

        Set(PropPanel, stylebox);

        // Get //

        labelTitle = GetNode<Label>("%Title");

        buttonConfirm = GetNode<Button>("%Confirm");
        buttonAbort = GetNode<Button>("%Abort");

        // Bind //

        buttonConfirm.Pressed += () => OnClose(true);
        buttonAbort.Pressed += () => OnClose(false);

        OnRequestExclusive += OnExclusiveRequest;
    }

    public override void _Notification(int what)
    {
        if (what != GodotObject.NotificationPredelete)
            return;

        OnRequestExclusive -= OnExclusiveRequest;
    }

    /// Events ///

    void OnExclusiveRequest(UIStructureModal exclusiveModalRef)
    {
        if (this == exclusiveModalRef)
            return;

        OnClose(false);
    }

    void OnClose(bool confirm)
    {
        SetVisible(false);
        OnClosed?.Invoke(confirm);
    }

    /// Modal ///

    public void SetTitle(string text)
    {
        labelTitle.Text = text;
    }

    public void SetVisible(bool visible)
    {
        if (IsInstanceValid(tween) && tween.IsRunning())
        {
            tween.Kill();
        }

        if (visible == lastVisible)
            return;

        tween = CreateTween();
        lastVisible = visible;

        if (visible)
        {
            OnRequestExclusive?.Invoke(this);
            Visible = true;
        }

        tween.TweenProperty(
            @object: stylebox,
            property: StyleBoxFlat.PropertyName.BgColor.ToString(),
            finalVal: visible ? activeColour : inactiveColour,
            duration: 0.5f
        );

        if (!visible)
        {
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        Tween viewTween = CreateTween();
        AnimateView(visible, ref viewTween);
    }

    /// Animation ///

    void AnimateView(bool visible, ref Tween tween)
    {
        AnimateModulate(visible, labelTitle, 0.0f, ref tween);
        AnimateModulate(visible, target: buttonConfirm, 0.025f, ref tween);
        AnimateModulate(visible, buttonAbort, 0.025f, ref tween);
    }

    void AnimateModulate(bool visible, Control target, float delay, ref Tween tween)
    {
        target.Modulate = visible ? Colors.Transparent : Colors.White;

        tween.TweenInterval(delay);

        tween.Parallel().TweenProperty(
            @object: target,
            property: CanvasItem.PropertyName.Modulate.ToString(),
            finalVal: visible ? Colors.White : Colors.Transparent,
            duration: visible ? 0.25f : 0.08f
        );
    }
}
