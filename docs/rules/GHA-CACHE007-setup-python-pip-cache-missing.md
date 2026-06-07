# GHA-CACHE007 setup-python-pip-cache-missing

## Summary

Reports `actions/setup-python` steps that omit pip dependency caching while the job installs Python dependencies.

## Why it matters

Without pip caching, every workflow run may download the same wheels and source distributions again. `actions/setup-python` can manage this cache directly with its `cache` input.

## How to fix

Add `cache: pip` to the setup-python step:

```yaml
- uses: actions/setup-python@v5
  with:
    python-version: "3.13"
    cache: pip
```

If the dependency file is not `requirements.txt`, set `cache-dependency-path` to the file used by the job.

## Examples

### Bad

```yaml
- uses: actions/setup-python@v5
- run: pip install -r requirements.txt
```

### Good

```yaml
- uses: actions/setup-python@v5
  with:
    cache: pip
- run: pip install -r requirements.txt
```

## Default severity

info

## Strict mode

No additional behavior in the MVP.

## False positive notes

The rule skips jobs that already use setup-python pip caching or an explicit `actions/cache` step for `~/.cache/pip`.
