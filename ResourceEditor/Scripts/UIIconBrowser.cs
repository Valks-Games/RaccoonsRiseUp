using Godot;
using System;

public partial class UIIconBrowser : PopupPanel
{
    static Vector2I ModalSize => new(380, 380);
    static StringName KeyIconPath => "iconPath";

    string[] AllowedExtensionTypes = {
        ".png",
        ".svg"
    };

    public event Action OnModalStarted;
    public event Action<Texture2D> OnIconSelected;

    [Export(PropertyHint.Dir)] string IconsDir;

    Control collectionView;
    Button template;

    Button btnClose;
    Button btnSelect;

    string selectedPath;

    public override void _Ready()
    {
        // Get
        collectionView = GetNode<Control>("%Collection");
        template = GetNode<Button>("%Template");

        btnClose = GetNode<Button>("%Close");
        btnSelect = GetNode<Button>("%Select");

        // Init
        template.Visible = false;
        ResetState();

        // Bind
        btnSelect.Pressed += OnSelectPressed;
        btnClose.Pressed += Hide;
    }

    /// <summary>
    /// Call this to show the popup.
    /// </summary>
    public void StartModal()
    {
        ReadOnlySpan<string> extensions = AllowedExtensionTypes;
        ReadOnlySpan<string> candidates = DirAccess.GetFilesAt(IconsDir);

        for (int i = 0; i < candidates.Length; ++i)
        {
            if (!IsIconFile(candidates[i]))
                continue;

            string path = $"{IconsDir}/{candidates[i]}";

            Button cell = (Button) template.Duplicate();
            cell.SetMeta(KeyIconPath, path);
            cell.Visible = true;

            TextureRect iconView = cell.GetNode<TextureRect>("Margin/Icon");
            iconView.Texture = ResourceLoader.Load<Texture2D>(path);

            collectionView.AddChild(cell);
            cell.Pressed += () => OnCellPressed(cell);
        }

        PopupCentered(ModalSize);
        OnModalStarted?.Invoke();
    }

    void ResetState()
    {
        selectedPath = null;
        btnSelect.Disabled = true;

        for (int i = 0, l = collectionView.GetChildCount(); i --> 0;)
        {
            Control child = collectionView.GetChild<Control>(i);

            if (!child.Visible)
                continue;

            child.QueueFree();
        }
    }

    bool IsIconFile(ReadOnlySpan<char> filePath)
    {
        ReadOnlySpan<string> extensions = AllowedExtensionTypes;

        for (int i = 0; i < extensions.Length; ++i)
        {
            if (!filePath.EndsWith(extensions[i]))
                continue;

            return true;
        }

        return false;
    }

    /// Events ///

    void OnCellPressed(Button cellRef)
    {
        selectedPath = (string) cellRef.GetMeta(KeyIconPath);
        btnSelect.Disabled = false;
    }

    void OnSelectPressed()
    {
        if (selectedPath == null)
            return;

        Texture2D texture = ResourceLoader.Load<Texture2D>(selectedPath);
        OnIconSelected?.Invoke(texture);

        ResetState();
        Hide();
    }
}
