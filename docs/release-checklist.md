# Release Checklist

## Before Tagging

- [ ] Update version in CLI project file.
- [ ] Update `CHANGELOG.md`.
- [ ] Run release build.
- [ ] Run tests.
- [ ] Pack tool.
- [ ] Install packed tool locally.
- [ ] Run sample scan.
- [ ] Verify README install instructions.
- [ ] Verify rule docs links.

## Commands

```bash
dotnet build GhaCacheDoctor.slnx --configuration Release
dotnet test GhaCacheDoctor.slnx --configuration Release --no-build
dotnet pack src/GhaCacheDoctor.Cli --configuration Release --no-build
dotnet tool install --tool-path .tmp/tools gha-cache-doctor --version 0.1.0 --add-source src/GhaCacheDoctor.Cli/bin/Release
.tmp/tools/gha-cache-doctor scan --path samples/github-actions/bad --fail-on none
```

## Tag

```bash
git tag v0.1.0
git push origin v0.1.0
```
