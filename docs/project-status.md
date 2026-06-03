# Project Status

This public status document tracks user-facing progress from the current MVP toward the next release.

## Current Version Target

Current preview target: `0.1.0-preview.1`

Goal: make the current tool installable, documented, and safe to share publicly.

## Completed For Preview

- .NET 10 target framework.
- Local CLI.
- GitHub Actions workflow scanning.
- Text and JSON output.
- Include/exclude filtering.
- `--fail-on` exit-code behavior, including `--fail-on none`.
- Strict mode behavior for selected rules.
- Initial cache rules.
- Sample good and bad workflows.
- README usage documentation.
- Changelog, contributing, security, code of conduct, roadmap, and release checklist.
- Rule, parser, repository context, reporter, and CLI tests.

## Remaining For Preview

- Verify CI on GitHub-hosted runners once the workflow runs remotely.
- Review README install instructions after the first package is published.
- Create the `v0.1.0-preview.1` GitHub release.

## Repository Protection

The repository is configured so public contribution should flow through pull requests:

- `main` is protected.
- The `build` GitHub Actions check is required before merge.
- Pull requests require at least one approving review.
- Code-owner review is required.
- `CODEOWNERS` assigns all files to `@Wezylnia`.
- Stale approvals are dismissed after new commits are pushed.
- The most recent push must be approved by someone other than the pusher.
- Conversation resolution is required before merge.
- Force pushes and branch deletion are disabled.
- Administrators are not locked out, so the maintainer keeps emergency bypass ability.
- GitHub Copilot is configured for automatic pull request review on pushes to PRs targeting `main`.

## Next After Preview

The next milestone is `v0.1.0`, focused on:

- broader parser edge-case coverage,
- source-location polish,
- more strict-mode checks,
- expanded repository context detection,
- first polished MVP release.

## Validation Commands

```bash
dotnet restore
dotnet build GhaCacheDoctor.slnx --configuration Release
dotnet test GhaCacheDoctor.slnx --configuration Release --no-build
dotnet pack src/GhaCacheDoctor.Cli --configuration Release --no-build
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --fail-on none
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --format json --fail-on none
```
