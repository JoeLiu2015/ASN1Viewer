using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ASN1Viewer.schema;
using ASN1Viewer.macOS.ui;
using ASN1Viewer.macOS.schema;

namespace ASN1Viewer.macOS;

public static class TreeConverters
{
    public static readonly IValueConverter HasChildren = new FuncValueConverter<object?, bool>(v =>
        v is System.Collections.IEnumerable items && items.Cast<object>().Any());
}

public sealed class NodeItem : INotifyPropertyChanged
{
    public ASNNode Node { get; }
    public string Label => Node.ToString();
    // Keep the full ASN.1 representation for copy/tooltips; the sidebar emphasizes schema names.
    public string DisplayLabel
    {
        get
        {
            var match = System.Text.RegularExpressions.Regex.Match(Label,
                @"^(.+?)\s+(?:SEQUENCE|SET|INTEGER|BOOLEAN|BITSTRING|OCTETSTRING|NULL|OID|UTF8String|PrintableString|UTCTime|GeneralizedTime|\[Context\])(?:\b|\[)");
            return match.Success ? match.Groups[1].Value : Label;
        }
    }
    public string IconKind => Node.TagClass == ASNNode.NODE_CLASS_UNIVERSAL ? "node-" + Node.TagNum : "context";
    public int IconIndex => Node.TagClass != ASNNode.NODE_CLASS_UNIVERSAL ? 24 : Node.TagNum switch
    {
        1 => 21, 2 or 9 or 10 => 1, 3 => 23, 4 => 25, 5 => 18, 6 or 7 => 11,
        8 => 6, 13 => 35, 16 => 32, 17 => 33, 18 => 8, 23 or 24 => 34,
        12 or 19 or 20 or 21 or 22 or 25 or 26 or 27 or 28 or 30 => 7, _ => 24
    };
    private ObservableCollection<NodeItem>? children;
    public ObservableCollection<NodeItem> Children => children ??= new(
        Enumerable.Range(0, Node.ChildCount).Select(i => new NodeItem(Node[i])));
    private bool expanded;
    public bool Expanded { get => expanded; set { expanded = value; PropertyChanged?.Invoke(this, new(nameof(Expanded))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    public NodeItem(ASNNode node) => Node = node;
}

public partial class MainForm : Window
{
    private ASNNode? root;
    private readonly bool rememberPreferences;
    private bool busy, schemasLoaded, suppressInput;
    private int inputRevision;
    private string? programmaticInput;
    private string language = "en_US";
    private readonly Lang lang = new();
    private readonly List<string> recent = new();
    private readonly DispatcherTimer inputTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private const int MaxFileSize = 16 * 1024 * 1024;
    public bool IsParsing => busy || inputTimer.IsEnabled;
    public MainForm() : this(null) { }

    public MainForm(string? initialFile, bool rememberPreferences = true)
    {
        this.rememberPreferences = rememberPreferences;
        InitializeComponent();
        if (rememberPreferences && !Design.IsDesignMode)
        {
            var config = Config.Load();
            language = config.Language;
            Topmost = config.TopMost;
            recent.AddRange(config.History);
        }
        SetLanguage(language);
        inputTimer.Tick += async (_, _) =>
        {
            if (busy) return;
            inputTimer.Stop();
            if (!string.IsNullOrWhiteSpace(Input.Text)) await ParseText(Input.Text);
        };
        Closed += (_, _) =>
        {
            inputTimer.Stop();
            if (this.rememberPreferences && !Design.IsDesignMode)
                new Config { Language = language, TopMost = Topmost, History = recent }.Save();
        };
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, (_, e) =>
        {
            e.DragEffects = !busy && e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        });
        AddHandler(DragDrop.DropEvent, async (_, e) =>
        {
            e.Handled = true;
            var file = e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().FirstOrDefault();
            if (file != null) await LoadStorageFile(file);
        });
        KeyDown += async (_, e) =>
        {
            var modifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
            if (e.Key == Key.O && e.KeyModifiers == modifier) { e.Handled = true; await PickFile(); }
        };
        Tree.AddHandler(PointerPressedEvent, (_, e) =>
        {
            if (!e.GetCurrentPoint(Tree).Properties.IsRightButtonPressed) return;
            var visual = e.Source as Control;
            while (visual != null && visual is not TreeViewItem) visual = visual.Parent as Control;
            if (visual is TreeViewItem item) Tree.SelectedItem = item.DataContext;
            BuildContextMenu();
        }, RoutingStrategies.Tunnel);
        if (initialFile != null) Opened += async (_, _) => await LoadFile(initialFile);
    }

    private void HeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Only the header's empty space and title drag the window; buttons retain clicks.
        var visual = e.Source as Control;
        while (visual != null && visual != sender)
        {
            if (visual is Button) return;
            visual = visual.Parent as Control;
        }
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private string T(string key) => lang[key];
    private void SetLanguage(string languageCode)
    {
        language = languageCode;
        lang.Select(languageCode);
        InputTab.Header = T("TAB_INPUT"); BytesTab.Header = T("TAB_BYTES");
        Title = T("PROD_NAME");
        BuildMenus();
        if (Tree.SelectedItem is NodeItem) SelectionChanged(null, null!);
    }

    private MenuItem Item(string key, Action action)
    {
        var item = new MenuItem { Header = T(key) };
        item.Click += (_, _) => action();
        return item;
    }
    private MenuItem Submenu(string key, IEnumerable<object> items) => new() { Header = T(key), ItemsSource = items.ToArray() };
    private IEnumerable<object> SampleItems() => Directory.Exists(DesktopUtils.GetFullPath("files/TestFiles"))
        ? Directory.GetFiles(DesktopUtils.GetFullPath("files/TestFiles")).OrderBy(Path.GetFileName)
            .Select(path => (object)Item(Path.GetFileName(path), async () => await LoadFile(path)))
        : new object[] { new MenuItem { Header = "No sample files", IsEnabled = false } };
    private IEnumerable<object> RecentItems() => recent.Count == 0
        ? new object[] { new MenuItem { Header = "—", IsEnabled = false } }
        : recent.Select(path => (object)Item(path, async () => await LoadFile(path))).ToArray();
    private IEnumerable<object> LanguageItems() => new object[]
    {
        Item("MENU_CHINESE", () => SetLanguage("zh_CN")), Item("MENU_ENGLISH", () => SetLanguage("en_US"))
    };

    private void BuildMenus()
    {
        Actions.Children.Clear();
        Tool("open", "MENU_OPEN", async () => await PickFile());
        Tool("recent", "MENU_RECENT", null, RecentItems);
        Tool("samples", "MENU_TESTFILES", null, SampleItems);
        Tool("modules", "MENU_MODULES", ShowModules);
        Tool("pin", "MENU_TOPMOST", () => { Topmost = !Topmost; BuildMenus(); }, active: Topmost);
        Tool("language", "MENU_LANG", null, LanguageItems);
        Tool("more", language == "zh_CN" ? "更多" : "More", null, () => new object[] {
            Item("MENU_ABOUT", () => new About(lang).ShowDialog(this)),
            Item("MENU_CHECKVER", UpdateNotice), new Separator(), Item("MENU_EXIT", Close)
        });
        StructureTitle.Text = language == "zh_CN" ? "结构" : "Structure";
        Input.Watermark = language == "zh_CN" ? "粘贴 PEM、Base64 或 HEX，自动解析…" : "Paste PEM, Base64 or HEX to parse automatically…";
        BuildContextMenu();
    }

    private string ActionLabel(string icon, string key) => (icon, language == "zh_CN") switch
    {
        ("open", true) => "打开", ("open", false) => "Open",
        ("recent", true) => "最近", ("recent", false) => "Recent",
        ("samples", true) => "示例", ("samples", false) => "Samples",
        ("modules", true) => "ASN.1 模块", ("modules", false) => "Modules",
        ("pin", true) => "置顶", ("pin", false) => "Pin",
        ("language", true) => "语言", ("language", false) => "Language",
        _ => T(key)
    };

    private void Tool(string icon, string key, Action? action, Func<IEnumerable<object>>? menu = null, bool active = false)
    {
        var content = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 7 };
        content.Children.Add(new VectorIcon { Kind = icon, Width = 18, Height = 18, Selected = icon == "open" });
        if (icon != "more") content.Children.Add(new TextBlock { Text = ActionLabel(icon, key), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        if (menu != null && icon != "more") content.Children.Add(new VectorIcon
        {
            Kind = "dropdown", Width = 12, Height = 12,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
        var button = new Button { Name = "Action_" + icon, Content = content, Tag = icon };
        button.Classes.Add("action");
        if (icon == "open") button.Classes.Add("primary");
        if (active) button.Classes.Add("active");
        ToolTip.SetTip(button, T(key));
        Avalonia.Automation.AutomationProperties.SetName(button, T(key));
        if (menu != null) button.ContextMenu = new ContextMenu { ItemsSource = menu().ToArray() };
        button.Click += (_, _) =>
        {
            if (menu != null) { button.ContextMenu = new ContextMenu { ItemsSource = menu().ToArray() }; button.ContextMenu.Open(button); }
            else action?.Invoke();
        };
        Actions.Children.Add(button);
    }

    private void BuildContextMenu()
    {
        var selected = Tree.SelectedItem as NodeItem;
        bool leaf = selected != null && selected.Children.Count == 0;
        var collapse = Item("MENU_COLLAPSE", () => { if (selected != null) selected.Expanded = false; });
        var expand = Item("MENU_EXPAND", () => { if (selected != null) Expand(selected); });
        collapse.IsEnabled = expand.IsEnabled = selected != null && !leaf;
        var edit = Item("MENU_EDIT", () => { if (selected != null) EditNode(selected); }); edit.IsEnabled = leaf;
        var parse = Item("MENU_PARSE", () => { if (selected != null) ParseContent(selected); }); parse.IsEnabled = leaf;
        var copyText = Item("MENU_COPYTEXT", async () => { if (selected != null && Clipboard != null) await Clipboard.SetTextAsync(selected.Label); });
        var copyHex = Item("MENU_COPYHEX", async () => { if (selected != null && Clipboard != null) await Clipboard.SetTextAsync(Convert.ToHexString(selected.Node.Data[selected.Node.Start..selected.Node.End])); });
        copyText.IsEnabled = copyHex.IsEnabled = selected != null;
        Tree.ContextMenu = new ContextMenu { ItemsSource = new object[] { collapse, expand, new Separator(), edit, copyText, copyHex, new Separator(), parse } };
        Hex.ContextMenu = new ContextMenu { ItemsSource = new object[] { Item("RICH_CTX_MENU_COPY", async () =>
        { if (Clipboard != null) await Clipboard.SetTextAsync(Convert.ToHexString(Hex.HighlightedBytes)); }) } };
    }
    private static void Expand(NodeItem item) { item.Expanded = true; foreach (var child in item.Children) Expand(child); }

    private void InputChanged(object? sender, TextChangedEventArgs e)
    {
        if (suppressInput || Input.Text == programmaticInput) { programmaticInput = null; return; }
        inputRevision++;
        inputTimer.Stop(); inputTimer.Start();
    }
    public Task ParseText(string text) => LoadDocument(() => Task.FromResult(System.Text.Encoding.UTF8.GetBytes(text)), T("TXT_TEXT"), true);

    private async void OpenClicked(object? sender, RoutedEventArgs e) => await PickFile();
    private async Task PickFile()
    {
        if (busy) return;
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = T("MENU_OPEN"), AllowMultiple = false });
            if (files.Count > 0) await LoadStorageFile(files[0]);
        }
        catch (Exception ex) { ShowError(ex); }
    }
    public Task LoadFile(string path) => LoadDocument(async () =>
    {
        if (new FileInfo(path).Length > MaxFileSize) throw new FormatException("Files must be 16 MiB or smaller.");
        return await File.ReadAllBytesAsync(path);
    }, Path.GetFullPath(path));

    private Task LoadStorageFile(IStorageFile file)
    {
        var path = file.TryGetLocalPath();
        if (path != null) return LoadFile(path);
        return LoadDocument(async () =>
        {
            await using var stream = await file.OpenReadAsync();
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int count;
            while ((count = await stream.ReadAsync(chunk)) > 0)
            {
                if (buffer.Length + count > MaxFileSize) throw new FormatException("Files must be 16 MiB or smaller.");
                await buffer.WriteAsync(chunk.AsMemory(0, count));
            }
            return buffer.ToArray();
        }, file.Name);
    }

    private void EnsureSchemas()
    {
        if (schemasLoaded) return;
        SchemaFile.ReloadSchemas(Path.Combine(AppContext.BaseDirectory, "files", "Asn1Modules"));
        schemasLoaded = true;
    }
    private async Task LoadDocument(Func<Task<byte[]>> read, string name, bool textOnly = false)
    {
        if (busy) return;
        inputTimer.Stop();
        int revision = inputRevision;
        busy = true;
        StatusRight.Text = "Parsing…";
        try
        {
            var bytes = await read();
            if (bytes.Length > MaxFileSize) throw new FormatException("Input must be 16 MiB or smaller.");
            EnsureSchemas();
            var parsed = await Task.Run(() =>
            {
                var decoded = ASNParser.DecodeText(System.Text.Encoding.UTF8.GetString(bytes));
                if (textOnly && decoded == null) throw new FormatException("Enter valid PEM, Base64, or HEX data.");
                return ASNParser.Parse(decoded ?? bytes);
            });
            if (textOnly && revision != inputRevision) return;
            root = parsed;
            Hex.SetData(parsed.Data);
            var item = new NodeItem(parsed) { Expanded = true };
            Tree.ItemsSource = new[] { item };
            Tree.SelectedItem = item;
            if (textOnly) Tabs.SelectedIndex = 0;
            else
            {
                suppressInput = true;
                Input.Text = programmaticInput = ASNParser.DecodeText(System.Text.Encoding.UTF8.GetString(bytes)) != null
                    ? System.Text.Encoding.UTF8.GetString(bytes) : Utils.HexDump(parsed.Data, 0);
                suppressInput = false;
                if (Path.IsPathFullyQualified(name))
                {
                    recent.Remove(name); recent.Insert(0, name);
                    if (recent.Count > 10) recent.RemoveAt(10);
                    BuildMenus();
                }
            }
            Title = T("PROD_NAME") + " - " + name;
            DocumentName.Text = Path.GetFileName(name);
            StatusRight.Text = $"{parsed.Data.Length:N0} bytes";
        }
        catch (Exception ex) { if (!textOnly || revision == inputRevision) ShowError(ex); }
        finally { busy = false; }
    }

    private void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Tree.SelectedItem is not NodeItem item) return;
        var node = item.Node;
        Hex.SelectNode(node);
        Tabs.SelectedIndex = 1;
        Status.Foreground = Brushes.Black;
        Status.Text = string.Format(CultureInfo.CurrentCulture, T("STATUS_ASNINFO"), node.Start, node.TagNum,
            node.ContentEnd - node.ContentStart, CountChildren(node));
        BuildContextMenu();
    }
    private static int CountChildren(ASNNode n) => Enumerable.Range(0, n.ChildCount).Sum(i => 1 + CountChildren(n[i]));
    private void ParseContent(NodeItem item)
    {
        try
        {
            var n = item.Node;
            item.Children.Add(new NodeItem(ASNNode.Parse(n.Data, n.ContentStart, n.ContentEnd - n.ContentStart, false)));
            item.Expanded = true;
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void EditNode(NodeItem item)
    {
        var dialog = new EditASN1Node(lang) { Node = item };
        dialog.OnNodeChanged += (_, _) =>
        {
            if (root == null) return;
            var updated = new NodeItem(root) { Expanded = true };
            Tree.ItemsSource = new[] { updated };
            NodeItem? Find(NodeItem n)
            {
                if (n.Node == item.Node) return n;
                foreach (var child in n.Children)
                {
                    var match = Find(child);
                    if (match != null) { n.Expanded = true; return match; }
                }
                return null;
            }
            Tree.SelectedItem = Find(updated) ?? updated;
            suppressInput = true;
            Input.Text = programmaticInput = Utils.HexDump(root.Data, 0);
            suppressInput = false;
        };
        dialog.ShowDialog(this);
    }
    private void ShowModules()
    {
        if (busy) return;
        EnsureSchemas();
        new SchemaDlg(lang).ShowDialog(this);
    }
    private void UpdateNotice() => Updater.ShowStatus(this, lang);
    private void ShowError(Exception ex) { Status.Foreground = Brushes.Red; Status.Text = T("MSG_ERROR") + ": " + ex.Message; StatusRight.Text = ""; }
}
