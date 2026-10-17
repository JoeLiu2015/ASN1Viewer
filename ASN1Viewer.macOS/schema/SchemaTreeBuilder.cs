using Avalonia.Controls;
using ASN1Viewer.schema;

namespace ASN1Viewer.macOS.schema;

internal static class SchemaTreeBuilder
{
    public static TreeViewItem Create(SchemaFile schema) => new()
    {
        Header = $"{Path.GetFileName(schema.FileName)} ({schema.Name} {schema.Oid.GetValue()})",
        ItemsSource = new object[]
        {
            new TreeViewItem { Header = $"Types ({schema.Types.Count})", ItemsSource = schema.Types.Values.Select(t => CreateType(t, 0)).ToArray() },
            new TreeViewItem { Header = $"OIDs ({schema.Oids.Count})", ItemsSource = schema.Oids.Values.Select(o => new TreeViewItem { Header = $"{o.Name} {o.GetValue()}" }).ToArray() }
        }
    };
    private static TreeViewItem CreateType(TypeDef type, int depth)
    {
        var children = new List<TreeViewItem>();
        if (depth < 32)
        {
            if (type.SequenceOfTypeName != null) children.Add(new TreeViewItem { Header = type.SequenceOfTypeName });
            else if (type.SequenceOfType != null) children.Add(CreateType(type.SequenceOfType, depth + 1));
            if (type.Fields != null) foreach (var f in type.Fields)
                children.Add(new TreeViewItem { Header = $"{f.Name} {f.TypeName}", ItemsSource = f.Type == null ? null : new[] { CreateType(f.Type, depth + 1) } });
        }
        return new TreeViewItem { Header = $"{type.TypeName}({type.BaseTypeName})", ItemsSource = children };
    }
}
