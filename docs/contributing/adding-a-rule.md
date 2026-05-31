# Adding a Rule

Rules are small deterministic checks that analyze a parsed GitHub Actions workflow together with repository context.

## Where Rules Live

Rule classes live under:

```text
src/GhaCacheDoctor.GitHubActions/Rules
```

Register new rules in:

```text
src/GhaCacheDoctor.GitHubActions/Rules/GitHubActionsRules.cs
```

## Rule Interface

Rules implement `GhaCacheDoctor.Core.IRule`:

```csharp
using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class ExampleRule : IRule
{
    public string Id => "GHA-CACHE999";
    public string Title => "example-rule";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(
        WorkflowDocument workflow,
        RepositoryContext repository,
        bool strictMode = false)
    {
        return [];
    }
}
```

## Choosing Severity And Category

Use conservative defaults:

- `Info`: performance suggestions and low-risk improvements.
- `Warning`: likely correctness problems or stale-cache risks.
- `Error`: reserved for future checks where cache configuration is very likely invalid.

Common categories:

- `performance`
- `correctness`
- `maintainability`

## Tests

Add focused tests under:

```text
tests/GhaCacheDoctor.GitHubActions.Tests
```

At minimum, include:

- one test that reports the intended finding,
- one test that avoids a false positive,
- one strict-mode test if the rule changes behavior in strict mode.

## Docs

Add a rule document under:

```text
docs/rules
```

Use this structure:

```md
# GHA-CACHE999 example-rule

## Summary

## Why it matters

## How to fix

## Examples

### Bad

### Good

## Default severity

## Strict mode

## False positive notes
```

Update the README rules table when adding a public rule.

## Validation

Run:

```bash
dotnet build GhaCacheDoctor.slnx
dotnet test GhaCacheDoctor.slnx
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --fail-on none
```
