namespace RRU;

public partial class Game
{
    public static Dictionary<TechType, TechData> TechData { get; } = new()
    {
        {
            TechType.WoodEffeciency, new TechData
            {
                ImagePath = "wood-beam",
                Description = "Increase wood production"
            }
        },
        {
            TechType.ResearchEffeciency, new TechData
            {
                ImagePath = "archive-research",
                Description = "Increase research production"
            }
        }
    };
}

public class TechData
{
    public string ImagePath { get; set; }
    public string Description { get; set; }

    public Texture2D GetImage() =>
        ResourceLoader.Load<Texture2D>($"res://Sprites/Icons/{ImagePath}.svg");
}

public enum TechType
{
    WoodEffeciency,
    ResearchEffeciency
}
