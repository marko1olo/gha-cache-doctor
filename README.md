# gha-cache-doctor

A GitHub Actions cache configuration linter and optimization advisor.

`gha-cache-doctor` scans workflow files for missing, weak, or inefficient cache configuration and suggests package-manager-aware fixes.

## Status

This repository contains the MVP implementation. It supports GitHub Actions workflows, text and JSON output, rule include/exclude filtering, `--fail-on`, and the first cache-quality rules.

## Build

```bash
dotnet build
dotnet test
```

## Usage

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan
```

By default, the scanner reads:

```text
.github/workflows/*.yml
.github/workflows/*.yaml
```

Common options:

```bash
dotnet run --project src/GhaCacheDoctor.Cli -- scan --repo .
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path .github/workflows/ci.yml
dotnet run --project src/GhaCacheDoctor.Cli -- scan --format json
dotnet run --project src/GhaCacheDoctor.Cli -- scan --fail-on warning
dotnet run --project src/GhaCacheDoctor.Cli -- scan --include GHA-CACHE001,GHA-CACHE003
dotnet run --project src/GhaCacheDoctor.Cli -- scan --exclude GHA-CACHE004
```

## Sample Output

```text
.github/workflows/ci.yml

[info] GHA-CACHE001 setup-node-cache-missing
Job: test
Step: Setup Node
actions/setup-node is used without dependency caching.
Recommendation: Add `cache: npm` to the setup-node `with` block.
```

## Exit Codes

```text
0 = no findings at or above the fail threshold
1 = findings exist at or above the fail threshold
2 = invalid CLI usage
3 = workflow parse error
```

Findings do not fail the command unless `--fail-on` is provided.

## Supported Rules

| Rule | Severity | Summary |
| --- | --- | --- |
| GHA-CACHE001 | info | `actions/setup-node` is used without dependency caching. |
| GHA-CACHE002 | warning | Monorepo-like Node repository uses setup-node cache without `cache-dependency-path`. |
| GHA-CACHE003 | warning | `actions/cache` dependency cache key does not include a lockfile hash. |
| GHA-CACHE004 | info | `restore-keys` are broad enough to restore unrelated caches. |
| GHA-CACHE005 | info | A job installs dependencies without a matching cache mechanism. |

Detailed rule docs live in [docs/rules](docs/rules).

## CI Usage

```yaml
- name: Check GitHub Actions cache configuration
  run: dotnet run --project src/GhaCacheDoctor.Cli -- scan --fail-on warning
```

## Design

The solution is split into focused projects:

```text
src/GhaCacheDoctor.Cli
src/GhaCacheDoctor.Core
src/GhaCacheDoctor.GitHubActions
src/GhaCacheDoctor.Reporters
```

The core project owns domain models and orchestration. GitHub Actions parsing and rules are isolated from CLI concerns. Reporters are small output adapters.

## Contributing

Keep rules deterministic, conservative, and package-manager-aware. Add focused tests for parser behavior, repository context detection, false-positive avoidance, and reporter output whenever a rule changes.
