namespace RRU;

[Obsolete]
public class TechData
{
    public string ImagePath { get; set; }
    public string Description { get; set; }

    public Texture2D GetImage() =>
        ResourceLoader.Load<Texture2D>($"res://Sprites/Icons/{ImagePath}.svg");
}

[Obsolete]
public enum TechType
{
    WoodEffeciency,
    ResearchEffeciency
}
