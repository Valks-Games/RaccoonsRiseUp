namespace RRU;

/// <summary>
/// A TechInfo represents an instance of TechData with its own modifiers
/// </summary>
[Obsolete]
public sealed partial class TechInfo
{
    public StringName Id { get; private set; }
    public float Modifier { get; private set; }

    public TechData Data { get; private set; }
    public TechType Type { get; private set; }

    public static TechInfo FromType(StringName id, TechType type)
    {
        return null;
    }
}
