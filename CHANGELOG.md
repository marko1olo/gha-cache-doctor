# Changelog

All notable changes to this project will be documented in this file.

The project follows preview releases until the first stable `1.0.0`.

## 0.1.0 - 2026-06-05

### Added

- Strict-mode behavior for broader restore-key and install-step cache checks.
- Repository context detection for `global.json`, `pyproject.toml`, Gradle files, and Docker Compose files.
- Direct unit coverage for `GHA-CACHE004` and `GHA-CACHE005`.
- Direct text and JSON reporter tests.

### Changed

- Promoted the package version from `0.1.0-preview.1` to `0.1.0`.
- Updated project status, roadmap, and install documentation for the polished MVP release.

## 0.1.0-preview.1 - 2026-05-31

### Added

- Initial .NET 10 CLI.
- GitHub Actions workflow discovery for `.github/workflows/*.yml` and `.github/workflows/*.yaml`.
- YAML parsing for workflow jobs and steps.
- Repository context detection for lockfiles and project files.
- Text and JSON report output.
- `scan` command with `--path`, `--format`, `--fail-on`, `--include`, `--exclude`, and `--strict`.
- Initial cache analysis rules:
  - `GHA-CACHE001` setup-node-cache-missing
  - `GHA-CACHE002` setup-node-cache-dependency-path-missing
  - `GHA-CACHE003` actions-cache-key-missing-lockfile-hash
  - `GHA-CACHE004` restore-keys-too-broad
  - `GHA-CACHE005` install-step-without-cache
- Sample good and bad workflows.
- Rule documentation.
- Unit tests for parser, repository context, rules, CLI, and reporters.

### Known Gaps

- SARIF output is not implemented yet.
- GitHub annotation output is not implemented yet.
- Official GitHub Action wrapper is not implemented yet.
- Configuration file support is not implemented yet.
- Auto-fix support is not implemented yet.
