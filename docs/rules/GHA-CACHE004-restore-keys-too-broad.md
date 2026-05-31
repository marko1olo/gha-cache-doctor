# GHA-CACHE004 restore-keys-too-broad

## Summary

Reports `actions/cache` restore keys that are broad enough to restore unrelated caches.

## Why it matters

Broad restore keys can make cache behavior harder to reason about, especially across package managers, operating systems, or projects in a monorepo.

## How to fix

Include runner OS, package manager, project path when relevant, and dependency context.

## Examples

### Bad

```yaml
restore-keys: |
  npm-
```

### Good

```yaml
restore-keys: |
  ${{ runner.os }}-npm-apps-web-
```

## Default severity

info

## Strict mode

Strict mode also reports additional package-manager-only restore key prefixes such as `pnpm-`, `yarn-`, `nuget-`, `pip-`, and `gradle-`.

## False positive notes

Broad restore keys are sometimes intentional, so this rule is informational.
