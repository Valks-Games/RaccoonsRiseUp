namespace RRU;

[GlobalClass]
public sealed partial class StructureDataService : Resource
{
    [Export] StructureDataInfo[] structures;

    public StructureDataService()
    {
    }

    public StructureDataService(StructureDataInfo[] structures)
    {
        this.structures = structures;
    }

    /// <summary>
    /// Writes to a span pointing to an array of structure definitions.
    /// </summary>
    public void GetStructures(ref ReadOnlySpan<StructureDataInfo> structures)
    {
        structures = this.structures;
    }

    /// <summary>
    /// (Must only be called by the Resource Editor.)
    /// </summary>
    public void _SetStructure(int idx, StructureDataInfo data)
    {
        structures[idx] = data;
    }
}