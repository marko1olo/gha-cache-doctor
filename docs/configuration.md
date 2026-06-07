# Configuration

`gha-cache-doctor` reads `.gha-cache-doctor.yml` or `.gha-cache-doctor.yaml` from the repository root by default.

Use `--config <path>` to load a specific file, or `--config none` to disable config loading.

## Example

```yaml
path: .github/workflows
format: text
failOn: warning
strict: true
include:
  - GHA-CACHE001
  - GHA-CACHE003
exclude:
  - GHA-CACHE004
severity:
  GHA-CACHE005: warning
```

## Fields

| Field | Description |
| --- | --- |
| `path` or `workflowPath` | Workflow file or directory to scan. |
| `format` | `text`, `json`, or `github-summary`. |
| `failOn` or `fail-on` | `none`, `info`, `warning`, or `error`. |
| `strict` | `true` or `false`. |
| `include` | Rule IDs to run. May be a YAML list or comma-separated string. |
| `exclude` | Rule IDs to skip. May be a YAML list or comma-separated string. |
| `disabledRules` | Alias for `exclude`. |
| `severity` or `severityOverrides` | Mapping of rule IDs to `info`, `warning`, or `error`. |

CLI options take precedence over config values. For example, `--exclude GHA-CACHE003` overrides a configured `exclude` list for that scan.
