# Routing Reference

How files in the catalog's `pages/` folder map to URLs.

## The prefix

All custom pages are served under a URL prefix so they can never collide with EventCatalog's own routes. Configure it in `eventcatalog.config.js`:

```js
export default {
	// ...
	pages: {
		prefix: "custom", // default — pages/reports.astro → /custom/reports
	},
};
```

Rules (validated at startup, invalid values throw a config error):

- Defaults to `custom` when omitted.
- May contain letters, numbers, `-` and `_`, optionally separated by `/` (e.g. `internal/tools` is valid → pages served at `/internal/tools/...`).
- Cannot be empty.
- The first segment cannot be one of the reserved core routes: `api`, `api-catalog`, `architecture`, `auth`, `diagrams`, `directory`, `discover`, `docs`, `icepanel`, `miro`, `rss`, `schemas`, `settings`, `studio`, `unauthorized`, `visualiser`, `.well-known`.

## File-to-URL mapping

Given the default prefix `custom`:

| File in `pages/`         | URL                     | Type                                                           |
| ------------------------ | ----------------------- | -------------------------------------------------------------- |
| `reports.astro`          | `/custom/reports`       | page                                                           |
| `reports/index.astro`    | `/custom/reports`       | page (index collapses to parent)                               |
| `reports/[id].astro`     | `/custom/reports/:id`   | dynamic page                                                   |
| `docs/[...slug].astro`   | `/custom/docs/*`        | rest/catch-all page                                            |
| `api/stats.ts`           | `/custom/api/stats`     | endpoint                                                       |
| `api/teams/[id].ts`      | `/custom/api/teams/:id` | dynamic endpoint                                               |
| `homepage.astro`         | `/`                     | special: replaces the catalog homepage (not under the prefix)  |
| `_components/Card.astro` | —                       | never routed (underscore)                                      |
| `reports/_helpers.ts`    | —                       | never routed (underscore)                                      |
| `styles.css`, `logo.png` | —                       | not routed (served as static files at `/generated/pages/...`)  |
| `data.json`              | `/custom/data`          | picked up as a route — keep data files in `_`-prefixed folders |

Details:

- **Routable extensions**: `.astro` (pages), `.ts` / `.js` / `.mjs` (endpoints). `.json` files are picked up too, so keep JSON data in an `_`-prefixed folder (e.g. `pages/_data/teams.json`) and import it. Other files (images, CSS) aren't routed; they're served as static files at `/generated/pages/<path>`.
- **Underscore convention** (same as Astro): any file or directory whose name starts with `_` is never routable. Use it for colocated components, helpers, and shared data (`pages/_components/`, `pages/_lib/`).
- **Dynamic segments** use Astro syntax: `[param]` for one segment, `[...rest]` for catch-all. Access via `Astro.params` in pages or the `params` argument in endpoints.
  - In **static** output mode, dynamic `.astro` pages must export `getStaticPaths()` (standard Astro rule). In `server` mode they render on demand.
- **`homepage.astro`** is special-cased: it does not become `/custom/homepage`. It is rendered as the catalog's landing page at `/`, replacing the default dashboard homepage. It is already rendered inside the EventCatalog shell, so don't wrap it in `Layout`. It receives EventCatalog's components on `Astro.props.components`, including `CatalogDashboard` (the default dashboard, with optional `title` and `description`). Toolkit imports, Tailwind and partials work as in any other page. If `landingPage` is set in `eventcatalog.config.js`, `/` redirects there and the custom homepage isn't shown.

## Dev-server behavior

- **Adding or deleting** a file in `pages/` is picked up automatically — EventCatalog rewrites an internal routes manifest, which triggers a brief automatic dev-server restart (a few seconds). No manual restart needed.
- **Editing** an existing page hot-reloads instantly via the normal Astro/Vite pipeline.

## Availability

Custom pages are served for every catalog from `@eventcatalog/core` 5.0.0, with no plan or license key. (On 4.x they needed a Scale license and logged `Custom pages require the Scale plan` without one.) A `pages/api/` directory needs `output: 'server'`: static builds fail with a message rather than skipping the routes.
