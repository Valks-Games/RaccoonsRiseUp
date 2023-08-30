namespace RRU.ResourceEditor;

[GlobalClass]
public sealed partial class EditorTechUpgradeDiscoverability : Resource
{
    IEditorTechUpgradeDiscoverability responder;

    public void BecomeResponder(IEditorTechUpgradeDiscoverability responder)
    {
        this.responder = responder;
    }

    public void GetTechUpgradeIds(ref Span<TechUpgradeInfo> upgrades)
    {
        responder.EditorListTechUpgradeIds(ref upgrades);
    }
}

public interface IEditorTechUpgradeDiscoverability
{
    void EditorListTechUpgradeIds(ref Span<TechUpgradeInfo> upgrades);
}