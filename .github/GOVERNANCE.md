# Governance

This document describes how the BookWorm project is governed and how decisions are made.

## Table of Contents

- [Governance](#governance)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Roles and Responsibilities](#roles-and-responsibilities)
    - [Maintainer](#maintainer)
    - [Committers](#committers)
    - [Contributors](#contributors)
  - [Decision-Making Process](#decision-making-process)
    - [Routine Changes](#routine-changes)
    - [Significant Changes](#significant-changes)
    - [Breaking Changes](#breaking-changes)
  - [Pull Request Policy](#pull-request-policy)
  - [Release Process](#release-process)
  - [Conflict Resolution](#conflict-resolution)
  - [Changes to This Document](#changes-to-this-document)

## Overview

BookWorm follows a **Benevolent Dictator For Life (BDFL)** governance model. The project is maintained by a single lead maintainer who has final authority over all technical and community decisions. Community contributors are welcome and their input is valued, though ultimate decisions rest with the maintainer.

## Roles and Responsibilities

### Maintainer

The lead maintainer is **Nhan Nguyen** ([@foxminchan](https://github.com/foxminchan)).

Responsibilities:

- Setting the technical direction and roadmap
- Reviewing and merging pull requests
- Triaging and responding to issues
- Managing releases and versioning
- Enforcing the [Code of Conduct](./CODE-OF-CONDUCT.md)
- Updating this governance document

### Committers

Committers are trusted contributors who have been granted write access to the repository. They may:

- Review and approve pull requests
- Triage issues and label them appropriately
- Participate in roadmap discussions

Committer status is granted at the discretion of the maintainer based on sustained, high-quality contributions.

### Contributors

Anyone who opens an issue, submits a pull request, improves documentation, or participates in discussions is a contributor. Contributions of all sizes are welcome.

To get started, see [CONTRIBUTING.md](./CONTRIBUTING.md).

## Decision-Making Process

### Routine Changes

Routine changes (bug fixes, documentation improvements, dependency updates, minor enhancements) are handled through the standard pull request process:

1. Open a pull request following the [contribution guidelines](./CONTRIBUTING.md).
2. At least one approval from the maintainer or a committer is required.
3. All CI checks must pass.
4. The maintainer merges the pull request.

### Significant Changes

Significant changes (new services, architectural changes, major new features, API redesigns) follow a more deliberate process:

1. Open a GitHub Discussion or an issue describing the proposed change.
2. Allow time for community feedback (at least one week for non-urgent items).
3. The maintainer evaluates feedback and makes a final decision.
4. An approved proposal proceeds through the normal pull request workflow.

### Breaking Changes

Breaking changes (removed APIs, incompatible protocol changes, renamed integration events) require:

1. A GitHub Discussion labelled `breaking-change` explaining the rationale and migration path.
2. Documentation of the migration path in the pull request description.
3. Updated EventCatalog schemas and OpenAPI specifications.
4. Explicit approval from the maintainer.

> [!CAUTION]
> Do not modify integration event namespaces or break Protobuf schemas without following the breaking-change process. The messaging system relies on consistent conventions for proper routing.

## Pull Request Policy

- All code changes must be submitted as pull requests — direct pushes to `main` are restricted.
- Pull request titles must follow [Conventional Commits](https://www.conventionalcommits.org/) format.
- Every pull request must pass the full CI pipeline (build, unit tests, contract tests, SonarQube, and security scans) before merging.
- PRs that are stale for more than 30 days without activity may be closed. They can be reopened at any time.

## Release Process

Releases follow [Semantic Versioning](https://semver.org/):

| Segment   | When to increment                                |
| --------- | ------------------------------------------------ |
| **MAJOR** | Incompatible API or integration contract changes |
| **MINOR** | New backward-compatible features                 |
| **PATCH** | Backward-compatible bug fixes                    |

Release notes are auto-generated from pull request labels using the configuration in [release.yml](./release.yml). The maintainer tags the release commit and publishes the GitHub Release.

## Conflict Resolution

If a disagreement arises over a technical or community matter:

1. Discuss the issue respectfully on the relevant GitHub issue or discussion thread.
2. If consensus cannot be reached, the maintainer makes the final call.
3. Code of Conduct violations are handled separately; see [CODE-OF-CONDUCT.md](./CODE-OF-CONDUCT.md).

## Changes to This Document

Changes to this governance document follow the same significant-change process described above and require an explicit pull request reviewed by the maintainer.
