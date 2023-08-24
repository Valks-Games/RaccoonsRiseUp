namespace RRU;

public partial class UIStructure : Node
{
    /// <summary>
    /// When selling, how much of the resource amount spent should be refunded?
    /// </summary>
    const double REFUND_FACTOR = 0.3;

    static NodePath pathModal => "%Modal";

    [Export] GameState gameState;

    StructureDataInfo info;

    Control requirements;

    TextureRect iconView;
    Label labelName;
    Label labelCount;
    SpinBox countField;

    Button btnBuy;
    Button btnSell;

    /// <summary>
    /// This should be bound to a preference item so that the player can
    /// toggle this behaviour by themself.
    /// </summary>
    bool configModalConfirm = true;

    bool buyMode;

    public override void _Ready()
    {
        // Init
        buyMode = false;

        // Get
        requirements = GetNode<Control>("%Requirements");

        labelName = GetNode<Label>("%Name");
        labelCount = GetNode<Label>("%Count");
        countField = GetNode<SpinBox>("%ActionCount");

        iconView = GetNode<TextureRect>("%Icon");

        btnBuy = GetNode<Button>("%Buy");
        btnSell = GetNode<Button>("%Sell");

        UIStructureModal modal = GetNode<UIStructureModal>(pathModal);

        // Bind
        gameState.StructuresChanged += OnStructuresUpdated;

        modal.OnClosed += OnModalClosed;

        btnBuy.Pressed += OnBuyPressed;
        btnSell.Pressed += OnSellPressed;

        countField.GetLineEdit().TextSubmitted += OnActionCountSubmitted;
    }

    public override void _Notification(int what)
    {
        if (what != GodotObject.NotificationPredelete)
            return;

        gameState.StructuresChanged -= OnStructuresUpdated;
    }

    /// Event Handlers ///

    void OnStructuresUpdated(StructureDict _)
    {
        labelCount.Text = 
            $"(In Storage: {gameState.GetStructureCount(info.Identifier)})";
    }

    void OnActionCountSubmitted(string _)
    {
        ReleaseCountFieldFocus();
    }

    void OnBuyPressed()
    {
        ReleaseCountFieldFocus();

        if (configModalConfirm)
        {
            ShowModal(true);
            return;
        }

        PerformPurchase(Mathf.RoundToInt(countField.Value));
    }

    void OnSellPressed()
    {
        ReleaseCountFieldFocus();

        if (configModalConfirm)
        {
            ShowModal(false);
            return;
        }

        PerformSell(Mathf.RoundToInt(countField.Value));
    }

    void OnModalClosed(bool confirmAction)
    {
        if (!confirmAction)
            return;

        int amount = Mathf.RoundToInt(countField.Value);

        if (buyMode)
        {
            PerformPurchase(amount);
            return;
        }

        PerformSell(amount);
    }

    /// Helpers ///

    void ReleaseCountFieldFocus()
    {
        countField.GetLineEdit().ReleaseFocus();
    }

    /// <summary>
    /// Must only be called during the 'setup' phase.
    /// Since the template node is destroyed after the initial call,
    /// subsequent attempts to 'configure' the node will result
    /// in a nil exception.
    /// </summary>
    public void Configure(StructureDataInfo info)
    {
        this.info = info;
        Node template = GetNode("%RequirementCell");

        // Basic details
        labelName.Text = info.DisplayName;
        iconView.Texture = info.Icon;

        // Requirements
        int count = gameState.GetStructureCount(info.Identifier);

        Span<MaterialCostInfo> materialCost = 
            stackalloc MaterialCostInfo[info.Cost.Length];

        GetUpdatedCost(count, 1, ref materialCost);

        for (int i = 0; i < materialCost.Length; ++i)
        {
            Control cell = (Control) template.Duplicate();
            requirements.AddChild(cell);

            UpdateCell(cell, materialCost[i].Resource, materialCost[i].Cost);
        }

        template.QueueFree();

        // Initial update
        OnStructuresUpdated(null);
    }

    /// <summary>
    /// Use this to update the requirements list.
    /// </summary>
    public void Update(StructureDataInfo info)
    {
        int count = gameState.GetStructureCount(info.Identifier);
        int amount = Mathf.RoundToInt(countField.Value);

        Span<MaterialCostInfo> materialCost = 
            stackalloc MaterialCostInfo[info.Cost.Length];

        GetUpdatedCost(count, amount, ref materialCost);

        for (int i = 0; i < materialCost.Length; ++i)
        {
            Control cell = requirements.GetChild<Control>(i);
            UpdateCell(cell, materialCost[i].Resource, materialCost[i].Cost);
        }

        btnBuy.Disabled = !CanAfford(null, amount);
        btnSell.Disabled = count < amount;
    }

    void UpdateCell(Control cell, ResourceType type, double cost)
    {
        ResourceType resourceType = type;

        double amountInInventory = gameState
            .GetResourceCount(resourceType);

        cell.GetNode<Label>("Resource")
            .Text = type.ToString();

        cell.GetNode<Label>("Cost")
            .Text = $"{amountInInventory:0.0} / {cost:0.0}";

        cell.Modulate = gameState.HasResource(type, cost)
            ? Colors.LightGreen : Colors.Salmon;
    }

    void SetModalVisibility(bool visible)
    {
        UIStructureModal modal = GetNode<UIStructureModal>(pathModal);
        modal.SetVisible(visible);
    }

    void ShowModal(bool isBuying)
    {
        buyMode = isBuying;
        SetModalVisibility(true);

        UIStructureModal modal = GetNode<UIStructureModal>(pathModal);

        // TODO: Localisation
        modal.SetTitle(
            buyMode ? "Perform Purchase?" : "Sell Structure?"
        );
    }

    bool CanAfford(int? countOverride = null, int forecast = 1)
    {
        int count = countOverride ?? 
            gameState.GetStructureCount(info.Identifier);

        count = Math.Max(0, count - 1);

        Span<MaterialCostInfo> materialCost = 
            stackalloc MaterialCostInfo[info.Cost.Length];

        GetUpdatedCost(count, forecast, ref materialCost);

        for (int i = 0; i < materialCost.Length; ++i)
        {
            ResourceType resource = materialCost[i].Resource;
            double cost = materialCost[i].Cost;

            if (gameState.HasResource(resource, cost))
                continue;

            return false;
        }

        return true;
    }

    void PerformPurchase(int amount)
    {
        int count = gameState.GetStructureCount(info.Identifier);

        Span<MaterialCostInfo> materialCost =
            stackalloc MaterialCostInfo[info.Cost.Length];

        GetUpdatedCost(count, amount, ref materialCost);

        for (int i = 0; i < materialCost.Length; ++i)
        {
            gameState.TakeResource(
                materialCost[i].Resource, 
                materialCost[i].Cost);
        }

        gameState.AddStructure(info.Identifier, amount);
    }

    void PerformSell(int amount)
    {
        int count = gameState.GetStructureCount(info.Identifier);

        Span<MaterialCostInfo> materialCost =
            stackalloc MaterialCostInfo[info.Cost.Length];

        GetUpdatedCost(Math.Max(0, count - amount), amount, ref materialCost);

        for (int i = 0; i < materialCost.Length; ++i)
        {
            gameState.AddResource(
                materialCost[i].Resource, 
                materialCost[i].Cost * REFUND_FACTOR);
        }

        gameState.RemoveStructure(info.Identifier, amount);
    }

    /// <summary>
    /// <para>
    /// Writes to a span describing a material requirement constraint with the updated price.
    /// </para>
    /// <para>
    /// (Assumes that 'costInfo' is allocated and has the same length as 'info.Cost').
    /// </para>
    /// </summary>
    void GetUpdatedCost(int count, int forecast, ref Span<MaterialCostInfo> costInfo)
    {
        for (int i = 0; i < info.Cost.Length; ++i)
        {
            costInfo[i] = new(
                structureId: info.Identifier,
                structureCount: count,
                forecast: forecast,
                requirement: info.Cost[i]
            );
        }
    }

    /// <summary>
    /// Calculates the updated cost of a resource requirement.
    /// </summary>
    /// <returns></returns>
    public static double CalculateCostForResource(
        double baseCost,
        int count,
        int forecast = 1,
        double penaltyModifier = 1.0)
    {
        double total = 0.0f;

        for (int i = 0, l = count + forecast; i < l; ++i)
        {
            total += baseCost + Mathf.Round(
                baseCost * Mathf.Pow(i, 1.5) * penaltyModifier
            );
        }

        return total;
    }

    struct MaterialCostInfo
    {
        public readonly ResourceType Resource;
        public readonly double Cost;

        public MaterialCostInfo(
            StringName structureId,
            int structureCount,
            int forecast,
            ResourceRequirement requirement)
        {
            Resource = requirement.Type;

            Cost = UIStructure.CalculateCostForResource(
                baseCost: requirement.Amount,
                count: structureCount,
                forecast: forecast,
                penaltyModifier: requirement.PenaltyModifier
            );
        }
    }
}
