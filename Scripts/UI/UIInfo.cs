namespace RRU;

public partial class UIInfo : Node
{
    [Export] GameState gameState;

    [Export] Label labelRaccoons;
    [Export] Label labelWoodcutters;
    [Export] Label labelResearchers;
    [Export] Label labelWood;
    [Export] Label labelTech;

    Dictionary<JobType, Label> jobLabels;
    Dictionary<ResourceType, Label> resourceLabels;

    public override void _Ready()
    {
        jobLabels = new()
        {
            { JobType.Woodcutter, labelWoodcutters },
            { JobType.Researcher, labelResearchers }
        };

        resourceLabels = new()
        {
            [ResourceType.Wood] = labelWood,
            [ResourceType.Tech] = labelTech
        };

        gameState.ResourcesChanged += _ => UpdateResourceCounts();
        gameState.JobsChanged += _ => UpdateAllJobCounts();

        UIJob.RaccoonAssigned += OnUpdateJobcount;
        UIJob.RaccoonUnassigned += OnUpdateJobcount;

        // Initial update
        UpdateResourceCounts();
        UpdateAllJobCounts();
        UpdateRaccoonsLabel();
    }

    /// Event Handlers ///

    void UpdateResourceCounts()
    {
        ReadOnlySpan<ResourceType> resourceTypes = default;
        gameState.GetResourceTypes(ref resourceTypes);

        for (int i = 0; i < resourceTypes.Length; ++i)
        {
            ResourceType type = resourceTypes[i];

            if (!resourceLabels.TryGetValue(type, out Label label))
                continue;

            label.Text = Utils.RoundGameValue(gameState.Resources[type]);
        }
    }

    void UpdateAllJobCounts()
    {
        ReadOnlySpan<JobType> jobs = default;
        gameState.GetJobTypes(ref jobs);

        for (int i = 0; i < jobs.Length; ++i)
            UpdateJobCount(jobs[i]);
    }

    void OnUpdateJobcount(JobType job)
    {
        UpdateJobCount(job);
        UpdateRaccoonsLabel();
    }

    void UpdateJobCount(JobType job)
    {
        if (!jobLabels.TryGetValue(job, out Label label))
            return;

        label.Text = gameState.Jobs[job].ToString();
    }

    void UpdateRaccoonsLabel()
    {
        labelRaccoons.Text = gameState.Raccoons.ToString();
    }
}
