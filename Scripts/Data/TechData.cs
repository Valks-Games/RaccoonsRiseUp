namespace RRU;

public partial class Game
{
    public static Dictionary<TechType, TechData> TechData { get; private set; }
}

public class TechData
{
    public Texture2D Image { get; set; }
    public string Description { get; set; }
}

public enum TechType
{
    WoodEffeciency,
    ResearchEffeciency
}
