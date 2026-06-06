# gha-cache-doctor

[![CI](https://github.com/Wezylnia/gha-cache-doctor/actions/workflows/ci.yml/badge.svg)](https://github.com/Wezylnia/gha-cache-doctor/actions/workflows/ci.yml)
[![GitHub release](https://img.shields.io/github/v/release/Wezylnia/gha-cache-doctor?include_prereleases)](https://github.com/Wezylnia/gha-cache-doctor/releases)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A focused .NET CLI that scans GitHub Actions workflows for cache misconfigurations, weak cache keys, and missed dependency-cache opportunities.

`gha-cache-doctor` is a polished MVP and intentionally small. If you like CI/CD tooling, static analysis, or shaving minutes off slow pipelines, there are good first issues ready for contributors.

## Try It In 30 Seconds

```bash
dotnet restore
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --fail-on none
```

JSON output:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --format json --fail-on none
```

## Why It Exists

GitHub Actions caching looks simple, but cache configuration is easy to get wrong. A workflow can install dependencies on every run, use a cache key that never invalidates correctly, or miss monorepo lockfiles entirely.

`gha-cache-doctor` focuses only on cache quality. It does not replace general workflow linters. Instead, it gives package-manager-aware findings and practical suggestions for improving CI cache behavior.

## Status

Current release: `0.2.0`

The project is ready for local usage and public contribution. The CLI, parser, reporters, strict-mode behavior, initial rules, tests, sample workflows, and contributor docs are in place. See [docs/project-status.md](docs/project-status.md) and [docs/roadmap.md](docs/roadmap.md).

Looking for a place to help? Start here:

- [Good first issues](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22good%20first%20issue%22)
- [Help wanted](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22help%20wanted%22)
- [Rule requests](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3Arule)

## Requirements

- .NET SDK 10

## Install

For local packaging:

```bash
dotnet pack src/GhaCacheDoctor.Cli --configuration Release
dotnet tool install --tool-path .tmp/tools gha-cache-doctor --version 0.2.0 --add-source src/GhaCacheDoctor.Cli/bin/Release
.tmp/tools/gha-cache-doctor scan --path samples/github-actions/bad --fail-on none
```

After a public package is published:

```bash
dotnet tool install --global gha-cache-doctor --version 0.2.0
gha-cache-doctor scan
```

## Quick Start

Scan the default workflow directory:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan
```

Scan a specific workflow directory:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path .github/workflows
```

Fail CI on warnings or errors:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --fail-on warning
```

## Example Output

```text
samples/github-actions/bad/weak-cache-key.yml

[warning] GHA-CACHE003 actions-cache-key-missing-lockfile-hash
Job: test
Step: Cache npm
actions/cache uses a dependency cache path, but the key does not include a lockfile hash.
Recommendation: Include a dependency lockfile hash, for example `${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}`.
```

## Rules

| Rule | Severity | Category | Description |
|---|---:|---|---|
| [`GHA-CACHE001`](docs/rules/GHA-CACHE001-setup-node-cache-missing.md) | info | performance | Reports `actions/setup-node` usage without dependency caching when Node installs are present. |
| [`GHA-CACHE002`](docs/rules/GHA-CACHE002-setup-node-cache-dependency-path-missing.md) | warning | performance | Reports `setup-node` cache usage without `cache-dependency-path` in likely monorepos. |
| [`GHA-CACHE003`](docs/rules/GHA-CACHE003-actions-cache-key-missing-lockfile-hash.md) | warning | correctness | Reports dependency caches whose keys do not include lockfile hashes. |
| [`GHA-CACHE004`](docs/rules/GHA-CACHE004-restore-keys-too-broad.md) | info | maintainability | Reports overly broad `restore-keys` that may restore unrelated caches. |
| [`GHA-CACHE005`](docs/rules/GHA-CACHE005-install-step-without-cache.md) | info | performance | Reports dependency install steps that appear to run without a matching cache. |
| [`GHA-CACHE006`](docs/rules/GHA-CACHE006-gradle-cache-missing.md) | info | performance | Reports Gradle build or test jobs that run before Gradle dependency caching is configured. |

Want to add the next rule? The rule system is intentionally simple: one small class, focused tests, one docs page, and a README table update. See [Adding a Rule](docs/contributing/adding-a-rule.md).

Need practical cache key patterns? See the [Cache Key Cookbook](docs/cache-key-cookbook.md) for before/after examples across common package managers and Docker BuildKit.

## CLI Reference

```text
gha-cache-doctor scan [options]

Options:
  --repo <path>             Repository root. Defaults to current directory.
  --path <path>             Workflow file or directory. Defaults to .github/workflows.
  --format <text|json>      Output format. Defaults to text.
  --fail-on <none|info|warning|error>
  --include <ids>           Comma-separated rule IDs to include.
  --exclude <ids>           Comma-separated rule IDs to exclude.
  --strict                  Enable stricter rule behavior.
  --config <path|none>      Config file. Defaults to .gha-cache-doctor.yml if present.
  -h, --help                Show help.
```

## Configuration

`gha-cache-doctor` automatically reads `.gha-cache-doctor.yml` or `.gha-cache-doctor.yaml` from the repository root when present. Use `--config <path>` to choose a file or `--config none` to disable config loading.

```yaml
path: .github/workflows
format: text
failOn: warning
strict: true
exclude:
  - GHA-CACHE004
severity:
  GHA-CACHE005: warning
```

CLI options take precedence over config values. See [Configuration](docs/configuration.md) for the full reference.

Exit codes:

```text
0 = no findings at or above the fail threshold
1 = findings exist at or above the fail threshold
2 = invalid CLI usage
3 = workflow parse error
```

## JSON Output

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --format json
```

The JSON schema is intentionally simple and stable for CI consumption:

```json
{
  "findings": [
    {
      "ruleId": "GHA-CACHE003",
      "severity": "warning",
      "category": "correctness",
      "message": "actions/cache uses a dependency cache path, but the key does not include a lockfile hash.",
      "recommendation": "Include a dependency lockfile hash, for example `${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}`.",
      "filePath": ".github/workflows/ci.yml",
      "line": 10,
      "jobId": "test",
      "stepName": "Cache npm"
    }
  ],
  "parseErrors": []
}
```

## GitHub Actions Usage

```yaml
- name: Check GitHub Actions cache configuration
  run: dotnet run --project src/GhaCacheDoctor.Cli -- scan --fail-on warning
```

Once installed as a tool:

```yaml
- name: Install gha-cache-doctor
  run: dotnet tool install --global gha-cache-doctor --version 0.2.0

- name: Check cache configuration
  run: gha-cache-doctor scan --fail-on warning
```

## Development

```bash
dotnet restore
dotnet build GhaCacheDoctor.slnx
dotnet test GhaCacheDoctor.slnx
```

Run the CLI locally:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --fail-on none
```

## Contributing

Contributions are very welcome. The project has a small surface area, clear rule boundaries, and deterministic tests, so it is a good place to contribute focused CI/CD tooling improvements.

Good contribution paths:

- Add a cache rule for a package manager you use.
- Improve monorepo detection and recommendations.
- Add reporter output such as SARIF or GitHub annotations.
- Add parser or false-positive tests from real workflows.
- Improve docs with before/after workflow examples.

Start with [CONTRIBUTING.md](CONTRIBUTING.md). For rule changes, see [docs/contributing/adding-a-rule.md](docs/contributing/adding-a-rule.md).

Open contribution queues:

- [Beginner friendly](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22beginner%20friendly%22)
- [Up for grabs](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22up%20for%20grabs%22)
- [High impact](https://github.com/Wezylnia/gha-cache-doctor/issues?q=is%3Aissue%20is%3Aopen%20label%3A%22high%20impact%22)

## Roadmap

- [Project status](docs/project-status.md)
- [Roadmap](docs/roadmap.md)
- [Release checklist](docs/release-checklist.md)
- [Rule catalog](docs/rules/README.md)

## Repository Governance

The public repository is set up for small, reviewable contributions. Issues are labeled for rule work, output formats, beginner-friendly tasks, and high-impact improvements. Pull requests should include tests and docs for user-visible behavior changes.

`main` is protected with required CI, code-owner review, stale-review dismissal, last-push approval, conversation resolution, and disabled force pushes/deletions. Repository admins keep emergency bypass ability. GitHub Copilot automatic PR review is enabled for pull requests targeting `main`.

Maintainer review is required before merge, and dependency update PRs should keep CI green before release work.

## License

MIT. See [LICENSE](LICENSE).
