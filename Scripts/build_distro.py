"""build_distro.py

This script create a new tar distribution for Unity_TDRS.

"""

import json
import pathlib
import tarfile

DIST_PATH = pathlib.Path(__file__).parent.parent / "dist"
"""The path to place the tar archive."""

PACKAGE_ROOT = (
    pathlib.Path(__file__).parent.parent / "Packages" / "io.github.shijbey.calypso"
)
"""The path to the root folder of the package."""


def make_dist_directory() -> None:
    """Create a new dist directory if one does not exist."""

    DIST_PATH.mkdir(parents=True, exist_ok=True)


def get_package_version() -> str:
    """Get the package version from the package.json."""

    with open(PACKAGE_ROOT / "package.json", "r", encoding="utf-8") as file:
        package_data = json.load(file)
        return package_data["version"]


def create_package_tar(output_path: pathlib.Path) -> None:
    """Create a new tar archive at the given path using the package files."""

    with tarfile.open(output_path, "w:gz") as tar:
        tar.add(PACKAGE_ROOT, arcname="package")


def main() -> None:
    """Main entry function."""

    package_version = get_package_version()
    make_dist_directory()
    create_package_tar(DIST_PATH / f"calypso_{package_version}.tgz")


if __name__ == "__main__":
    main()
