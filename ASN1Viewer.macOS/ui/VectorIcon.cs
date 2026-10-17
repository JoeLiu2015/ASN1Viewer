using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ASN1Viewer.macOS.ui;

// Geometry is drawn in a 24-point coordinate space, remaining sharp at every DPI.
public sealed class VectorIcon : Control
{
    public static readonly StyledProperty<string> KindProperty = AvaloniaProperty.Register<VectorIcon, string>(nameof(Kind), "context");
    public static readonly StyledProperty<bool> SelectedProperty = AvaloniaProperty.Register<VectorIcon, bool>(nameof(Selected));
    public string Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public bool Selected { get => GetValue(SelectedProperty); set => SetValue(SelectedProperty, value); }
    static VectorIcon() => AffectsRender<VectorIcon>(KindProperty, SelectedProperty);

    private const string Sequence = "M5,4 L19,4 Q21,4 21,6 L21,9 Q21,11 19,11 L5,11 Q3,11 3,9 L3,6 Q3,4 5,4 M3,9 L3,15 Q3,17 5,17 L19,17 Q21,17 21,15 L21,9 M3,15 L3,19 Q3,21 5,21 L19,21 Q21,21 21,19 L21,15";
    private const string Clock = "M12,2 A10,10 0 1 1 11.99,2 M12,6 L12,12 L16,12";
    private const string Link = "M12,3 A3,3 0 1 1 11.99,3 M10,9 L6,15 M14,9 L18,15 M5,15 A3,3 0 1 1 4.99,15 M19,15 A3,3 0 1 1 18.99,15";
    private static readonly Dictionary<string, Geometry> GeometryCache = new();
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var (path, glyph, color) = Describe(Kind);
        IBrush brush = Selected ? Brushes.White : new SolidColorBrush(Color.Parse(color));
        using var transform = context.PushTransform(Matrix.CreateScale(Bounds.Width / 24, Bounds.Height / 24));
        if (path != null)
        {
            if (!GeometryCache.TryGetValue(path, out var geometry)) GeometryCache[path] = geometry = Geometry.Parse(path);
            context.DrawGeometry(null, new Pen(brush, 1.8, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        }
        if (glyph != null)
        {
            var text = new FormattedText(glyph, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium), glyph.Length > 2 ? 10 : 16, brush);
            context.DrawText(text, new Point((24 - text.Width) / 2, (24 - text.Height) / 2));
        }
    }

    private static (string? path, string? glyph, string color) Describe(string kind)
    {
        const string blue = "#007AFF", orange = "#CF7418", green = "#24864B", purple = "#9254C8", gray = "#596579", teal = "#008F9C";
        return kind switch
        {
            "node-16" => (Sequence, null, blue),
            "node-17" => ("M3,3 H9 V9 H3 Z M15,3 H21 V9 H15 Z M3,15 H9 V21 H3 Z M15,15 H21 V21 H15 Z", null, blue),
            "node-1" => ("M8,6 H16 A6,6 0 0 1 16,18 H8 A6,6 0 0 1 8,6 M8,9 A3,3 0 1 1 7.99,9", null, orange),
            "node-2" => ("M9,3 L7,21 M17,3 L15,21 M3,9 H21 M2,16 H20", null, orange),
            "node-9" => (null, "1.2", orange),
            "node-10" => ("M3,5 H5 M3,12 H5 M3,19 H5 M10,5 H21 M10,12 H21 M10,19 H21", null, orange),
            "node-3" => ("M5,5 V19 M10,8 V16 M15,5 V19 M20,8 V16", null, purple),
            "node-4" => ("M2,5 H12 V16 H2 Z M12,9 H22 V20 H12", null, purple),
            "node-5" => (null, "∅", gray),
            "node-6" => (Link, null, purple),
            "node-13" => ("M3,4 V12 H12 M8,8 L12,12 L8,16 M15,12 H21 V20 H15 Z", null, purple),
            "node-7" => (null, "Id", purple),
            "node-8" => ("M13,3 H21 V11 M21,3 L10,14 M9,5 H3 V21 H19 V15", null, blue),
            "node-23" => (Clock, null, teal),
            "node-24" => ("M3,5 H21 V21 H3 Z M7,2 V8 M17,2 V8 M3,10 H21 M8,15 H16", null, teal),
            "node-12" => (null, "Aa", green),
            "node-18" => (null, "123", green),
            "node-19" => (null, "Ab", green),
            "node-20" => (null, "T61", green),
            "node-21" => (null, "Vtx", green),
            "node-22" => (null, "IA5", green),
            "node-25" => (null, "Gr", green),
            "node-26" => (null, "Vis", green),
            "node-27" => (null, "Gen", green),
            "node-28" => (null, "Uni", green),
            "node-30" => (null, "BMP", green),
            "open" => ("M2,6 H10 L12,9 H22 V20 H2 Z M2,9 H22", null, gray),
            "recent" => (Clock, null, gray),
            "samples" => ("M5,2 H14 L20,8 V22 H5 Z M14,2 V8 H20 M9,12 H16 M9,16 H16", null, gray),
            "modules" => ("M3,3 H10 V10 H3 Z M14,3 H21 V10 H14 Z M3,14 H10 V21 H3 Z M14,14 H21 V21 H14 Z", null, gray),
            "pin" => ("M7,3 H17 V5 H15 V10 L19,14 V16 H5 V14 L9,10 V5 H7 Z M12,16 V21", null, gray),
            "language" => ("M12,2 A10,10 0 1 1 11.99,2 M2,12 H22 M12,2 C4,8 4,16 12,22 C20,16 20,8 12,2", null, gray),
            "dropdown" => ("M5,8.5 L12,15.5 L19,8.5", null, gray),
            "more" => ("M5,11 A1,1 0 1 1 4.99,11 M12,11 A1,1 0 1 1 11.99,11 M19,11 A1,1 0 1 1 18.99,11", null, gray),
            _ => ("M8,3 H4 V21 H8 M16,3 H20 V21 H16", null, gray)
        };
    }
}
