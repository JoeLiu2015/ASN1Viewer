using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ASN1Viewer.macOS.ui;

public partial class About : Window
{
    public About() : this(new Lang()) { }
    public About(Lang lang)
    {
        InitializeComponent();
        Title = string.Format(lang["ABOUT_TITLE"], lang["PROD_NAME"]);
        ProductName.Text = lang["PROD_NAME"];
        Copyright.Text = string.Format(lang["ABOUT_COPYRIGHT"], DateTime.Now.Year);
        Build.Text = string.Format(lang["ABOUT_VERSION"], AssemblyVersion);
        BuildDate.Text = string.Format(lang["ABOUT_BUILD_DATE"], DesktopUtils.GetAssembyBuildTime().ToString("yyyy-MM-dd"));
        OK.Content = lang["BTN_OK"];
    }
    public string AssemblyVersion => DesktopUtils.ApplicationVersion;
    private void CloseClicked(object? sender, RoutedEventArgs e) => Close();
}
