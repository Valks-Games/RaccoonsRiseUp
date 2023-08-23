namespace RRU;

[GlobalClass]
public sealed partial class StructureDataService : Resource
{
    [Export] StructureDataInfo[] structures;

    /// <summary>
    /// Writes to a span pointing to an array of structure definitions.
    /// </summary>
    public void GetStructures(ref ReadOnlySpan<StructureDataInfo> structures)
    {
        structures = this.structures;
    }
}