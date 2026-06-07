# GHA-CACHE002 setup-node-cache-dependency-path-missing

## Summary

Reports setup-node caches in monorepo-like repositories when `cache-dependency-path` is missing, including pnpm workspaces.

## Why it matters

In monorepos, the lockfile used by a job may live outside the repository root. Without an explicit dependency path, cache keys can miss the intended lockfile or use the wrong one.

For pnpm workspaces, an explicit dependency path also makes the workspace lockfile inputs clear when jobs are scoped to nested packages.

## How to fix

Point setup-node at the lockfile used by the job:

```yaml
- uses: actions/setup-node@v4
  with:
    cache: npm
    cache-dependency-path: apps/web/package-lock.json
```

For pnpm workspaces, include the workspace lockfiles that can affect the install:

```yaml
- uses: actions/setup-node@v4
  with:
    cache: pnpm
    cache-dependency-path: |
      pnpm-lock.yaml
      apps/*/pnpm-lock.yaml
      packages/*/pnpm-lock.yaml
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
For pnpm, the presence of `pnpm-workspace.yaml` is also treated as a monorepo signal.
