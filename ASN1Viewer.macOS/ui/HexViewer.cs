using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace ASN1Viewer.macOS.ui;

public enum ByteRegion { Outside, Tag, Length, Content, EndOfContent }

// Paint only visible rows; the complete document remains available at every size.
public sealed class HexViewer : Grid
{
    private readonly ScrollBar scroll = new() { Orientation = Avalonia.Layout.Orientation.Vertical, SmallChange = 1, Width = 16 };
    private readonly HexSurface surface;
    public byte[] Data { get; private set; } = Array.Empty<byte>();
    public ASNNode? SelectedNode { get; private set; }
    public int FirstVisibleByte => (int)scroll.Value * 16;
    public const double LineHeight = 18;

    public HexViewer()
    {
        ColumnDefinitions = new ColumnDefinitions("*,16");
        surface = new HexSurface(this);
        var horizontal = new ScrollViewer
        {
            Content = surface, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        Children.Add(horizontal);
        SetColumn(scroll, 1);
        Children.Add(scroll);
        scroll.PropertyChanged += (_, e) => { if (e.Property == RangeBase.ValueProperty) surface.InvalidateVisual(); };
        SizeChanged += (_, _) => UpdateScroll();
        PointerWheelChanged += (_, e) =>
        {
            scroll.Value = Math.Clamp(scroll.Value - e.Delta.Y * 3, 0, scroll.Maximum);
            e.Handled = true;
        };
    }

    public void SetData(byte[] data)
    {
        Data = data;
        SelectedNode = null;
        scroll.Value = 0;
        UpdateScroll();
        surface.InvalidateVisual();
    }

    public void SelectNode(ASNNode node)
    {
        if (!ReferenceEquals(Data, node.Data)) SetData(node.Data);
        SelectedNode = node;
        scroll.Value = Math.Clamp(node.Start / 16, 0, scroll.Maximum);
        surface.InvalidateVisual();
    }

    private void UpdateScroll()
    {
        var visibleRows = Math.Max(1, (int)((Bounds.Height - 32) / LineHeight));
        scroll.Maximum = Math.Max(0, (Data.Length + 15) / 16 - visibleRows);
        scroll.ViewportSize = visibleRows;
        scroll.LargeChange = visibleRows;
        scroll.Value = Math.Min(scroll.Value, scroll.Maximum);
    }

    public ByteRegion RegionAt(int offset)
    {
        var n = SelectedNode;
        if (n == null || offset < n.Start || offset >= n.End) return ByteRegion.Outside;
        if (offset < n.Start + n.TagLen) return ByteRegion.Tag;
        if (offset < n.ContentStart) return ByteRegion.Length;
        if (offset < n.ContentEnd) return ByteRegion.Content;
        return ByteRegion.EndOfContent;
    }

    public byte[] HighlightedBytes => SelectedNode is { } n
        ? Data[n.Start..n.End] : Array.Empty<byte>();

    private sealed class HexSurface : Control
    {
        private readonly HexViewer owner;
        private readonly Typeface typeface = new(new FontFamily("Menlo"), FontStyle.Normal, FontWeight.Normal);
        private const double Cell = 8;
        public HexSurface(HexViewer owner) { this.owner = owner; MinWidth = 654; ClipToBounds = true; }
        public override void Render(DrawingContext context)
        {
            base.Render(context);
            context.FillRectangle(Brushes.White, new Rect(Bounds.Size));
            void Text(string text, double x, double y, IBrush color, double cellWidth = 0)
            {
                var formatted = new FormattedText(text, CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, typeface, 12, color);
                context.DrawText(formatted, new Point(x + (cellWidth > 0 ? (cellWidth - formatted.Width) / 2 : 0), y));
            }
            double columnWidth = Math.Max(24, (Bounds.Width - 92 - 20 - 16 * Cell) / 16);
            double asciiStart = 92 + 16 * columnWidth + 20;
            Text("Offset", 4, 7, Brushes.SlateGray);
            Text("Hexadecimal", 92, 7, Brushes.SlateGray);
            Text("ASCII", asciiStart, 7, Brushes.SlateGray);
            context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#E8EBF0")), 1), new Point(0, 28), new Point(Bounds.Width, 28));
            int rows = (int)Math.Ceiling((Bounds.Height - 32) / LineHeight);
            for (int row = 0; row < rows; row++)
            {
                int start = owner.FirstVisibleByte + row * 16;
                if (start >= owner.Data.Length) break;
                double y = row * LineHeight + 33;
                Text($"{start:X8}", 4, y, Brushes.Gray);

                // Fill each contiguous region once, avoiding seams at fractional column widths.
                int count = Math.Min(16, owner.Data.Length - start);
                for (int first = 0; first < count;)
                {
                    var region = owner.RegionAt(start + first);
                    int end = first + 1;
                    while (end < count && owner.RegionAt(start + end) == region) end++;
                    if (region != ByteRegion.Outside)
                    {
                        var tint = new SolidColorBrush(Color.Parse(region switch {
                            ByteRegion.Tag => "#FCE1E4", ByteRegion.Content => "#DCF3E3", _ => "#DFEBFF" }));
                        context.FillRectangle(tint, new Rect(92 + first * columnWidth, y,
                            (end - first) * columnWidth, LineHeight));
                        context.FillRectangle(tint, new Rect(asciiStart + first * Cell, y,
                            (end - first) * Cell, LineHeight));
                    }
                    first = end;
                }

                for (int col = 0; col < 16 && start + col < owner.Data.Length; col++)
                {
                    int offset = start + col;
                    var region = owner.RegionAt(offset);
                    IBrush color = region switch
                    {
                        ByteRegion.Tag => new SolidColorBrush(Color.Parse("#D03346")),
                        ByteRegion.Length or ByteRegion.EndOfContent => new SolidColorBrush(Color.Parse("#1264D7")),
                        ByteRegion.Content => new SolidColorBrush(Color.Parse("#238347")),
                        _ => new SolidColorBrush(Color.Parse("#394154"))
                    };
                    double x = 92 + col * columnWidth, asciiX = asciiStart + col * Cell;
                    byte b = owner.Data[offset];
                    Text(b.ToString("X2"), x, y, color, columnWidth);
                    Text(b >= 32 && b <= 126 ? ((char)b).ToString() : ".", asciiX, y, color, Cell);
                }
            }
        }
    }
}
