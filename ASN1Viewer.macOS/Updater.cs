using Avalonia.Controls;
using ASN1Viewer.macOS.ui;

namespace ASN1Viewer.macOS;

internal static class Updater
{
    // MainForm.CheckUpdate in the Windows application currently returns immediately.
    public static async void ShowStatus(Window owner, Lang lang) => await MsgBox.Show(owner,
        lang["MENU_CHECKVER"], lang.Language == "zh_CN"
            ? "与 Windows 版一样，自动更新目前已停用。"
            : "Automatic updates are currently disabled, as in the Windows version.", lang["BTN_OK"]);
}
