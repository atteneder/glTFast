# Upstream sync

[`sync_upstream.py`](sync_upstream.py) updates this branch to a released version of
*Unity glTFast* and re-applies the modifications that make it *glTFast*. The
[Sync upstream release](../workflows/sync-upstream.yml) workflow runs it and opens a pull
request for review.

## Source

The `release/<version>` tags of <https://github.com/Unity-Technologies/com.unity.cloud.gltfast>
are the source. `Packages/com.unity.cloud.gltfast` of the tagged commit replaces the content of
this repository, so files removed upstream are removed here as well.

The monorepo keeps hundreds of test assets in Git LFS, so the clone skips LFS content and pulls
only the objects inside the package folder afterwards.

## Release channels

Upstream develops several versions in parallel, currently 6.x on `develop` and 7.x on
`preview`, and tags releases of both. Each branch here tracks one channel, recorded in its own
[`state.json`](state.json):

| Branch | Channel | Syncs releases like |
| --- | --- | --- |
| `openupm` | `stable` | 6.20.0 |
| `openupm-preview` | `prerelease` | 7.0.0-exp.1 |

Without a version input the workflow picks the highest release of its branch's channel. A
release of the other channel is refused unless it is asked for explicitly *and* the branch's
channel is changed, which keeps a preview out of the stable branch by accident. Once 7.0.0
ships as a stable release, the `openupm` branch picks it up on its own and the preview branch
moves on to the next prerelease.

Since the patch rules live in `.github`, which each branch maintains itself, a channel whose
upstream needs different rules can diverge without affecting the other.

Both branches are tagged the same way, `com.atteneder.gltfast/<version>`. OpenUPM discovers
tags repository-wide rather than per branch, so a prerelease tag is picked up and published as
a prerelease version without further configuration. If previews should stay off OpenUPM
entirely, that is what its `gitTagIgnore` setting is for.

### Adding the preview branch

```sh
git switch --create openupm-preview openupm
git push --set-upstream origin openupm-preview
```

Then run the workflow once on that branch with `--channel prerelease`, or set `"channel":
"prerelease"` in its `state.json` beforehand. Leaving `upstream_version` at the release the
branch was cut from makes the first sync report everything that changed upstream since then.

## Patch rules

`PATCH_RULES` maps a file to the modification this fork applies to it: the package identifier
and author in `package.json`, the *glTFast* naming in `README.md`, `LICENSE.md`,
`GltfGlobals.cs` and `GltfImporter.uxml`, and the notes in `Documentation~` that point readers
towards Unity's fork for development.

Each rule is anchored on the upstream text it expects. When upstream rewords a patched section,
the rule stops matching and the sync aborts naming the file, instead of silently producing a
wrong result. Adjust the rule in that case and run the workflow again.

This replaces the older approach of storing a `git diff` per release and applying it to the next
one, which needed manual conflict resolution on nearly every release.

## Files maintained by this repository

`REPO_OWNED` lists the entries that live outside of the package folder and are maintained here:
`.github`, `.gitignore`, `.gitattributes`, `CODE_OF_CONDUCT.md` and `CONTRIBUTING.md` (including
their meta files). They are never overwritten, because most of what the monorepo keeps in them
is meaningless for a package-only repository.

Upstream changes to them still have to be noticed, so every sync diffs them between the release
it last synced from ([`state.json`](state.json)) and the one being synced, and puts the result
into the pull request. Carrying a change over is a manual decision.

## Running it locally

```sh
python3 .github/sync/sync_upstream.py               # latest release of this branch's channel
python3 .github/sync/sync_upstream.py --version 6.20.0
python3 .github/sync/sync_upstream.py --channel prerelease
```

It writes into the enclosing repository and updates `state.json`, so run it on a clean working
tree and review `git status` afterwards.

## Automating the trigger

The workflow is idempotent: without a version input it syncs the latest release of its
branch's channel and does nothing when the branch is already up to date. Upstream releases
cannot notify this repository, so the way to pick them up automatically is a `schedule:`
trigger in [`sync-upstream.yml`](../workflows/sync-upstream.yml) that polls for new tags. A
scheduled run uses the default branch, so the preview branch needs its own schedule entry
with an explicit checkout of that branch.
