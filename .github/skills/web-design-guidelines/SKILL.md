---
name: web-design-guidelines
description: Review UI code for Web Interface Guidelines compliance. Use when asked to "review my UI", "check accessibility", "audit design", "review UX", or "check my site against best practices".
metadata:
  author: vercel
  version: "1.0.0"
  argument-hint: <file-or-pattern>
---

# Web Interface Guidelines

Review files for compliance with Web Interface Guidelines.

## How It Works

1. Fetch the latest guidelines from the source URL below
2. Read the specified files (or prompt user for files/pattern)
3. Check against all rules in the fetched guidelines
4. Output findings in the terse `file:line` format

## Guidelines Source

Fetch fresh guidelines before each review using the WebFetch tool:

```
WebFetch("https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md")
```

The fetched content contains all the rules and output format instructions.

### Error Handling

- If the fetch fails (network error, 404, timeout), inform the user and suggest checking the URL or retrying.
- If the fetched content is empty or malformed, report the issue rather than proceeding with an incomplete ruleset.
- Cache the fetched guidelines for the duration of the session to avoid redundant requests when reviewing multiple files.

## Usage

When a user provides a file or pattern argument:

1. Fetch guidelines from the source URL above using WebFetch
2. Read the specified files
3. Apply all rules from the fetched guidelines
4. Output findings using the format specified in the guidelines

If no files specified, ask the user which files to review.

### Example

```
User: "review my UI src/components/*.tsx"

Step 1: WebFetch("https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md")
Step 2: Read all files matching src/components/*.tsx
Step 3: Check each file against the fetched rules
Step 4: Output findings as:
  src/components/Button.tsx:12 — Missing focus-visible outline (accessibility)
  src/components/Modal.tsx:45 — Dialog missing aria-label attribute
```
