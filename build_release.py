"""Build, test, package, and copy ASN1Viewer to a new destination directory."""

import argparse
from datetime import datetime
import os
from pathlib import Path
import shutil
import subprocess
import sys


ROOT = Path(__file__).resolve().parent


def find_msbuild(explicit):
    if explicit:
        candidate = shutil.which(explicit)
        if candidate:
            return candidate
        raise FileNotFoundError("MSBuild not found: " + explicit)
    candidate = shutil.which("MSBuild.exe")
    if candidate:
        return candidate
    vswhere = Path(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")) / (
        "Microsoft Visual Studio/Installer/vswhere.exe"
    )
    if vswhere.is_file():
        result = subprocess.run(
            [str(vswhere), "-latest", "-products", "*", "-requires",
             "Microsoft.Component.MSBuild", "-find", r"MSBuild\**\Bin\MSBuild.exe"],
            check=True, stdout=subprocess.PIPE, encoding="utf-8",
        )
        for line in result.stdout.splitlines():
            if Path(line).is_file():
                return line
    raise FileNotFoundError(
        "MSBuild not found. Use a Visual Studio Developer Command Prompt "
        "or pass --msbuild PATH."
    )


def run(command):
    print("> " + subprocess.list2cmdline([str(arg) for arg in command]), flush=True)
    subprocess.run([str(arg) for arg in command], cwd=str(ROOT), check=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("destination", type=Path, help="New directory for the complete release")
    parser.add_argument("--msbuild", help="Optional path to MSBuild.exe")
    args = parser.parse_args()
    destination = args.destination.expanduser().resolve()
    if destination.exists():
        raise FileExistsError("Destination already exists; choose a new directory: " + str(destination))

    run([find_msbuild(args.msbuild), ROOT / "ASN1Viewer.sln", "/t:Rebuild",
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


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, subprocess.CalledProcessError) as error:
        print("Release failed: " + str(error), file=sys.stderr)
        sys.exit(1)
