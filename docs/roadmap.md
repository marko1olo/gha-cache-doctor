# Roadmap

This roadmap describes the public path from the current MVP to `v1.0.0`.

## v0.1.0-preview.1

Focus: package and documentation readiness.

- Pack as a .NET global tool.
- Document installation and quick start.
- Add release files.
- Add initial rule docs.
- Add sample workflows.
- Add parser, rule, CLI, and reporter tests.

## v0.1.0

Focus: polished MVP.

- Add more parser edge-case tests.
- Improve source locations where practical.
- Expand strict mode coverage.
- Expand repository context detection.
- Add more end-to-end CLI tests.

## v0.2.0

Focus: configuration support.

- Add `.gha-cache-doctor.yml`.
- Allow rule disabling.
- Allow severity overrides.
- Allow file/rule ignores.
- Add config parse validation.

## v0.3.0

Focus: CI integration.

- Add GitHub annotation output.
- Add official GitHub Action wrapper.
- Add Markdown report output.

## v0.4.0

Focus: SARIF output.

- Add `--format sarif`.
- Include rule metadata and source locations.
- Document GitHub code scanning usage.

## v0.5.0

Focus: broader cache coverage.

- Add dotnet restore cache rule.
- Add Python pip cache rule.
- Add Gradle cache rule.
- Add Docker BuildKit layer cache rule.

## v1.0.0

Focus: stable public release.

- Stable CLI.
- Stable JSON schema.
- Stable config schema.
- Documented rules.
- CI-ready output formats.
- Reliable tests across supported platforms.
