using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Threading;
using ASN1Viewer.macOS;
using ASN1Viewer.macOS.ui;

AppBuilder.Configure<App>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
var window = new MainForm(null, rememberPreferences: false);
window.Show();
var input = window.FindControl<TextBox>("Input")!;
var tree = window.FindControl<TreeView>("Tree")!;
var status = window.FindControl<TextBlock>("Status")!;
var tabs = window.FindControl<TabControl>("Tabs")!;
var hex = window.FindControl<HexViewer>("Hex")!;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
void PumpUntil(Func<bool> ready)
{
    var deadline = DateTime.UtcNow.AddSeconds(20);
    var frame = new DispatcherFrame();
    using var timer = new System.Threading.Timer(_ => Dispatcher.UIThread.Post(() =>
    {
        if (ready() || DateTime.UtcNow >= deadline) frame.Continue = false;
    }), null, 10, 10);
    Dispatcher.UIThread.PushFrame(frame);
    Check(ready(), "Operation timed out");
}
void Parse(string text)
{
    var task = window.ParseText(text);
    PumpUntil(() => task.IsCompleted);
    task.GetAwaiter().GetResult();
}
Check(window.FindControl<Menu>("MainMenu")!.Items.Count == 3, "Windows menu structure missing");
Check(window.FindControl<StackPanel>("Toolbar")!.Children.Count == 7, "Windows toolbar structure missing");
Parse("30 03 02 01 2A");
var root = (NodeItem)tree.SelectedItem!;
Check(root.Children.Count == 1 && root.IconIndex == 32 && root.Children[0].IconIndex == 1, "Node icons or children missing");
tree.SelectedItem = root.Children[0];
Check(tabs.SelectedIndex == 1, "Tree selection must switch to ASN.1 Bytes");
Check(status.Text!.Contains("Offset[2]") && status.Text.Contains("Length[1]"), "Windows status format missing");
Check(hex.Data.Length == 5, "Selection must preserve the complete document");
Check(hex.RegionAt(0) == ByteRegion.Outside && hex.RegionAt(2) == ByteRegion.Tag && hex.RegionAt(3) == ByteRegion.Length && hex.RegionAt(4) == ByteRegion.Content, "Header/content highlight boundaries differ");
Check(hex.HighlightedBytes.SequenceEqual(new byte[] {2,1,42}), "Copy highlight must include TLV bytes");
// Exercise the actual Edit context action and its live length-changing update.
var edit = tree.ContextMenu!.Items.OfType<MenuItem>().Single(i => (string?)i.Header == "Edit");
edit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
var editor = window.OwnedWindows.Single();
var editText = editor.GetVisualDescendants().OfType<TextBox>().Single();
editText.Text = "01 02";
PumpUntil(() => hex.Data.Length == 6);
Check(hex.SelectedNode!.Value.SequenceEqual(new byte[] { 1, 2 }), "Live edit did not update selected node");
Check(hex.Data.SequenceEqual(new byte[] { 48, 4, 2, 2, 1, 2 }), "Live edit did not resize parent length");
editor.Close();
Parse("-----BEGIN CERTIFICATE-----\nMAMCAQE=\n-----END CERTIFICATE-----");
Check(hex.Data.Length == 5 && tabs.SelectedIndex == 0, "PEM parse / input tab behavior");
var previous = tree.SelectedItem;
Parse("invalid %%");
Check(status.Text!.StartsWith("ERROR"), "Invalid input has no error");
Check(ReferenceEquals(previous, tree.SelectedItem), "Invalid input replaced the previous document");
Parse("30 80 02 01 01 00 00");
Check(hex.RegionAt(5) == ByteRegion.EndOfContent && hex.RegionAt(6) == ByteRegion.EndOfContent && hex.RegionAt(7) == ByteRegion.Outside, "BER end markers not highlighted correctly");
Check(hex.HighlightedBytes.Length == 7, "BER copy drops end-of-content bytes");
Parse("9F 81 49 01 00");
Check(hex.RegionAt(2) == ByteRegion.Tag && hex.RegionAt(3) == ByteRegion.Length, "Multi-byte tag highlighting");
input.Text = "30 03 02 01 07";
PumpUntil(() => !window.IsParsing && hex.Data.LastOrDefault() == 7);
Check(hex.Data.Last() == 7, "Input changes must auto-parse without a button");
var fileTask = window.LoadFile(Path.Combine(AppContext.BaseDirectory, "files", "TestFiles", "PKCS12.pfx"));
PumpUntil(() => fileTask.IsCompleted);
fileTask.GetAwaiter().GetResult();
Check(input.Text!.Length > 0 && window.Title!.EndsWith("PKCS12.pfx"), "File path or input mirror missing");
Check(((NodeItem)tree.SelectedItem!).Label.Contains("PFX"), "Sample schema matching missing");
// A selected node past the old 64 KiB preview limit remains reachable in the full document.
var data = new byte[100000]; data[90000] = 2; data[90001] = 1; data[90002] = 42;
var distant = ASN1Viewer.ASNNode.Parse(data, 90000, 3, false);
hex.SetData(data); hex.SelectNode(distant);
Check(hex.Data.Length == 100000 && hex.FirstVisibleByte == 90000 && hex.RegionAt(90002) == ByteRegion.Content, "Full-document navigation beyond 64 KiB failed");
var mainMenu = window.FindControl<Menu>("MainMenu")!;
var view = (MenuItem)mainMenu.Items[1]!;
view.Items.OfType<MenuItem>().Single(i => (string?)i.Header == "ASN.1 Modules")
    .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
var modules = window.OwnedWindows.Single();
Check(modules.FindControl<TreeView>("ModuleTree")!.Items.Count > 0, "Schema module window is empty");
modules.Close();
var help = (MenuItem)mainMenu.Items[2]!;
var languages = (MenuItem)help.Items[0]!;
languages.Items.OfType<MenuItem>().Single(i => (string?)i.Header == "Chinese")
    .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
Check((string?)window.FindControl<TabItem>("InputTab")!.Header == "输入文本", "Shared Chinese labels not loaded");
var about = new About();
var aboutClosed = about.ShowDialog(window);
Check(about.FindControl<TextBlock>("ProductName")!.Text!.Contains("ASN.1"), "About window product metadata missing");
about.FindControl<Button>("OK")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
PumpUntil(() => aboutClosed.IsCompleted);
aboutClosed.GetAwaiter().GetResult();
var confirmation = MsgBox.Show(window, "Test", "Message", "OK", "Cancel");
var message = (MsgBox)window.OwnedWindows.Single();
Check(message.Message == "Message" && message.CancelText == "Cancel", "MsgBox labels missing");
message.FindControl<Button>("CancelButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
PumpUntil(() => confirmation.IsCompleted);
Check(!confirmation.GetAwaiter().GetResult(), "MsgBox cancellation result incorrect");
var accepted = MsgBox.Show(window, "Test", "Message", "OK");
message = (MsgBox)window.OwnedWindows.Single();
Check(!message.FindControl<Button>("CancelButton")!.IsVisible, "One-button MsgBox must hide Cancel");
message.FindControl<Button>("OKButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
PumpUntil(() => accepted.IsCompleted);
Check(accepted.GetAwaiter().GetResult(), "MsgBox acceptance result incorrect");
window.Close();
Console.WriteLine("PASS Windows UI parity: menus, toolbar, icons, automatic input, selection, full-document hex, byte colors, BER, sample schema, live editing, language, About, MsgBox and error recovery");
