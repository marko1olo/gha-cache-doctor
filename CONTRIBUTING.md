# Contributing

Thanks for considering a contribution to `gha-cache-doctor`.

The project is intentionally focused and rule-based. The best contributions are small, well-tested changes that improve GitHub Actions cache analysis without making the tool heavy.

## Development Setup

Requirements:

- .NET SDK 10

Restore, build, and test:

```bash
dotnet restore
dotnet build GhaCacheDoctor.slnx
dotnet test GhaCacheDoctor.slnx
```

Run the CLI locally:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --fail-on none
```

## Contribution Guidelines

- Keep changes small and focused.
- Add or update tests for behavior changes.
- Prefer a new rule class for a new rule.
- Keep rules deterministic and static-analysis only.
- Avoid network calls in core analysis.
- Avoid auto-fixing workflows unless working on a future explicit `fix` command.
- Update public docs when behavior changes.
- Keep false positives low.
- Add sample workflows when useful.

## Adding a Rule

See [docs/contributing/adding-a-rule.md](docs/contributing/adding-a-rule.md).

A typical rule contribution should include:

- a new rule class,
- rule registration,
- focused tests,
- a rule document under `docs/rules`,
- README rule table updates if needed.

## Finding Something To Work On

The issue tracker is organized for external contributors:

- [`good first issue`](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22good%20first%20issue%22): small, well-bounded work.
- [`beginner friendly`](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22beginner%20friendly%22): approachable if you are new to the codebase.
- [`help wanted`](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22help%20wanted%22): open for outside contribution.
- [`up for grabs`](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22up%20for%20grabs%22): no one is actively working on it yet.
- [`rule`](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3Arule): cache analysis rules.
- [`output`](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3Aoutput): reporters and output formats.

If you want to work on an issue, leave a short comment with your intended approach. Small pull requests are easiest to review and merge.

## Test Expectations

Add tests for:

- positive detection,
- false-positive avoidance,
- reporter output when output behavior changes,
- CLI behavior when command-line options change.

## Docs Expectations

Update user-facing docs when a behavior, rule, severity, option, or output field changes.

## Commit Style

Use clear commit messages such as:

```text
feat: add pnpm cache dependency path rule
fix: avoid duplicate setup-node cache findings
docs: document restore keys rule
test: add reporter tests for json output
```
