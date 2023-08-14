namespace RRU;

public partial class World : SubViewport
{
    [Export] GameState gameState;

    FastNoiseLite fnlGrass = new()
    {
        Frequency = 0.05f
    };
    FastNoiseLite fnlVegetation = new()
    {
        Frequency = 0.05f,
        Offset = new Vector3(1000, 0, 0)
    };
    TileMap tileMapGrass;
    TileMap tileMapLayer2;
    Label clock;
    CanvasModulate canvasModulate;
    Dictionary<string, Vector2I[]> tiles = new()
    {
        { 
            "grass", new Vector2I[]
            {
                new Vector2I(3, 1),
                new Vector2I(0, 8),
                new Vector2I(1, 8),
                new Vector2I(4, 1)
            }
        },
        {
            "vegetation", new Vector2I[]
            {
                //new Vector2I(6, 3), // pine tree
                new Vector2I(6, 4), // oak tree 1
                //new Vector2I(4, 9), // oak tree 2
                //new Vector2I(5, 9), // oak tree 3
                //new Vector2I(4, 2)  // bush
            }
        },
        {
            "settlement", new Vector2I[]
            {
                new Vector2I(7, 5)
            }
        }
    };

    public override void _Ready()
    {
        clock = GetNode<Label>("%Clock");
        canvasModulate = GetNode<CanvasModulate>("CanvasModulate");
        tileMapGrass = GetNode<TileMap>("Grass");
        tileMapLayer2 = GetNode<TileMap>("Layer2");

        var size = 50;

        for (int x = -size; x < size; x++)
        {
            for (int y = -size; y < size; y++)
            {
                SetupTile(x, y);
            }
        }
    }

    bool isDay = true;

    public override void _PhysicsProcess(double delta)
    {
        // Multiply by 50 to speed up in-game time for testing
        TimeSpan diff = (DateTime.Now - gameState.StartOfGame) * 50;
        double seconds = diff.TotalSeconds;

        byte white = isDay ?
            (byte)(seconds % 255) : (byte)(255 - seconds % 255);

        if (isDay && white == 254)
            isDay = !isDay;

        // 254
        // 0

        GD.Print(white);

        UpdateClock(diff);

        canvasModulate.Color = Color.Color8(white, white, white, 255);
    }

    void UpdateClock(TimeSpan diff)
    {
        var days = LeadingZero(diff.Days);
        var hours = LeadingZero(diff.Hours);
        var minutes = LeadingZero(diff.Minutes);
        var seconds = LeadingZero(diff.Seconds);

        clock.Text = $"{days}:{hours}:{minutes}:{seconds}";
    }

    string LeadingZero(int n) => n.ToString().PadLeft(2, '0');

    void SetupTile(int x, int y)
    {
        SetupLayer1(x, y);
        SetupLayer2(x, y);
    }

    void SetupLayer1(int x, int y)
    {
        // Noise values range between [-1, 1]
        var noise = fnlGrass.GetNoise2D(x, y);

        var index = (int)noise.Remap(-1, 1, 0, tiles["grass"].Length - 1);

        SetTile(x, y, tiles["grass"][index], tileMapGrass, 2);
    }

    void SetupLayer2(int x, int y)
    {
        // Noise values range between [-1, 1]
        var noise = fnlVegetation.GetNoise2D(x, y);

        var index = (int)noise.Remap(-1, 1, 0, tiles["vegetation"].Length - 1);

        // Will need to come up with a better way of doing this
        // Ideally would be nice to say I want 90% of tiles to be clear
        // and 10% to be a random tree. This should be incoperated into
        // noise instead of using GD.Randf()
        if (GD.Randf() < 0.9f)
            // Clear tile
            SetTile(x, y, Vector2I.One * -1, tileMapLayer2, -1);
        else
            // Set tile
            SetTile(x, y, tiles["vegetation"][index], tileMapLayer2, 0);

        // Set the player settlement tile
        SetTile(0, 0, tiles["settlement"][0], tileMapLayer2, 0);
    }

    void SetTile(int x, int y, Vector2I tile, TileMap tileMap, int sourceId) =>
        tileMap.SetCell(
            layer: 0, 
            coords: new Vector2I(x, y), 
            sourceId: sourceId,
            atlasCoords: tile);
}
