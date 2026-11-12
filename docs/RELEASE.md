# Releasing to NuGet

[Package usage](../README.md) · [Contributor guide](CONTRIBUTING.md)

GitHub Actions builds and tests pull requests and pushes to `master` on Windows,
Linux, and macOS. Tag pushes do not trigger a separate build, and feature branches
are tested through their pull requests rather than both push and PR events.
The Linux job saves the NuGet package as the `nuget-build` workflow artifact.
Publishing a GitHub release triggers `.github/workflows/release.yml`, which reruns
the full build/test matrix before packing and publishing to NuGet.org. Ordinary
pushes and pull requests do not publish packages.

## One-time publishing setup

1. Configure [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
   for repository owner `ShawInnes`, repository `SshKeyGenerator`, and workflow
   filename `release.yml` (without the `.github/workflows/` prefix). Leave the
   environment field empty; the workflow does not use a GitHub environment.
   Scope publishing to the `SshKeyGenerator` package.
2. Set the GitHub repository Actions variable `NUGET_USER` to the NuGet.org
   username of the person who created the policy (not the package/policy owner
   organization or an email address). The workflow exchanges
   its GitHub OIDC token for a short-lived NuGet API key; no stored API key is needed.
3. Disable the old AppVeyor project/webhook and remove any required AppVeyor status
   check from branch protection if still configured. AppVeyor configuration and
   publishing have been removed from this repository.

## Publishing a release

For version `2.0.0`, merge the changes into `master`, then create and publish a
GitHub release targeting that commit with tag **`v2.0.0`**. The tag must equal
`v` followed by the package version in `src/SshKeyGenerator.csproj`; a mismatch
fails before publishing. Update `<Version>` in that project for subsequent releases.

The release workflow saves the package as the `nuget-release` artifact before
publishing. A rerun skips an already-published package version; NuGet versions
cannot be overwritten. This workflow does not create tags or GitHub releases.

## Release candidates and naming

The project version is the NuGet package version. The Git tag must have a `v`
prefix; select that same tag when creating the GitHub release. The release title
is a display name and does not affect the package version.

| Release | Project `<Version>` | Git tag / GitHub release tag |
| --- | --- | --- |
| Release candidate | `2.0.0-rc.1` | `v2.0.0-rc.1` |
| Stable | `2.0.0` | `v2.0.0` |

Mark release candidates as GitHub prereleases. Publish the GitHub release to
trigger the workflow; creating a tag or saving a draft release does not publish
the package.

## Troubleshooting trusted publishing

If NuGet login fails with HTTP 401 and a **workflow mismatch**, edit the trusted
publishing policy on NuGet.org. Its workflow filename must be exactly
**`release.yml`**, matching `.github/workflows/release.yml` in this repository.
`release.yaml` is a different filename and will be rejected. Do not include the
`.github/workflows/` prefix in the policy field.

After correcting the policy, use **Re-run failed jobs** on the existing GitHub
Actions release run. A policy-only correction does not require a new tag or
release. Renaming a workflow in a later commit will not change the filename used
by the existing failed run.

If authentication still fails, verify that `NUGET_USER` is the username of the
policy creator, and that the policy repository owner, repository name, and
environment match the workflow. The current workflow has no GitHub environment,
so that field must be empty.
