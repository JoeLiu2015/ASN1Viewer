using Avalonia.Controls;
using ASN1Viewer.schema;

namespace ASN1Viewer.macOS.schema;

public partial class SchemaDlg : Window
{
    public SchemaDlg() : this(new Lang()) { }
    public SchemaDlg(Lang lang)
    {
        InitializeComponent();
        Title = lang["MODULES_TITLE"];
        Heading.Text = Title;
        ModuleCount.Text = $"{SchemaFile.Schemas.Count} ASN.1 modules";
        Done.Content = lang["BTN_OK"];
        ModuleTree.ItemsSource = SchemaFile.Schemas.Values.Select(SchemaTreeBuilder.Create).ToArray();
    }
    private void DoneClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
