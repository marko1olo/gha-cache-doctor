# Cache Key Cookbook

Use cache keys that describe the dependency cache and change when the dependency graph changes. A practical key usually includes:

- `runner.os` so Linux, macOS, and Windows caches do not collide.
- The package manager or tool name.
- A project path or service name for monorepos.
- A lockfile hash, dependency manifest hash, or equivalent dependency input.

Avoid keys that are only `node-cache`, `deps`, `nuget`, or another static prefix. They restore quickly, but they can also restore stale or unrelated dependencies.

## npm

### Before

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.npm
    key: npm-cache
- run: npm ci
```

### After

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.npm
    key: ${{ runner.os }}-npm-${{ hashFiles('**/package-lock.json') }}
    restore-keys: |
      ${{ runner.os }}-npm-
- run: npm ci
```

For single-project Node workflows, `actions/setup-node` can own the cache:

```yaml
- uses: actions/setup-node@v4
  with:
    node-version: 22
    cache: npm
    cache-dependency-path: package-lock.json
- run: npm ci
```

## pnpm

### Before

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.pnpm-store
    key: pnpm
- run: pnpm install --frozen-lockfile
```

### After

```yaml
- uses: pnpm/action-setup@v4
  with:
    version: 9
- uses: actions/setup-node@v4
  with:
    node-version: 22
    cache: pnpm
    cache-dependency-path: pnpm-lock.yaml
- run: pnpm install --frozen-lockfile
```

For monorepos, point `cache-dependency-path` at every lockfile that can affect the install:

```yaml
cache-dependency-path: |
  pnpm-lock.yaml
  apps/*/pnpm-lock.yaml
```

## Yarn

### Before

```yaml
- uses: actions/cache@v4
  with:
    path: .yarn/cache
    key: yarn-cache
- run: yarn install --immutable
```

### After

```yaml
- uses: actions/setup-node@v4
  with:
    node-version: 22
    cache: yarn
    cache-dependency-path: yarn.lock
- run: yarn install --immutable
```

If you cache Yarn directories manually, include the lockfile hash and keep restore keys scoped:

```yaml
- uses: actions/cache@v4
  with:
    path: .yarn/cache
    key: ${{ runner.os }}-yarn-${{ hashFiles('yarn.lock') }}
    restore-keys: |
      ${{ runner.os }}-yarn-
```

## NuGet

### Before

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: nuget
- run: dotnet restore
```

### After

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json', '**/*.csproj', '**/*.props') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
- run: dotnet restore --locked-mode
```

Prefer `packages.lock.json` when the repository uses NuGet lock files. Include project files only when lock files are not present or when central package management files affect restore.

## Python

### Before

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.cache/pip
    key: pip-cache
- run: pip install -r requirements.txt
```

### After

```yaml
- uses: actions/setup-python@v5
  with:
    python-version: "3.12"
    cache: pip
    cache-dependency-path: |
      requirements.txt
      requirements-dev.txt
- run: pip install -r requirements.txt
```

For Poetry or uv, key off the lockfile used by the installer:

```yaml
cache-dependency-path: |
  poetry.lock
  pyproject.toml
```

## Gradle

### Before

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.gradle/caches
    key: gradle
- run: ./gradlew build
```

### After

```yaml
- uses: gradle/actions/setup-gradle@v4
- run: ./gradlew build --no-daemon
```

If a manual cache is required, scope it by OS and Gradle dependency inputs:

```yaml
- uses: actions/cache@v4
  with:
    path: |
      ~/.gradle/caches
      ~/.gradle/wrapper
    key: ${{ runner.os }}-gradle-${{ hashFiles('**/*.gradle', '**/*.gradle.kts', '**/gradle.lockfile', 'gradle/wrapper/gradle-wrapper.properties') }}
    restore-keys: |
      ${{ runner.os }}-gradle-
```

## Docker BuildKit

### Before

```yaml
- uses: docker/build-push-action@v6
  with:
    context: .
    push: false
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

### After

```yaml
- uses: docker/build-push-action@v6
  with:
    context: .
    push: false
    cache-from: type=gha,scope=web-${{ github.ref_name }}
    cache-to: type=gha,scope=web-${{ github.ref_name }},mode=max
```

BuildKit's GitHub Actions cache uses `scope` rather than an `actions/cache` key. Use a stable scope per image or service so independent Docker builds do not overwrite each other. Do not put secrets, tokens, or private repository names in the scope.

## Restore key checklist

- Keep restore keys narrower than the cache key, but still scoped by OS and package manager.
- In monorepos, include the app, package, or service path before falling back to a repository-wide prefix.
- Do not use a package-manager-only restore key such as `npm-`, `nuget-`, or `gradle-` unless unrelated dependency caches are acceptable.
- Never include secrets or credentials in a key. Cache keys and scopes can appear in logs.
