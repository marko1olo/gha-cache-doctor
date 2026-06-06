# Rule Catalog

`gha-cache-doctor` rules focus on GitHub Actions cache correctness, performance, and maintainability.

| Rule | Description |
| --- | --- |
| [`GHA-CACHE001`](GHA-CACHE001-setup-node-cache-missing.md) | Detects `actions/setup-node` without dependency caching when installs are present. |
| [`GHA-CACHE002`](GHA-CACHE002-setup-node-cache-dependency-path-missing.md) | Detects missing `cache-dependency-path` in likely monorepos. |
| [`GHA-CACHE003`](GHA-CACHE003-actions-cache-key-missing-lockfile-hash.md) | Detects dependency cache keys that do not include lockfile hashes. |
| [`GHA-CACHE004`](GHA-CACHE004-restore-keys-too-broad.md) | Detects overly broad restore keys. |
| [`GHA-CACHE005`](GHA-CACHE005-install-step-without-cache.md) | Detects install steps that appear to run without a matching cache. |
| [`GHA-CACHE006`](GHA-CACHE006-gradle-cache-missing.md) | Detects Gradle build or test jobs that run before Gradle dependency caching is configured. |

New rules should include focused tests, a rule document, and a README table update.

For package-manager-specific key examples, see the [Cache Key Cookbook](../cache-key-cookbook.md).
