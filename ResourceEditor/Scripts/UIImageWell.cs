namespace RRU.ResourceEditor;

public sealed partial class UIImageWell : VBoxContainer
{
    [Export] UIIconBrowser iconBrowser;

    TextureRect imageView;
    Button btnChange;

    public override void _Ready()
    {
        imageView = GetNode<TextureRect>("%Image");
        btnChange = GetNode<Button>("%ChangeImage");

        if (!IsInstanceValid(iconBrowser))
        {
            btnChange.Visible = false;
            return;
        }

        btnChange.Pressed += iconBrowser.StartModal;
        iconBrowser.OnIconSelected += OnImagePicked;
    }

    public void SetImage(Texture2D image)
    {
        imageView.Texture = image;
    }

    /// Events ///

    void OnImagePicked(Texture2D image)
    {
        imageView.Texture = image;
    }
}
