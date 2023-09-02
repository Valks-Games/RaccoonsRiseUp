namespace RRU.ResourceEditor;

public sealed partial class UIStepperControl : Control
{
    public event Action<float> OnValueChanged;

    [Export] public string PrefixString = null;
    [Export] public float MinValue = 0.0f;
    [Export] public float MaxValue = float.MaxValue;
    [Export] public float StepSize = 1.0f;

    LineEdit numberField;
    Button btnIncrement;
    Button btnDecrement;

    float innerValue;

    public override void _Ready()
    {
        // Get
        numberField = GetNode<LineEdit>("%Value");
        btnIncrement = GetNode<Button>("%Increase");
        btnDecrement = GetNode<Button>("%Decrease");

        // init
        SetValue(MinValue, true);

        // Bind
        numberField.FocusExited += OnFocusLost;
        numberField.TextSubmitted += OnNumberChangedRequest;
        btnIncrement.Pressed += OnIncrementPressed;
        btnDecrement.Pressed += OnDecrementPressed;
    }

    /// Events ///

    void OnFocusLost()
    {
        OnNumberChangedRequest(numberField.Text);
    }

    void OnIncrementPressed()
    {
        SetValue(innerValue + StepSize);
    }

    void OnDecrementPressed()
    {
        SetValue(innerValue - StepSize);
    }

    void OnNumberChangedRequest(string text)
    {
        // Trim to start of numerical input
        ReadOnlySpan<char> inputText = text;

        for (int i = 0; i < inputText.Length; ++ i)
        {
            if (!char.IsDigit(inputText[i]))
                continue;

            inputText = inputText[i..];
            break;
        }

        numberField.ReleaseFocus();

        if (!float.TryParse(inputText, out float floatValue))
        {
            SyncNumberFieldToSource();
            return;
        }

        SetValue(floatValue);
    }

    /// Setter/Getter ///

    /// <summary>
    /// Shorthand for getter/setter
    /// </summary>
    public float Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public void SetValue(float value, bool silenceEvent = false)
    {
        innerValue = Mathf.Clamp(value, MinValue, MaxValue);

        btnIncrement.Disabled = value >= MaxValue;
        btnDecrement.Disabled = value <= MinValue;

        SyncNumberFieldToSource();

        if (silenceEvent)
            return;

        OnValueChanged?.Invoke(innerValue);
    }

    public float GetValue()
    {
        return innerValue;
    }

    /// Helpers ///

    void SyncNumberFieldToSource()
    {
        if (PrefixString == null || PrefixString.Length < 1)
        {
            numberField.Text = innerValue.ToString();
            return;
        }

        numberField.Text = $"{PrefixString} {innerValue}";
    }
}