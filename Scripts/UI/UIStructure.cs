namespace RRU;

public partial class UIStructure : Node
{
    // 'int cost' needs to be replaced with an array of resources required
    // to purchase this structure. For example a structure could cost
    // 50 tech points, 150 wood and 200 stone
    int cost;
    string name;
    string prevLineEditCountText;
    LineEdit lineEditCount;

    public override void _Ready()
    {
        lineEditCount = GetNode<LineEdit>("%Count");
        lineEditCount.TextChanged += EnforceOnlyNumbersOnCountLineEdit;
        GetNode<Button>("%Purchase").Pressed += HandlePurchase;
    }

    public UIStructure UpdateName(string name)
    {
        this.name = name;
        GetNode<Label>("%Name").Text = name;
        return this;
    }

    public UIStructure UpdateCost(int cost)
    {
        this.cost = cost;
        GetNode<Label>("%Cost").Text = cost.ToString();
        return this;
    }

    // The param should not be of type 'TechType' but rather 'StructureType'
    // but then this means that techs and structures both share the same image
    // data in common. The code should be refactored around this somehow.
    public UIStructure UpdateIcon(TechType techType)
    {
        GetNode<TextureRect>("%Icon").Texture = 
            Game.TechData[techType].GetImage();

        return this;
    }

    void HandlePurchase()
    {
        if (!int.TryParse(lineEditCount.Text, out int amount))
            return;

        GD.Print($"Purchased {amount} {name} for {cost * amount} wood");
    }

    void EnforceOnlyNumbersOnCountLineEdit(string text)
    {
        if (!text.IsDigitsOnly())
        {
            lineEditCount.Text = prevLineEditCountText;
            return;
        }

        prevLineEditCountText = text;
    }
}
