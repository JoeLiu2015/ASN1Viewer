# ASN1Viewer

[![Build Status](https://github.com/JoeLiu2015/ASN1Viewer/actions/workflows/build.yml/badge.svg)](https://github.com/JoeLiu2015/ASN1Viewer/actions/workflows/build.yml)



A lightweight and fast ASN.1 data viewer, built in C# with Windows (WinForms) and macOS (Avalonia) interfaces. Parses ASN.1 data or files(*.pfx, .p7b, *.pem, *.cer, *.der, etc) and displays it in a tree view.

## Features
- 🧬 Parses DER-encoded, PEM-encoded, HEX-encoded ASN.1
- 🌳 Tree-view UI
- 🪟 Windows UI built with WinForms
- 🍎 macOS UI built with Avalonia
- 📦 Simple executable, no install needed

## Screenshots
![Run](https://github.com/JoeLiu2015/ASN1Viewer/blob/main/.github/imgs/Asn1Viewer.gif)

## Getting Started
1. Clone repo
2. Open in Visual Studio
3. Build and run
4. Open sample(Test) files
![TestFiles](https://github.com/JoeLiu2015/ASN1Viewer/blob/main/.github/imgs/Test%20Files.png)
5. There are 3 ways to open ASN.1 data(file)
  - By menue (File->Open)
  - Drag the file into the Form
  - Input PEM text or HEX text in the **"Input Text"** tab

  ![TreeView](https://github.com/JoeLiu2015/ASN1Viewer/blob/main/.github/imgs/Pfx.png)

## Use cases
- Inspect X.509 certificates
- Analyze PKCS structures
- Debug ASN.1-based protocols

## License
MIT


## macOS desktop UI

The `ASN1Viewer.macOS` application uses Avalonia and the existing C# parser and
schema sources. The original Windows solution continues to build with MSBuild.
The separate SDK projects require the **.NET 8 SDK** (or a newer SDK with the
.NET 8 runtime installed). No Xcode project or Mono installation is required.

```sh
dotnet run --project ASN1Viewer.macOS/ASN1Viewer.macOS.csproj
# Optionally open a file at startup:
dotnet run --project ASN1Viewer.macOS/ASN1Viewer.macOS.csproj -- /path/to/certificate.pem
```

The macOS main window follows the Windows layout: File/View/Help menus, the
original compact toolbar and node icons, a resizable structure tree on the left,
Input Text / ASN.1 Bytes tabs on the right, and node information in the status bar.
Typing PEM/Base64/HEX automatically parses after a short debounce. Selecting a
node scrolls the complete hex document to its offset and highlights Tag (red),
length (blue), content (green), and BER end markers in both HEX and ASCII columns.
Only visible rows are drawn; there is no 64 KiB preview truncation.

Right-click a node to expand/collapse, edit leaf content, copy text/bytes, or parse
embedded content. The menus include all bundled test files, persistent recent files, always-on-top, English/Chinese labels and the ASN.1 module viewer.
Language and always-on-top preferences are saved when the window closes.
Input remains limited to 16 MiB. Automatic updates are disabled, as in the original
Windows code. macOS retains its system window title bar and file dialogs.

Build a self-contained application bundle on macOS (macOS 12 or newer):

```sh
python3 build_release.py artifacts/osx-arm64 --runtime osx-arm64  # Apple Silicon
python3 build_release.py artifacts/osx-x64 --runtime osx-x64    # Intel
open artifacts/osx-arm64/ASN1Viewer.app
```

The bundle includes .NET, schemas and sample files; the destination Mac does not
need the SDK. Use `--dotnet /path/to/dotnet` or `DOTNET=/path/to/dotnet` if it is not on PATH.
The script selects macOS on a Mac and Windows elsewhere; `--platform macos` or
`--platform windows` selects it explicitly. Both paths build and test before
packaging, and require a new destination directory to avoid overwriting a release.
The existing Windows command remains `python build_release.py <destination>`. Builds are locally
ad-hoc signed. Public distribution requires Developer ID signing and notarization;
see the [Avalonia macOS deployment guide](https://docs.avaloniaui.net/docs/deployment/macos).
CI builds both architectures and uploads ZIP bundles.

The macOS app uses its own 1024×1024 icon at
`ASN1Viewer.macOS/files/Icons/AppIcon.png`. The Python release script generates
the standard 16–512 point iconset at 1× and 2× using `sips` and `iconutil`, and
packages `Contents/Resources/ASN1Viewer.icns` with `CFBundleIconFile` configured
for Finder, Dock and the application switcher. No separate shell script is needed.

Run the existing behavior baseline against the shared Core on modern .NET:

```sh
dotnet run --project ASN1Viewer.macOS/Tests/ASN1Viewer.Core.Tests.Modern.csproj -c Release
dotnet run --project ASN1Viewer.macOS/UITests/ASN1Viewer.macOS.UITests.csproj -c Release
```

### macOS source layout

The macOS frontend mirrors the Windows frontend's component boundaries. Avalonia
`.axaml` files replace WinForms `.Designer.cs`/`.resx` view definitions; window
logic lives in the matching `.axaml.cs` files.

| Windows frontend | macOS frontend |
| --- | --- |
| `MainForm.cs` + Designer/resx | `MainForm.axaml.cs` + `MainForm.axaml` |
| `ui/About.cs` + Designer/resx | `ui/About.axaml.cs` + `ui/About.axaml` |
| `ui/EditASN1Node.cs` + Designer/resx | `ui/EditASN1Node.axaml.cs` + `ui/EditASN1Node.axaml` |
| `ui/HexViewer.cs` | `ui/HexViewer.cs` (custom-drawn control) |
| `ui/MsgBox.cs` + Designer/resx | `ui/MsgBox.axaml.cs` + `ui/MsgBox.axaml` |
| `schema/SchemaDlg.cs` + Designer/resx | `schema/SchemaDlg.axaml.cs` + `schema/SchemaDlg.axaml` |
| `schema/SchemaTreeBuilder.cs` | `schema/SchemaTreeBuilder.cs` |
| `Config.cs`, `Lang.cs`, `DesktopUtils.cs`, `Updater.cs` | Matching files in the macOS project root |

Icons live in `files/Icons`; node PNGs were extracted from the original Windows
ImageList resource. The original language files are linked under `lang`, and
sample files are copied into `files/TestFiles` at build time. Parser and schema
model sources remain shared with `ASN1Viewer.Core`.
