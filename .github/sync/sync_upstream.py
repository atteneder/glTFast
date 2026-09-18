#!/usr/bin/env python3
"""Sync a released version of *Unity glTFast* into this *glTFast* (OpenUPM) repository.

The source is the `release/<version>` tag of the public Unity monorepo. Its package
folder replaces the content of this repository and the modifications that make it
*glTFast* are re-applied afterwards.

Every difference between the Unity package and this fork is expressed as an explicit
rule below. A rule that no longer matches aborts the sync with a message naming the
file, instead of silently producing a wrong result. When upstream rewords a patched
section, update the rule here -- that is the only maintenance this script needs.

Files outside of the package (`.github`, `.gitignore`, `.gitattributes`, ...) belong to
this repository and are never overwritten. Upstream changes to them are reported, so
they can be carried over by hand where that makes sense.

Upstream develops several versions in parallel, so each branch of this repository tracks
one release channel, recorded in its own `state.json`: `stable` for releases like 6.20.0
and `prerelease` for releases like 7.0.0-exp.1. A release of the other channel is only
synced when it is asked for explicitly.

Usage:
    python3 .github/sync/sync_upstream.py [--version 6.20.0] [--watch-report PATH]

Without --version the highest release of the branch's channel is used.
"""

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

SOURCE_REPO = "https://github.com/Unity-Technologies/com.unity.cloud.gltfast.git"
PACKAGE_DIR = "Packages/com.unity.cloud.gltfast"
TAG_PREFIX = "release/"

UNITY_NAME = "com.unity.cloud.gltfast"
OPENUPM_NAME = "com.atteneder.gltfast"

# Records the release channel of this branch and the release it was last synced from, so
# the next sync can report what changed in the files it does not touch.
STATE_PATH = ".github/sync/state.json"

STABLE = "stable"
PRERELEASE = "prerelease"

# Top level entries owned by this repository. They have no counterpart in the package
# folder and are never written to, but upstream changes to them are reported.
REPO_OWNED = {
    ".git",
    ".github",
    ".gitattributes",
    ".gitignore",
    "CODE_OF_CONDUCT.md",
    "CODE_OF_CONDUCT.md.meta",
    "CONTRIBUTING.md",
    "CONTRIBUTING.md.meta",
}

# Upstream counterparts of the repository owned entries, relative to the monorepo root.
WATCHED_PATHS = sorted(REPO_OWNED - {".git"})

AUTHOR = {
    "name": "Andreas Atteneder",
    "email": "andreas.atteneder@gmail.com",
    "url": "https://pixel.engineer",
}

# Only meaningful within the Unity monorepo.
DROP_PACKAGE_KEYS = ("relatedPackages", "documentationUrl", "repository")

OPENUPM_BADGE = (
    "[![openupm](https://img.shields.io/npm/v/com.atteneder.gltfast"
    "?label=openupm&registry_uri=https://package.openupm.com)]"
    "(https://openupm.com/packages/com.atteneder.gltfast/)"
)


class RuleError(Exception):
    """A patch rule did not match the upstream content anymore."""


def run(*command, cwd=None, env=None):
    return subprocess.run(
        command,
        cwd=cwd,
        env={**os.environ, **env} if env else None,
        check=True,
        capture_output=True,
        text=True,
    ).stdout


def expect(condition, message):
    if not condition:
        raise RuleError(message)


def replace_once(text, old, new, path):
    count = text.count(old)
    expect(count == 1, f"{path}: expected exactly one occurrence of {old!r}, found {count}")
    return text.replace(old, new)


def patch_package_json(text, path):
    data = json.loads(text)
    expect(data.get("name") == UNITY_NAME, f"{path}: unexpected name {data.get('name')!r}")
    expect(
        data.get("displayName") == "Unity glTFast",
        f"{path}: unexpected displayName {data.get('displayName')!r}",
    )

    result = {}
    for key, value in data.items():
        if key in DROP_PACKAGE_KEYS:
            continue
        if key == "dependencies":
            result["author"] = AUTHOR
        if key == "name":
            value = OPENUPM_NAME
        elif key == "displayName":
            value = "glTFast"
        result[key] = value

    expect("author" in result, f"{path}: no 'dependencies' key to anchor the author entry on")
    return json.dumps(result, indent=2, ensure_ascii=False) + "\n"


def patch_readme(text, path):
    expect(text.startswith("# Unity glTFast\n"), f"{path}: does not start with '# Unity glTFast'")
    return text.replace("# Unity glTFast\n", f"# glTFast\n\n{OPENUPM_BADGE}\n", 1)


def patch_license(text, path):
    text = replace_once(text, "Unity glTFast copyright", "glTFast copyright", path)
    return replace_once(text, "the Unity glTFast authors", "the glTFast authors", path)


def patch_gltf_globals(text, path):
    return replace_once(
        text,
        f'GltfPackageName = "{UNITY_NAME}"',
        f'GltfPackageName = "{OPENUPM_NAME}"',
        path,
    )


def patch_importer_uxml(text, path):
    return replace_once(
        text,
        f"/Packages/{UNITY_NAME}/Editor/UI/gltf-logo.png",
        f"/Packages/{OPENUPM_NAME}/Editor/UI/gltf-logo.png",
        path,
    )


# The fork is a package-only branch, so its documentation warns readers that development
# has to happen in the Unity monorepo. Both notes do not exist upstream.
DEVELOPMENT_NOTE = (
    "> [!CAUTION]\n"
    "> To do meaningful development you have to [switch to the Unity fork]"
    "(./UpgradeGuides#transition-to-unity-gltfast), as this repository/branch does not contain"
    " the tools, test and projects required for development. See [Download Sources]"
    "(./sources.md#download-sources) to learn where to obtain said fork.\n"
)

OPENUPM_BRANCH_NOTE = (
    "> [!IMPORTANT]\n"
    "> Unlike the Unity fork, the `openupm` branch does not contain the full [content of the"
    " monorepo structure](./UpgradeGuides.md#repository-structure-monorepo)! It contains just a"
    " slightly modified variant of the package itself without tests, tools or test projects and"
    " is not recommended for [development](./development.md).\n"
)


def insert_after_heading(note):
    """Insert `note` right below the level 1 heading of a document."""

    def rule(text, path):
        lines = text.splitlines(keepends=True)
        expect(bool(lines) and lines[0].startswith("# "), f"{path}: no level 1 heading on line 1")
        expect(len(lines) > 1 and not lines[1].strip(), f"{path}: no blank line below the heading")
        expect(note not in text, f"{path}: already contains the note")
        return "".join(lines[:2] + [note, "\n"] + lines[2:])

    return rule


def insert_before_line(anchor, note):
    """Insert `note` above the single line starting with `anchor`."""

    def rule(text, path):
        lines = text.splitlines(keepends=True)
        matches = [i for i, line in enumerate(lines) if line.startswith(anchor)]
        expect(
            len(matches) == 1,
            f"{path}: expected exactly one line starting with {anchor!r}, found {len(matches)}",
        )
        expect(note not in text, f"{path}: already contains the note")
        return "".join(lines[: matches[0]] + [note, "\n"] + lines[matches[0] :])

    return rule


# Relative path -> rule. Every one of these has to apply, otherwise the sync fails.
PATCH_RULES = {
    "package.json": patch_package_json,
    "README.md": patch_readme,
    "LICENSE.md": patch_license,
    "Runtime/Scripts/GltfGlobals.cs": patch_gltf_globals,
    "Editor/UI/GltfImporter.uxml": patch_importer_uxml,
    "Documentation~/Original.md": insert_before_line(
        "The package identifier in the `main` branch was changed", OPENUPM_BRANCH_NOTE
    ),
    "Documentation~/development.md": insert_after_heading(DEVELOPMENT_NOTE),
    "Documentation~/test-project-setup.md": insert_after_heading(DEVELOPMENT_NOTE),
    "Documentation~/tests.md": insert_after_heading(DEVELOPMENT_NOTE),
}


def version_key(version):
    match = re.fullmatch(r"(\d+)\.(\d+)\.(\d+)(?:-(.+))?", version)
    if not match:
        return None
    major, minor, patch, prerelease = match.groups()
    # Stable releases sort after their own prereleases.
    return int(major), int(minor), int(patch), prerelease is None, prerelease or ""


def released_versions():
    listing = run("git", "ls-remote", "--tags", "--refs", SOURCE_REPO, f"refs/tags/{TAG_PREFIX}*")
    versions = set()
    for line in listing.splitlines():
        version = line.split(f"refs/tags/{TAG_PREFIX}", 1)[-1].strip()
        if version_key(version):
            versions.add(version)
    if not versions:
        sys.exit(f"No {TAG_PREFIX}* tags found in {SOURCE_REPO}")
    return versions


def channel_of(version):
    return STABLE if version_key(version)[3] else PRERELEASE


def resolve_version(requested, channel):
    versions = released_versions()
    if requested:
        requested = requested.removeprefix(TAG_PREFIX)
        if requested not in versions:
            sys.exit(f"{TAG_PREFIX}{requested} is not tagged in {SOURCE_REPO}")
        if channel_of(requested) != channel:
            sys.exit(
                f"{requested} is a {channel_of(requested)} release, but this branch tracks "
                f"{channel} releases. Sync it on the branch for that channel, or change the "
                f"channel of this one with --channel."
            )
        return requested
    candidates = [version for version in versions if channel_of(version) == channel]
    if not candidates:
        sys.exit(f"No {channel} release tagged in {SOURCE_REPO}")
    return max(candidates, key=version_key)


def clone_source(version, previous, workdir):
    """Shallow clone the release tag, with the package's binary files resolved."""
    clone = os.path.join(workdir, "source")
    print(f"Cloning {SOURCE_REPO} at {TAG_PREFIX}{version}")
    run(
        "git",
        "clone",
        "--quiet",
        "--depth",
        "1",
        "--branch",
        f"{TAG_PREFIX}{version}",
        SOURCE_REPO,
        clone,
        # The monorepo carries hundreds of test assets in LFS. Only the handful inside
        # the package folder is needed, so they are pulled selectively below.
        env={"GIT_LFS_SKIP_SMUDGE": "1"},
    )
    run("git", "lfs", "pull", "--include", f"{PACKAGE_DIR}/**", cwd=clone)

    if previous and previous != version:
        try:
            run("git", "fetch", "--quiet", "--depth", "1", "origin", "tag", f"{TAG_PREFIX}{previous}", cwd=clone)
        except subprocess.CalledProcessError:
            print(f"Note: {TAG_PREFIX}{previous} is not available, skipping the upstream report")
            return clone, None
        return clone, previous
    return clone, None


def collect_source_files(package_root):
    files = []
    for directory, _, filenames in os.walk(package_root):
        for filename in filenames:
            path = os.path.join(directory, filename)
            files.append(os.path.relpath(path, package_root))
    return sorted(files)


def apply_patch_rules(package_root, source_files):
    errors = []
    for relative, rule in PATCH_RULES.items():
        if relative not in source_files:
            errors.append(f"{relative}: patched file is missing from the package")
            continue
        path = os.path.join(package_root, relative)
        with open(path, encoding="utf-8") as file:
            text = file.read()
        try:
            patched = rule(text, relative)
        except RuleError as error:
            errors.append(str(error))
            continue
        with open(path, "w", encoding="utf-8", newline="") as file:
            file.write(patched)
    if errors:
        sys.exit(
            "The OpenUPM patch rules no longer match the Unity package:\n  - "
            + "\n  - ".join(errors)
            + "\n\nUpdate the rules in .github/sync/sync_upstream.py and run again."
        )


def tracked_files(repo):
    output = run("git", "-C", repo, "ls-files", "-z")
    return {path for path in output.split("\0") if path}


def sync_tree(package_root, source_files, repo):
    # Removals run first so a rename that only changes the case of a file name survives
    # on case insensitive file systems.
    source_set = set(source_files)
    removed = []
    for relative in sorted(tracked_files(repo)):
        if relative.split("/", 1)[0] in REPO_OWNED or relative in source_set:
            continue
        os.remove(os.path.join(repo, relative))
        removed.append(relative)

    for relative in source_files:
        target = os.path.join(repo, relative)
        os.makedirs(os.path.dirname(target) or repo, exist_ok=True)
        shutil.copy2(os.path.join(package_root, relative), target)

    for directory, subdirectories, filenames in os.walk(repo, topdown=False):
        if os.path.relpath(directory, repo).split("/", 1)[0] in REPO_OWNED:
            continue
        if directory != repo and not subdirectories and not filenames:
            os.rmdir(directory)

    return removed


def upstream_report(clone, previous, version):
    """Report upstream changes to the files this repository maintains itself."""
    compare = (
        "https://github.com/Unity-Technologies/com.unity.cloud.gltfast/compare/"
        f"{TAG_PREFIX}{previous}...{TAG_PREFIX}{version}"
    )
    def upstream_diff(*options):
        return run(
            "git",
            "diff",
            *options,
            f"{TAG_PREFIX}{previous}",
            f"{TAG_PREFIX}{version}",
            "--",
            *WATCHED_PATHS,
            cwd=clone,
        )

    heading = "### Files maintained by this repository\n\n"
    summary = upstream_diff("--stat")
    if not summary.strip():
        return (
            heading
            + f"Unchanged upstream between {TAG_PREFIX}{previous} and {TAG_PREFIX}{version}.\n"
        )

    diff = upstream_diff()
    limit = 20000
    if len(diff) > limit:
        diff = diff[:limit] + f"\n... truncated, see {compare}\n"

    return (
        heading
        + f"Upstream changed these between [{TAG_PREFIX}{previous} and "
        f"{TAG_PREFIX}{version}]({compare}). They are **not** part of this pull request. "
        "Carry over whatever applies to a package-only repository by hand.\n\n"
        "```\n" + summary + "```\n\n"
        "<details><summary>Upstream diff</summary>\n\n"
        "```diff\n" + diff + "```\n\n"
        "</details>\n"
    )


def read_state(repo):
    try:
        with open(os.path.join(repo, STATE_PATH), encoding="utf-8") as file:
            return json.load(file)
    except FileNotFoundError:
        return {}


def write_state(repo, version, channel):
    path = os.path.join(repo, STATE_PATH)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    state = {
        "comment": "Release channel of this branch and the release it was last synced from."
        " Maintained by sync_upstream.py.",
        "channel": channel,
        "upstream_version": version,
    }
    with open(path, "w", encoding="utf-8") as file:
        json.dump(state, file, indent=2)
        file.write("\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version", help="released version to sync, defaults to the latest one")
    parser.add_argument(
        "--channel",
        choices=(STABLE, PRERELEASE),
        help="release channel this branch tracks, defaults to the one in state.json",
    )
    parser.add_argument("--repo", help="repository to sync into, defaults to the enclosing repo")
    parser.add_argument("--watch-report", help="write the upstream report to this file")
    arguments = parser.parse_args()

    repo = arguments.repo or run("git", "rev-parse", "--show-toplevel").strip()
    state = read_state(repo)
    channel = arguments.channel or state.get("channel", STABLE)
    version = resolve_version(arguments.version, channel)
    previous = state.get("upstream_version")
    print(f"Syncing {UNITY_NAME} {version} ({channel}) into {repo}")

    with tempfile.TemporaryDirectory() as workdir:
        clone, previous = clone_source(version, previous, workdir)
        package_root = os.path.join(clone, PACKAGE_DIR)
        if not os.path.isdir(package_root):
            sys.exit(f"{PACKAGE_DIR} does not exist in {TAG_PREFIX}{version}")

        source_files = collect_source_files(package_root)
        apply_patch_rules(package_root, source_files)
        removed = sync_tree(package_root, source_files, repo)
        report = upstream_report(clone, previous, version) if previous else ""

    write_state(repo, version, channel)

    for relative in removed:
        print(f"Removed {relative}")
    print(f"Synced {len(source_files)} files")
    if report:
        print(report)
    if arguments.watch_report:
        with open(arguments.watch_report, "w", encoding="utf-8") as file:
            file.write(report)

    output = os.environ.get("GITHUB_OUTPUT")
    if output:
        with open(output, "a", encoding="utf-8") as file:
            file.write(f"version={version}\nchannel={channel}\n")


if __name__ == "__main__":
    main()
