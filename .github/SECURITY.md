# Security Policy

## Supported Versions

Only the latest release series receives security updates.

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Scope

The following are considered in scope for security reports:

- Authentication and authorization bypasses (Keycloak integration)
- Injection vulnerabilities (SQL, command, prompt injection)
- Sensitive data exposure or PII leakage
- Insecure deserialization or message-bus exploits
- Supply-chain vulnerabilities in first-party code

The following are **out of scope**:

- Vulnerabilities in third-party dependencies that are already publicly known — please report those upstream
- Issues in infrastructure you manage yourself (your own Kafka cluster, PostgreSQL instance, etc.)
- Theoretical vulnerabilities without a working proof-of-concept

## Reporting a Vulnerability

**Do not disclose security vulnerabilities publicly as GitHub issues.**

Preferred method — use [GitHub Private Vulnerability Reporting](https://github.com/foxminchan/BookWorm/security/advisories/new). This keeps the report private and creates a shared draft advisory between you and the maintainer.

Alternatively, email [nguyenxuannhan407@gmail.com](mailto:nguyenxuannhan407@gmail.com) with:

1. A clear description of the vulnerability and its impact
2. Step-by-step reproduction instructions
3. Affected component(s) and version(s)
4. Any potential remediation ideas you may have

### What to Expect

| Stage                          | Timeline                                                                |
| ------------------------------ | ----------------------------------------------------------------------- |
| Initial acknowledgment         | Within 48 hours                                                         |
| Triage and severity assessment | Within 5 business days                                                  |
| Resolution or mitigation plan  | Varies by severity (critical: best-effort ASAP)                         |
| Public disclosure              | After a fix is released, or 90 days from report — whichever comes first |

We will keep you informed at each stage. If you do not receive acknowledgment within 48 hours, please follow up by email.

## Security Updates and Announcements

Security fixes are shipped as patch releases and documented in the [GitHub Release Notes](https://github.com/foxminchan/BookWorm/releases). For critical vulnerabilities, a GitHub Security Advisory will be published at the time of disclosure.
