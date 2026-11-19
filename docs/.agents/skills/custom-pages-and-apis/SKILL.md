---
name: custom-pages-and-apis
description: Builds custom pages and API routes inside an EventCatalog project. Creates Astro pages and TypeScript endpoints in the catalog's pages/ folder using the stable @catalog/layouts and @catalog/utils toolkit, wires them into the navigation, and sets up editor support. Use when user asks to "add a custom page to my catalog", "build a team page", "add an API endpoint to EventCatalog", "create a custom homepage", "add a custom route", "query catalog data from my own page", or "extend EventCatalog with my own pages".
license: MIT
metadata:
  author: eventcatalog
  version: "1.0.0"
---

# EventCatalog Custom Pages & APIs

Build user-defined pages and API routes that are served by EventCatalog itself — with full access to catalog data (domains, services, events, teams, ...) through a stable, documented toolkit.

Custom pages live in a `pages/` folder at the root of the catalog and are served under a configurable URL prefix (default `/custom`). API routes live in `pages/api/` and require SSR mode.

## Requirements (check these FIRST)

Before writing any files, verify both.

1. **Core version** — custom pages are served for every catalog from `@eventcatalog/core` **5.0.0** (no plan or license key needed). Versions 4.1.0–4.x only served them with a Scale license key, and the Layout's `fullWidth` prop needs 4.12 or later. Check the catalog's `package.json`, and suggest upgrading if it's on 4.x.
2. **SSR for API routes only** — `.astro` pages work in any output mode, but anything under `pages/api/` requires `output: "server"` in `eventcatalog.config.js`. A production build fails with a hard error if API routes exist in static mode (dev only warns). See [references/api-routes.md](references/api-routes.md).

## Instructions

### Step 1: Locate the catalog

Find the user's EventCatalog project (look for `eventcatalog.config.js`). Then check:

- Does `pages/` already exist? Read what's there — including `pages/homepage.astro`, which is a special file (it replaces the catalog homepage at `/`, not served under the prefix). Without one, the catalog opens on its built-in dashboard homepage.
- What is the configured prefix? Read `pages.prefix` from `eventcatalog.config.js` (default `custom`). All generated links must use it.
- What output mode is set (`output: "static"` or `"server"`)?
- Does the catalog have a `components/` folder? Files in it are importable as `@catalog/components/*`.
- Is `landingPage` set in `eventcatalog.config.js`? If so, `/` redirects there and a custom homepage is never shown.

### Step 2: Understand what the user wants to build

Common asks and where they land:

| User wants                 | File to create                 | URL                              |
| -------------------------- | ------------------------------ | -------------------------------- |
| A content/dashboard page   | `pages/reports.astro`          | `/custom/reports`                |
| A custom homepage          | `pages/homepage.astro`         | `/` (replaces default homepage)  |
| A section with an index    | `pages/tools/index.astro`      | `/custom/tools`                  |
| A dynamic page             | `pages/reports/[id].astro`     | `/custom/reports/:id`            |
| A JSON API endpoint        | `pages/api/stats.ts`           | `/custom/api/stats`              |
| A dynamic API endpoint     | `pages/api/teams/[id].ts`      | `/custom/api/teams/:id`          |
| Shared partials/components | `pages/_components/Card.astro` | never routed (underscore prefix) |

Full routing rules (extensions, index handling, underscore convention, reserved prefixes, prefix validation): [references/routing.md](references/routing.md).

**Custom homepage:** `pages/homepage.astro` is already rendered inside the EventCatalog shell, so don't wrap it in `Layout`. It receives EventCatalog's components on `Astro.props.components`, including `CatalogDashboard` (the default dashboard, with optional `title` and `description`), so a homepage can add to the dashboard rather than rebuild it:

```astro
---
const { CatalogDashboard } = Astro.props.components;
---

<div class="not-prose">
  <CatalogDashboard title="Acme Catalog" description="Our domains, systems, services and messages." />
</div>
```

### Step 3: Build pages with the toolkit

Always prefer the **stable public toolkit** over EventCatalog internals:

- `@catalog/layouts/Layout.astro` — the EventCatalog page shell (header, nav, optional resource sidebar). Props: `title` (required), `description?`, `sidebar?` (default `true`), `showHeader?` (default `true`), `fullWidth?` (default `false`; renders edge-to-edge without EventCatalog's page padding, for pages that control their own layout).
- `@catalog/utils` — cached collection getters: `getDomains`, `getServices`, `getSystems`, `getEvents`, `getCommands`, `getQueries`, `getFlows`, `getChannels`, `getEntities`, `getAgents`, `getContainers`, `getDataProducts`, `getAdrs`, `getTeams`, `getUsers`, plus `getItemsFromCollectionByIdAndSemverOrLatest` for semver resolution. Pass `{ getAllVersions: false }` to get only latest versions.
- `@catalog/components/*` — the user's own components from their catalog's `components/` folder.

Minimal page:

```astro
---
import Layout from '@catalog/layouts/Layout.astro';
import { getServices } from '@catalog/utils';

const services = await getServices({ getAllVersions: false });
---

<Layout title="Service Report" description="All services in our architecture" sidebar={false}>
  <div class="max-w-4xl mx-auto p-8">
    <h1 class="text-2xl font-bold text-[rgb(var(--ec-page-text))]">Services ({services.length})</h1>
    <ul>
      {services.map((service) => (
        <li>
          <a href={`/docs/services/${service.data.id}/${service.data.version}`}>{service.data.name}</a>
        </li>
      ))}
    </ul>
  </div>
</Layout>
```

Key conventions when generating page code:

- **Tailwind works out of the box.** For colors, use EventCatalog theme variables so pages respect light/dark mode: `text-[rgb(var(--ec-page-text))]`, `bg-[rgb(var(--ec-page-bg))]`, `border-[rgb(var(--ec-page-border))]`, `bg-[rgb(var(--ec-card-bg))]`, `text-[rgb(var(--ec-accent))]`. Never use `dark:` variants.
- Collection entries are Astro content entries: data is on `entry.data` (e.g. `entry.data.id`, `entry.data.name`, `entry.data.version`, `entry.data.summary`). Entry ids follow `{id}-{version}`.
- Link to catalog docs pages as `/docs/{collection}/{id}/{version}` (e.g. `/docs/services/OrderService/1.0.0`). Teams and users have no version: `/docs/teams/{id}`, `/docs/users/{id}`. Lists of resources live at `/discover/{type}` (e.g. `/discover/services`, `/discover/events`).
- `owners` and team `members` are references (`{ id, collection }`), not full entries. Look them up in `getTeams()` / `getUsers()` by id to show names.
- Put shared partials in `pages/_components/` and import them with relative paths.
- Do NOT import from EventCatalog internals (`@components/*`, `@utils/*`, `@layouts/*`). They resolve, but they are not a stable contract and can break on any release. Only `@catalog/*` is public API.

Full toolkit reference (all getter options, Layout props, entry shape): [references/toolkit.md](references/toolkit.md).
Complete working examples (external data fetch, dynamic routes, nav partials, API endpoints): [references/examples.md](references/examples.md).

### Step 4: Wire pages into the navigation

Pages are reachable by URL immediately, but users usually want them in the vertical navigation. Add a group to `navigation.groups` in `eventcatalog.config.js` — remembering that **providing `groups` replaces the entire default nav**, so include the built-ins the user still wants:

```js
navigation: {
  groups: [
    { id: 'main', items: [{ id: 'home' }, { id: 'docs' }] },
    { id: 'browse', label: 'Browse', items: [{ id: 'catalog' }, { id: 'schemas' }] },
    {
      id: 'my-pages',
      label: 'My Company',
      items: [
        { id: 'reports', label: 'Reports', href: '/custom/reports', icon: 'ChartBar' },
        { id: 'team', label: 'Our Team', href: '/custom/team', icon: 'UsersRound' },
      ],
    },
    { id: 'settings-group', position: 'bottom', items: [{ id: 'settings' }] },
  ],
}
```

- Built-in item ids: `home`, `docs`, `catalog`, `schemas`, `schema-insights`, `teams`, `users`, `settings`, `studio`.
- Custom items need `href`; `icon` is any [lucide-react](https://lucide.dev/icons) icon name (e.g. `Workflow`, `Github`, `ChartBar`).
- Use `match` (string or array) to keep an item highlighted on child routes, e.g. `match: '/custom/reports'`. Matching is a plain prefix check (no globs), so this also covers `/custom/reports/2024`.
- If the config has no `navigation.groups` yet and the user only wants one link added, ask before replacing the default nav — or skip nav wiring and just share the URL.

### Step 5: Editor support (recommended)

VS Code resolves imports from the nearest `tsconfig.json`, so without one the `@catalog/*` imports show "cannot find module" warnings (the build still works — aliases are resolved by EventCatalog at runtime). Create a `tsconfig.json` at the catalog root pointing at the toolkit inside the installed `@eventcatalog/core` package:

```jsonc
{
	// Editor support (IntelliSense, go-to-definition) for custom pages.
	// These aliases mirror the ones the EventCatalog build provides at runtime.
	"compilerOptions": {
		"baseUrl": ".",
		"allowJs": true,
		"jsx": "preserve",
		"paths": {
			"@catalog/components/*": ["components/*", "federated/components/*"],
			"@catalog/layouts/*": ["node_modules/@eventcatalog/core/eventcatalog/src/toolkit/layouts/*"],
			"@catalog/utils": ["node_modules/@eventcatalog/core/eventcatalog/src/toolkit/utils/index.ts"],
			"@catalog/utils/*": ["node_modules/@eventcatalog/core/eventcatalog/src/toolkit/utils/*"],
		},
	},
	"include": ["pages/**/*", "components/**/*"],
}
```

Note: older guides pointed these paths at `.eventcatalog-core/`, which EventCatalog no longer generates (4.12+). If the catalog has an old `tsconfig.json` with those paths, update it. Tell the user to reload the TS/Astro language server after adding this file.

### Step 6: Verify

1. Start the dev server (`npm run dev`) — or if it's already running, no restart is needed: EventCatalog watches the `pages/` folder and picks up new/removed files automatically (adding/removing a file triggers a brief automatic server restart; edits hot-reload).
2. Open the page at its prefixed URL (e.g. `http://localhost:3000/custom/reports`) and confirm it renders inside the EventCatalog shell.
3. For API routes, curl the endpoint and check the JSON response.
4. If a page 404s, check in order: Correct prefix in the URL? File has a routable extension (`.astro`, `.ts`, `.js`, `.mjs`)? Not under an underscore-prefixed folder?

## Troubleshooting

| Symptom                                                                                      | Cause                                                                                                                                         |
| -------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| All custom pages 404, warning `Custom pages require the Scale plan`                          | The catalog is on `@eventcatalog/core` 4.x without a Scale key — upgrade to 5.x, where custom pages are served for every catalog              |
| Build fails: API routes require `output: "server"`                                           | `pages/api/**` exists but config is static — set `output: "server"` or remove the API routes                                                  |
| Page 404s but others work                                                                    | Underscore-prefixed path segment, non-routable extension, or wrong prefix                                                                     |
| `Cannot find module '@catalog/utils'` in editor only                                         | Missing catalog-level `tsconfig.json` (Step 5); build is unaffected                                                                           |
| Config error: `pages.prefix ... is reserved`                                                 | Prefix collides with a core route — see reserved list in [references/routing.md](references/routing.md)                                       |
| Homepage unchanged after adding `pages/homepage.astro`                                       | `landingPage` is set in `eventcatalog.config.js` (it redirects `/` first), or you're looking under the prefix — the homepage is served at `/` |
| Homepage shows two headers / nested navigation                                               | `homepage.astro` is wrapped in `Layout` — remove it; the homepage is already rendered inside the shell                                        |
| Unexpected route or route error for a `.json` file (e.g. `pages/data.json` → `/custom/data`) | `.json` files in `pages/` are picked up as routes — keep data files in an `_`-prefixed folder (`pages/_data/`)                                |

## Reference Files

- [references/routing.md](references/routing.md) — file-to-URL mapping, prefix config and validation, reserved prefixes, underscore convention, homepage.astro
- [references/toolkit.md](references/toolkit.md) — `@catalog/layouts`, `@catalog/utils`, `@catalog/components` in full detail
- [references/api-routes.md](references/api-routes.md) — API endpoints, SSR requirement, response patterns
- [references/examples.md](references/examples.md) — complete working examples to adapt
