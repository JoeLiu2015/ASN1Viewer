"""Build, test, package, and copy ASN1Viewer to a new destination directory."""

import argparse
from datetime import datetime
import os
import platform
import plistlib
import re
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile


ROOT = Path(__file__).resolve().parent


def find_msbuild(explicit=None):
    candidate = shutil.which(explicit or "MSBuild.exe")
    if explicit and not candidate:
        raise FileNotFoundError("Specified MSBuild not found: " + explicit)
    if not candidate:
        vswhere = Path(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")) / (
            "Microsoft Visual Studio/Installer/vswhere.exe"
        )
        if vswhere.is_file():
            result = subprocess.run(
                [str(vswhere), "-products", "*", "-requires",
                 "Microsoft.Component.MSBuild", "-find", r"MSBuild\**\Bin\MSBuild.exe"],
                check=True, stdout=subprocess.PIPE, encoding="utf-8",
            )
            candidate = next((line for line in result.stdout.splitlines()
                              if Path(line).is_file()), None)
    if not candidate:
        raise FileNotFoundError(
            "MSBuild not found. Install Visual Studio Build Tools "
            "or add MSBuild.exe's directory to PATH."
        )
    candidate = str(Path(candidate).resolve())
    print("MSBuild ({}): {}".format(
        "specified" if explicit else "auto-selected", candidate), flush=True)
    return candidate


def run(command):
    print("> " + subprocess.list2cmdline([str(arg) for arg in command]), flush=True)
    subprocess.run([str(arg) for arg in command], cwd=str(ROOT), check=True)


def build_windows(destination, msbuild=None):
    run([find_msbuild(msbuild), ROOT / "ASN1Viewer.sln", "/t:Rebuild",
         "/p:Configuration=Release", "/p:Platform=Any CPU", "/nologo", "/v:minimal"])
    run([ROOT / "ASN1Viewer.Tests/bin/Release/ASN1Viewer.Tests.exe"])

    output = ROOT / "ASN1Viewer/bin/Release"
    files = {
        Path("ASN1Viewer.exe"): output / "ASN1Viewer.exe",
        Path("ASN1Viewer.Core.dll"): output / "ASN1Viewer.Core.dll",
        # Use the repository defaults, never a config modified by running the app.
        Path("ASN1Viewer.exe.config"): ROOT / "ASN1Viewer/app.config",
        Path("LICENSE"): ROOT / "LICENSE",
    }
    for directory, source in (
        ("Asn1Modules", ROOT / "ASN1Viewer.Core/files/Asn1Modules"),
        ("TestFiles", ROOT / "ASN1Viewer/files/TestFiles"),
    ):
        resources = sorted(path for path in source.rglob("*") if path.is_file())
        if not resources:
            raise FileNotFoundError("Missing release resources: " + str(source))
        for path in resources:
            files[Path("files") / directory / path.relative_to(source)] = path
    for source in files.values():
        if not source.is_file():
            raise FileNotFoundError("Missing release file: " + str(source))

    package = ROOT / "bin/publish" / datetime.now().strftime("%Y%m%d-%H%M%S-%f") / "ASN1Viewer"
    package.mkdir(parents=True)
    for relative, source in files.items():
        target = package / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(str(source), str(target))
    # copytree refuses existing destinations, including ones created during the build.
    shutil.copytree(str(package), str(destination))
    print("Packaged {} files: {}".format(len(files), package))
    print("Copied release to: " + str(destination))
    return 0


def find_dotnet(explicit=None):
    requested = explicit or os.environ.get("DOTNET")
    candidate = shutil.which(requested or "dotnet")
    if not candidate and not requested:
        local = Path.home() / ".dotnet/dotnet"
        if local.is_file():
            candidate = str(local)
    if not candidate:
        raise FileNotFoundError(".NET SDK not found. Install the .NET 8 SDK or specify --dotnet.")
    return str(Path(candidate).resolve())


def build_macos_icon(source, destination):
    # Apple's iconset names distinguish logical sizes from Retina pixel sizes.
    with tempfile.TemporaryDirectory(prefix="asn1viewer-icon-") as temporary:
        iconset = Path(temporary) / "ASN1Viewer.iconset"
        iconset.mkdir()
        for size in (16, 32, 128, 256, 512):
            for scale in (1, 2):
                suffix = "@2x" if scale == 2 else ""
                target = iconset / ("icon_{}x{}{}.png".format(size, size, suffix))
                run(["sips", "-z", size * scale, size * scale, source, "--out", target])
        run(["iconutil", "--convert", "icns", "--output", destination, iconset])


def write_macos_plist(destination):
    assembly_info = (ROOT / "ASN1Viewer/Properties/AssemblyInfo.cs").read_text(encoding="utf-8-sig")
    match = re.search(r'^\s*\[assembly:\s*AssemblyVersion\("(\d+\.\d+\.\d+)\.\d+"\)\]',
                      assembly_info, re.MULTILINE)
    if not match:
        raise ValueError("AssemblyInfo.cs must define a four-part numeric AssemblyVersion")
    with (ROOT / "ASN1Viewer.macOS/Info.plist").open("rb") as source:
        metadata = plistlib.load(source)
    # macOS bundle versions use three components rather than .NET's four.
    metadata["CFBundleVersion"] = match.group(1)
    metadata["CFBundleShortVersionString"] = match.group(1)
    with destination.open("wb") as output:
        plistlib.dump(metadata, output, sort_keys=False)


def build_macos(destination, runtime, dotnet=None):
    if sys.platform != "darwin":
        raise OSError("macOS packaging requires a Mac for code signing.")
    sdk = find_dotnet(dotnet)
    project = ROOT / "ASN1Viewer.macOS"
    for test in ("Tests/ASN1Viewer.Core.Tests.Modern.csproj",
                 "UITests/ASN1Viewer.macOS.UITests.csproj"):
        run([sdk, "run", "--project", project / test, "-c", "Release"])

    package = (ROOT / "bin/publish" / datetime.now().strftime("%Y%m%d-%H%M%S-%f")
               / runtime)
    app = package / "ASN1Viewer.app"
    executable_dir = app / "Contents/MacOS"
    resources = app / "Contents/Resources"
    resources.mkdir(parents=True)
    run([sdk, "publish", project / "ASN1Viewer.macOS.csproj", "-c", "Release",
         "-r", runtime, "--self-contained", "true", "-p:UseAppHost=true",
         "-o", executable_dir])
    write_macos_plist(app / "Contents/Info.plist")
    shutil.copy2(ROOT / "LICENSE", resources / "LICENSE")
    build_macos_icon(project / "files/Icons/AppIcon.png", resources / "ASN1Viewer.icns")
    executable = executable_dir / "ASN1Viewer.macOS"
    executable.chmod(executable.stat().st_mode | 0o111)
    run(["codesign", "--force", "--deep", "--sign", "-", app])
    run(["codesign", "--verify", "--deep", "--strict", app])
    # Refuse existing destinations, then preserve macOS resource forks and
    # extended attributes (including signatures on non-Mach-O libraries).
    destination.mkdir(parents=True, exist_ok=False)
    run(["ditto", package, destination])
    run(["codesign", "--verify", "--deep", "--strict", destination / "ASN1Viewer.app"])
    print("Packaged app: " + str(app))
    print("Copied release to: " + str(destination))
    return 0


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("destination", type=Path, help="New directory for the complete release")
    parser.add_argument("--platform", choices=("windows", "macos"),
                        default="macos" if sys.platform == "darwin" else "windows",
                        help="Release platform (default: current host)")
    parser.add_argument("--msbuild", help="Use this MSBuild.exe instead of automatic discovery")
    parser.add_argument("--dotnet", help="Use this dotnet executable (also accepts DOTNET environment variable)")
    parser.add_argument("--runtime", choices=("osx-arm64", "osx-x64"),
                        help="macOS architecture (default: current host architecture)")
    args = parser.parse_args()
    if args.platform == "windows" and (args.runtime or args.dotnet):
        parser.error("--runtime and --dotnet are only valid for --platform macos")
    if args.platform == "macos" and args.msbuild:
        parser.error("--msbuild is only valid for --platform windows")
    destination = args.destination.expanduser().resolve()
    if destination.exists():
        raise FileExistsError("Destination already exists; choose a new directory: " + str(destination))
    if args.platform == "macos":
        runtime = args.runtime
        if not runtime:
            runtime = {"arm64": "osx-arm64", "aarch64": "osx-arm64",
                       "x86_64": "osx-x64", "amd64": "osx-x64"}.get(platform.machine().lower())
            if not runtime:
                parser.error("Unknown host architecture; specify --runtime")
        return build_macos(destination, runtime, args.dotnet)
    return build_windows(destination, args.msbuild)


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, subprocess.CalledProcessError) as error:
        print("Release failed: " + str(error), file=sys.stderr)
        sys.exit(1)
