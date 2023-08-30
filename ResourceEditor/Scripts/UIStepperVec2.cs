namespace RRU.ResourceEditor;

public sealed partial class UIStepperVec2 : Control
{
    public event Action<Vector2I> OnValueChanged;

    UIStepperControl fieldX;
    UIStepperControl fieldY;

    Vector2I innerValue;

    public override void _Ready()
    {
        // Get
        fieldX = GetNode<UIStepperControl>("%PosX");
        fieldY = GetNode<UIStepperControl>("%PosY");

        // Init
        BroadcastAndSync(true);

        // Bind
        fieldX.OnValueChanged += OnPositionXChanged;
        fieldY.OnValueChanged += OnPositionYChanged;
    }

    /// Events ///

    void OnPositionXChanged(float value)
    {
        innerValue.X = Mathf.RoundToInt(value);
        BroadcastAndSync();
    }

    void OnPositionYChanged(float value)
    {
        innerValue.Y = Mathf.RoundToInt(value);
        BroadcastAndSync();
    }

    /// Setter/Getter ///

    public Vector2I Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public void SetX(int value)
    {
        OnPositionXChanged(value);
    }

    public void SetY(int value)
    {
        OnPositionYChanged(value);
    }

    public void SetValue(Vector2I value, bool silenceEvent = false)
    {
        innerValue = value;
        BroadcastAndSync(silenceEvent);
    }

    public Vector2I GetValue()
    {
        return innerValue;
    }

    /// Helpers ///

    void BroadcastAndSync(bool silenceEvent = false)
    {
        if (!silenceEvent)
        {
            OnValueChanged?.Invoke(innerValue);
        }

        fieldX.SetValue(innerValue.X, true);
        fieldY.SetValue(innerValue.Y, true);
    }
}
