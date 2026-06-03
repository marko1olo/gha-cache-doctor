# Copilot Instructions

This repository contains `gha-cache-doctor`, a focused .NET CLI for static GitHub Actions cache analysis.

When reviewing or generating changes:

- Keep analysis static. Do not execute workflow steps, package installs, Docker builds, or arbitrary repository code.
- Prefer small rule classes with focused tests.
- Every new rule should have rule docs under `docs/rules/` and sample workflows when useful.
- Keep findings practical and cautious. Avoid claiming certainty when a rule is heuristic.
- Do not include real tokens, webhook URLs, credentials, or private workflow content in tests or docs.
- Preserve deterministic text and JSON output for CI usage.
- Update README rule tables when adding a user-visible rule.
