# Security Review Checklist

Use this checklist for changes that touch scanning, workflow parsing, reporting, packaging, or CI.

## Input Handling

- Treat workflow files and repository contents as untrusted input.
- Do not execute workflow steps, package managers, shell scripts, Docker builds, or arbitrary repository code.
- Keep file reads bounded and deterministic.
- Avoid network calls in core analysis.

## Secret Safety

- Do not commit real tokens, webhook URLs, credentials, or private workflow content.
- Redact any sensitive-looking values in findings, logs, samples, and tests.
- Prefer synthetic test values that cannot be mistaken for real credentials by push protection.

## Workflow and CI Safety

- Keep GitHub Actions permissions least-privilege.
- Keep CI checks deterministic and fast.
- Avoid adding credentials to workflows unless the use case is documented and scoped.

## Reporting

- Keep JSON output stable for automation.
- Do not include absolute local paths unless the command explicitly requires them.
- Keep findings cautious when a rule is heuristic.
