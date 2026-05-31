# GHA-CACHE005 install-step-without-cache

## Summary

Reports jobs that install dependencies without a matching cache mechanism.

## Why it matters

Dependency installs are often among the slowest parts of CI. A nearby cache can reduce repeated downloads and runner minutes.

## How to fix

Use the package-manager-specific cache mechanism for the job, such as setup-node `cache` or `actions/cache` for NuGet, pip, or Gradle cache directories.

## Examples

### Bad

```yaml
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

The rule skips install commands that explicitly include `--no-cache`.
