# GHA-CACHE001 setup-node-cache-missing

## Summary

Reports `actions/setup-node` steps that omit the built-in `cache` input while the job appears to install Node dependencies.

## Why it matters

Without dependency caching, every workflow run may download the same npm, yarn, or pnpm packages again.

## How to fix

Add the package-manager-specific cache input:

```yaml
- uses: actions/setup-node@v4
  with:
    node-version: 22
    cache: npm
```

## Examples

### Bad

```yaml
- uses: actions/setup-node@v4
- run: npm ci
```

### Good

```yaml
- uses: actions/setup-node@v4
  with:
    cache: npm
- run: npm ci
```

## Default severity

info

## Strict mode

No additional behavior in the MVP.

## False positive notes

The rule skips jobs that already use setup-node caching or an `actions/cache` Node dependency cache.
