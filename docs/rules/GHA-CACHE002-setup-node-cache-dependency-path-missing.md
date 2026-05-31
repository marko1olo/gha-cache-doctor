# GHA-CACHE002 setup-node-cache-dependency-path-missing

## Summary

Reports setup-node caches in monorepo-like repositories when `cache-dependency-path` is missing.

## Why it matters

In monorepos, the lockfile used by a job may live outside the repository root. Without an explicit dependency path, cache keys can miss the intended lockfile or use the wrong one.

## How to fix

Point setup-node at the lockfile used by the job:

```yaml
- uses: actions/setup-node@v4
  with:
    cache: npm
    cache-dependency-path: apps/web/package-lock.json
```

## Examples

### Bad

```yaml
- uses: actions/setup-node@v4
  with:
    cache: npm
```

### Good

```yaml
- uses: actions/setup-node@v4
  with:
    cache: npm
    cache-dependency-path: apps/web/package-lock.json
```

## Default severity

warning

## Strict mode

No additional behavior in the MVP.

## False positive notes

The rule requires monorepo signals such as multiple package files, multiple Node lockfiles, or lockfiles under `apps/` or `packages/`.
