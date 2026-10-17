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
        ModuleTree.ItemsSource = SchemaFile.Schemas.Values.Select(SchemaTreeBuilder.Create).ToArray();
    }
}
