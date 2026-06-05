# GHA-CACHE003 actions-cache-key-missing-lockfile-hash

## Summary

Reports dependency cache keys that do not include a lockfile hash or known lockfile reference.

## Why it matters

Dependency caches should change when dependencies change. A static or overly broad key can restore stale dependency state.

## How to fix

Include a lockfile hash in the key:

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.npm
    key: ${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}
```

See the [Cache Key Cookbook](../cache-key-cookbook.md) for package-manager-specific before/after examples.

## Examples

### Bad

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.npm
    key: npm-cache
```

### Good

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.npm
    key: ${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}
```

## Default severity

warning

## Strict mode

No additional behavior in the MVP.

## False positive notes

The MVP only reports when the cached path clearly looks like a dependency cache directory.
