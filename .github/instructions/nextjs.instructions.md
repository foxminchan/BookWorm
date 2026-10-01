---
name: Next.js Frontend Style Guide
description: Frontend conventions for BookWorm's Bun, Turbo, and Next.js workspace
applyTo: "src/Clients/**/*.{ts,tsx,js,jsx}"
---

# Frontend Development

## Workspace rules

- Use the Bun workspace in [src/Clients](../../src/Clients) instead of `npm` or `pnpm`.
- Run commands from the client monorepo root, for example `bun run lint`, `bun run test`, and `bun run format`.
- Keep the repo free of conflicting lockfiles; do not introduce `package-lock.json`, `pnpm-lock.yaml`, or `yarn.lock` in this workspace.
- Prefer the existing Turbo structure: `apps/storefront`, `apps/backoffice`, and shared packages in `packages/*`.

## Delivery conventions

- Follow the existing monorepo boundaries; shared code belongs in a package, app-specific code belongs in the app.
- Prefer workspace packages such as `@workspace/*` over ad hoc duplicated utilities.
- Keep React components small and focused; move reusable logic into shared packages when it is used across apps.
- Preserve the established TypeScript strictness; avoid `any` and prefer typed interfaces and discriminated unions.
- Match the current project naming and folder patterns before introducing new conventions.

## Next.js and React guidance

- Follow the existing Next.js app-router patterns and route structure.
- Keep server and client boundaries explicit; do not mix server-only logic into browser components.
- Prefer idiomatic React 19 patterns and keep components accessible by default.
- Reuse shared UI primitives from the local packages instead of re-implementing common patterns.
- Guard against heavy client-side rendering when a server-side or cached pattern is already used elsewhere in the app.

## Quality bar

- Favor small, explainable changes over broad refactors.
- Maintain existing styling and import ordering patterns used by the repo.
- Validate with the smallest relevant frontend command, usually from the client monorepo root.
- When adding a feature, align the implementation with the surrounding app package and shared package conventions before introducing new abstraction layers.
