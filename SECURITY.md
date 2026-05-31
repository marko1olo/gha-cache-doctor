# Security Policy

## Supported Versions

`gha-cache-doctor` is currently in preview. Security issues are handled on a best-effort basis until the first stable release.

| Version | Supported |
| --- | --- |
| `0.1.0-preview.1` | Best effort |

## Reporting a Vulnerability

Please do not open a public issue for secrets, exploit details, or sensitive vulnerability information.

Report security concerns through GitHub using the maintainer contact options available on the repository. Include enough detail to reproduce the issue, but avoid sharing secrets or private workflow contents.

## Scope

`gha-cache-doctor` is a static analysis tool. It reads repository files and GitHub Actions workflow YAML. It does not execute workflows, run workflow steps, contact the GitHub API, or make network calls during analysis.
