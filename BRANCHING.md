# Branching

This page documents the branching strategy used by this repository.  Please read the documentation on [contributing to this repository](CONTRIBUTING.md) prior to submitting any contributions.

Whilst code can be downloaded from any public branch, it is strongly recommended that only [release](#General-releases) code is run in production environments.  [Prerelease code](#prereleases) can be run in test environments (e.g. to test a new feature).  It is recommended that code from the [default branch](#default-branch) is only used for creating new feature branches.

# Default branch

The default branch is always deemed to be most up-to-date, including changes merged in from other branches.  Whilst every effort is made to ensure that this code is adequately tested (via unit tests and manually), users should acknowledge that this code may be of lower quality than the releases.  Code from this branch is not recommended for production use.

# Feature branches

New features (extension methods, helper classes, etc.) should be made in a separate branch.  Unless otherwise marked, feature branches should be considered 'under active development' and may be broken or have known (even known and undocumented) issues.

Once complete (including adequate unit testing), a pull request should be made asking for the changes to be included into the repository's default branch.  Any accepted pull requests will be merged into the default branch as per the [contribution guidance](CONTRIBUTING.md).

Feature branches should be removed once the changes are merged into the default branch.

# Releases

Periodically, M-Files will organise an update of the [nuget package](https://www.nuget.org/packages/MFiles.VAF.Extensions) so that users can more easily test out the new features.  To do this, changes will be merged into the `prerelease` or `release` branch accordingly.

## Prereleases

Changes from the default branch will be periodically merged into the `prerelease` branch.  Code merged into this branch on GitHub will trigger a GitHub action that will publish the package to nuget.  Code from this branch should follow semantic versioning conventions and have a [suitable suffix](https://docs.microsoft.com/en-gb/nuget/concepts/package-versioning#pre-release-versions) to denote that it is a prerelease version.

*It is **not recommended** to use prerelease packages in production environments.  These packages are designed only for use in test or internal environments.*

## General releases

Once prerelease code has undergone community testing and commenting, changes will be merged into the `release` branch.  Code merged into this branch on GitHub will trigger a GitHub action that will publish the package to nuget. Code from this branch should follow semantic versioning conventions and should not contain any suffixes to mark it as pre-release.

*It is recommended to **only use general release packages in production environments**.*
