using Avalonia.Controls;

namespace ASN1Viewer.macOS.ui;

public partial class EditASN1Node : Window
{
    private NodeItem? node;
    private readonly Lang lang;
    public event EventHandler? OnNodeChanged;
    public EditASN1Node() : this(new Lang()) { }
    public EditASN1Node(Lang lang)
    {
        this.lang = lang;
        InitializeComponent();
        EditLabel.Text = lang["TXT_EDITHEX"];
    }
    public NodeItem? Node
    {
        get => node;
        set
        {
            node = value;
            HexEdit.Text = value == null ? "" : Utils.GetHexString(value.Node.Value, 0, value.Node.Value.Length);
            UpdateTitle();
        }
    }
    public ASNNode? ASNNode => node?.Node;
    private void UpdateTitle() => Title = lang["TXT_EDITHEX"] + (node == null ? "" : " - " + node.Label);
    private void HexChanged(object? sender, TextChangedEventArgs e)
    {
        if (node == null) return;
        try
        {
            var value = string.IsNullOrWhiteSpace(HexEdit.Text) ? Array.Empty<byte>() : Utils.ParseHexBytes(HexEdit.Text!);
            if (value == null) { Error.Text = "Invalid HEX"; return; }
            if (node.Node.SetContent(value)) OnNodeChanged?.Invoke(this, EventArgs.Empty);
            Error.Text = "";
            UpdateTitle();
        }
        catch (Exception ex) { Error.Text = ex.Message; }
    }
}
