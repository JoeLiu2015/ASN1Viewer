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
    public const double LineHeight = 20;

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
        var visibleRows = Math.Max(1, (int)((Bounds.Height - 18) / LineHeight));
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
        private readonly Typeface typeface = new("Menlo,Consolas,monospace");
        private const double Cell = 8;
        public HexSurface(HexViewer owner) { this.owner = owner; MinWidth = 654; ClipToBounds = true; }
        public override void Render(DrawingContext context)
        {
            base.Render(context);
            context.FillRectangle(Brushes.White, new Rect(Bounds.Size));
            void Text(string text, double x, double y, IBrush color) => context.DrawText(
                new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 13, color), new Point(x, y));
            int rows = (int)Math.Ceiling(Bounds.Height / LineHeight);
            for (int row = 0; row < rows; row++)
            {
                int start = owner.FirstVisibleByte + row * 16;
                if (start >= owner.Data.Length) break;
                double y = row * LineHeight + 3;
                Text($"{start:X8}|", 4, y, Brushes.Gray);
                Text("|", 490, y, Brushes.Gray);
                // Windows paints a continuous band across spaces between selected bytes.
                if (owner.SelectedNode is { } selected)
                {
                    int first = Math.Max(start, selected.Start) - start;
                    int last = Math.Min(start + 16, selected.End) - start - 1;
                    if (first <= last && first < 16 && last >= 0)
                    {
                        double firstX = 92 + first * 24 + (first >= 8 ? 8 : 0);
                        double lastX = 92 + last * 24 + (last >= 8 ? 8 : 0);
                        var yellow = new SolidColorBrush(Color.FromRgb(255, 255, 179));
                        context.FillRectangle(yellow, new Rect(firstX, y, lastX - firstX + 2 * Cell, LineHeight));
                        context.FillRectangle(yellow, new Rect(508 + first * Cell, y, (last - first + 1) * Cell, LineHeight));
                    }
                }
                for (int col = 0; col < 16 && start + col < owner.Data.Length; col++)
                {
                    int offset = start + col;
                    var region = owner.RegionAt(offset);
                    var color = region switch
                    {
                        ByteRegion.Tag => Brushes.Red,
                        ByteRegion.Length or ByteRegion.EndOfContent => Brushes.Blue,
                        ByteRegion.Content => Brushes.Green,
                        _ => Brushes.Gray
                    };
                    double x = 92 + col * 24 + (col >= 8 ? 8 : 0), asciiX = 508 + col * Cell;
                    byte b = owner.Data[offset];
                    Text(b.ToString("X2"), x, y, color);
                    Text(b >= 32 && b <= 126 ? ((char)b).ToString() : ".", asciiX, y, color);
                }
            }
        }
    }
}
