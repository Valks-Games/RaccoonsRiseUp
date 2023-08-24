namespace RRU;

public partial class SaveData
{
    public int Raccoons { get; set; }
    public JobDict NumJobs { get; set; }
    public ResourceDict NumResources { get; set; }
    public StructureDict Structures { get; set; }
    public string[] ResearchedUpgrades { get; set; }
}
