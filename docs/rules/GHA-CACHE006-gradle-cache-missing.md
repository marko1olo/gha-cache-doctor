# GHA-CACHE006 gradle-cache-missing

## Summary

Reports jobs that run Gradle build or test tasks before configuring an obvious Gradle dependency cache.

## Why it matters

Gradle builds often download plugins, wrappers, and dependency artifacts on clean GitHub-hosted runners. Caching Gradle's local dependency directories can reduce repeated network downloads and shorten CI feedback loops.

## How to fix

Use one Gradle cache mechanism before running Gradle:

```yaml
- uses: actions/setup-java@v4
  with:
    distribution: temurin
    java-version: "21"
    cache: gradle
- run: ./gradlew build
```

`gradle/actions/setup-gradle` is also accepted:

```yaml
- uses: gradle/actions/setup-gradle@v4
- run: ./gradlew test
```

If a manual cache is required, cache Gradle directories with a key tied to Gradle dependency inputs:

```yaml
- uses: actions/cache@v4
  with:
    path: |
      ~/.gradle/caches
      ~/.gradle/wrapper
    key: ${{ runner.os }}-gradle-${{ hashFiles('**/*.gradle', '**/*.gradle.kts', '**/gradle.lockfile', 'gradle/wrapper/gradle-wrapper.properties') }}
- run: ./gradlew build
```

## Examples

### Bad

```yaml
- run: ./gradlew build
```

### Good

```yaml
- uses: actions/setup-java@v4
  with:
    distribution: temurin
    java-version: "21"
    cache: gradle
- run: ./gradlew build
```

## Default severity

info

## Strict mode

Strict mode does not change this rule.

## False positive notes

The rule intentionally uses a conservative heuristic. It reports steps that invoke `gradle` or `gradlew` with a `build` or `test` task, including common forms such as `./gradlew build`, `./gradlew test`, `gradle build`, `gradle --no-daemon test`, and module-qualified tasks such as `./gradlew :app:build`.

The rule does not run Gradle, inspect remote build scans, or infer custom tasks that may resolve dependencies indirectly. It only checks for a nearby Gradle cache in earlier steps of the same job.
