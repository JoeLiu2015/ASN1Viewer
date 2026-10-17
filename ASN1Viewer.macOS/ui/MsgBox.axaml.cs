using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ASN1Viewer.macOS.ui;

public partial class MsgBox : Window
{
    public MsgBox() => InitializeComponent();
    public string? OKText { get => OKButton.Content as string; set => OKButton.Content = value; }
    public string? CancelText { get => CancelButton.Content as string; set { CancelButton.Content = value; CancelButton.IsVisible = value != null; } }
    public string? Message { get => MessageText.Text; set => MessageText.Text = value; }
    public static Task<bool> Show(Window owner, string title, string message, string okText, string? cancelText = null)
    {
        var dialog = new MsgBox { Title = title, Message = message, OKText = okText, CancelText = cancelText };
        return dialog.ShowDialog<bool>(owner);
    }
    private void OKClicked(object? sender, RoutedEventArgs e) => Close(true);
    private void CancelClicked(object? sender, RoutedEventArgs e) => Close(false);
}
