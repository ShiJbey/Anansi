#!/usr/bin/python3

"""Anansi package bundler script.

This Python script to facilitates creating package distributions.
Running it will create a `dist` directory and output the bundled Unity
package as a `.tgz` archive.

"""

import json
import pathlib
import re
import tarfile
from typing import Optional

PROJECT_ROOT = pathlib.Path(__file__).parent.parent
"""The path to the root folder of the entire Project."""

DIST_PATH = PROJECT_ROOT / "dist"
"""The path to place the tar archive."""

PACKAGE_ROOT = PROJECT_ROOT / "Packages" / "com.shijbey.anansi"
"""The path to the root folder of the package."""


def make_dist_directory() -> None:
    """Create a new dist directory if one does not exist."""

    DIST_PATH.mkdir(parents=True, exist_ok=True)


def get_package_version() -> str:
    """Get the package version from the package.json."""

    with open(PACKAGE_ROOT / "package.json", "r", encoding="utf-8") as file:
        package_data = json.load(file)
        return package_data["version"]


def copy_file(source: pathlib.Path, destination: pathlib.Path) -> None:
    """Copy the source file to the destination."""

    with open(source, "r", encoding="utf-8") as source_file:
        file_data = source_file.readlines()

    with open(destination, "w", encoding="utf-8") as destination_file:
        destination_file.writelines(file_data)

    print(f"{source} => {destination}")


def create_package_tar(output_path: pathlib.Path) -> None:
    """Create a new tar archive at the given path using the package files."""

    # Regular expressions for file paths to ignore
    excluded_paths: tuple[str, ...] = (
        r"^.*\.git.*$",
        r"^.*\.vscode.*$",
        r"^.*\.DS_Store.*$",
    )

    def tar_filter_fn(info: tarfile.TarInfo) -> Optional[tarfile.TarInfo]:
        """Filter tar entries using excluded paths"""

        if any(re.match(p, info.name) for p in excluded_paths):
            return None

        return info

    with tarfile.open(output_path, "w:gz") as tar:
        tar.add(PROJECT_ROOT / "README.md", arcname="package/README.md")
        tar.add(PROJECT_ROOT / "LICENSE.md", arcname="package/LICENSE.md")
        tar.add(PROJECT_ROOT / "CHANGELOG.md", arcname="package/CHANGELOG.md")

        tar.add(PACKAGE_ROOT, arcname="package", filter=tar_filter_fn)

        tar.list(False)


def main() -> None:
    """Main entry function."""

    package_version = get_package_version()

    print(f"=== Bundling Anansi version {package_version} ===")

    make_dist_directory()

    create_package_tar(DIST_PATH / f"anansi_{package_version}.tgz")

    print("=== Bundling complete! ===")
    print(DIST_PATH / f"anansi_{package_version}.tgz")


if __name__ == "__main__":
    main()
