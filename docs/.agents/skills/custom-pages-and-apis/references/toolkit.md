# Toolkit Reference

The `@catalog/*` aliases are EventCatalog's **stable public API** for custom pages. Internal aliases (`@components/*`, `@utils/*`, `@layouts/*`, `@features/*`) also resolve inside pages but are NOT a contract — never generate code that imports them.

## `@catalog/layouts/Layout.astro`

The EventCatalog page shell: header with search, vertical navigation rail, optional resource sidebar, theme support. Wrap every custom page in it unless the user explicitly wants a bare page.

```astro
---
import Layout from '@catalog/layouts/Layout.astro';
---

<Layout title="My Page" description="Optional meta description" sidebar={false}>
  <!-- page content -->
</Layout>
```

Props:

| Prop          | Type      | Default  | Description                                                                                                                                   |
| ------------- | --------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| `title`       | `string`  | required | Page title (browser tab + metadata)                                                                                                           |
| `description` | `string`  | —        | Meta description                                                                                                                              |
| `sidebar`     | `boolean` | `true`   | Render the resource sidebar (the one listing domains, services, messages). Set `false` for full-width pages like dashboards and landing pages |
| `showHeader`  | `boolean` | `true`   | Show the EventCatalog header (search, navigation)                                                                                             |
| `fullWidth`   | `boolean` | `false`  | Render content edge-to-edge without EventCatalog's page padding (core 4.12+)                                                                  |

Guidance: dashboards, team pages, and marketing-style pages usually read better with `sidebar={false}`. Documentation-adjacent pages can keep it. The Layout already adds page padding, so don't add your own outer `px-8 py-10` on top; use `fullWidth` when the page controls its own spacing.

Don't wrap `pages/homepage.astro` in `Layout`: the homepage is already rendered inside the shell.

## `@catalog/utils`

Cached, hydrated collection getters. All are async and return arrays of Astro content entries.

```ts
import {
	getServices,
	getEvents,
	getItemsFromCollectionByIdAndSemverOrLatest,
} from "@catalog/utils";
```

Available getters:

`getDomains`, `getServices`, `getSystems`, `getEvents`, `getCommands`, `getQueries`, `getFlows`, `getChannels`, `getEntities`, `getAgents`, `getContainers`, `getDataProducts`, `getAdrs`, `getTeams`, `getUsers`

Options (versioned getters):

```ts
const services = await getServices(); // all versions of every service
const latest = await getServices({ getAllVersions: false }); // latest version of each only
```

- `getAllVersions` (default `true`) — set `false` for "latest of each" — this is what most custom pages want.
- Results are cached in memory, so calling the same getter from many pages is cheap.
- `getTeams()` and `getUsers()` take no options (teams and users aren't versioned).

### Entry shape

Getters return Astro content collection entries. The useful data is on `.data`:

```ts
const events = await getEvents({ getAllVersions: false });

events[0].id; // collection entry id, e.g. "OrderCreated-1.0.0" ({id}-{version})
events[0].data.id; // resource id, e.g. "OrderCreated"
events[0].data.name; // display name
events[0].data.version; // e.g. "1.0.0"
events[0].data.summary; // short summary text
events[0].data.owners; // owner references: [{ id, collection: 'teams' | 'users' }]
events[0].data.latestVersion; // newest version of this resource
events[0].data.versions; // all versions
```

Messages (events/commands/queries) hydrate their producer/consumer service relationships; services hydrate their message relationships. Frontmatter fields from the resource's markdown are available on `.data`.

`owners` and a team's `members` are **references, not entries**: they only carry `id` and `collection`. To show names, look them up:

```ts
const [teams, users] = await Promise.all([getTeams(), getUsers()]);
const owners = new Map([...teams, ...users].map((o) => [o.data.id, o]));
const ownerNames = (event.data.owners ?? []).map((ref) => owners.get(ref.id)?.data.name ?? ref.id);
```

Link resources to their docs pages with `/docs/{collection}/{id}/{version}` (teams and users have no version: `/docs/teams/{id}`, `/docs/users/{id}`). Lists of resources are at `/discover/{type}`, e.g. `/discover/services`, `/discover/events`, `/discover/domains`.

```astro
<a href={`/docs/events/${event.data.id}/${event.data.version}`}>{event.data.name}</a>
```

### Resolving a specific version

```ts
import { getEvents, getItemsFromCollectionByIdAndSemverOrLatest } from "@catalog/utils";

const events = await getEvents();
// Semver range or exact version; 'latest' resolves the newest
const [match] = getItemsFromCollectionByIdAndSemverOrLatest(events, "OrderCreated", "1.x");
```

## `@catalog/components/*`

Resolves to the user's own `components/` folder at the catalog root (the same folder used for custom MDX components), then to `federated/components/` for components that come from federated catalogs (local ones win). Reusable across markdown docs AND custom pages:

```astro
---
import MyBanner from '@catalog/components/MyBanner.astro';
---
```

For components only used by custom pages, prefer colocating them in `pages/_components/` with relative imports instead — it keeps the page-specific code together and out of the MDX component namespace.

## Styling

- Tailwind CSS is available in all custom pages with no setup.
- Use EventCatalog theme CSS variables for colors so pages respect light/dark mode and custom themes. Syntax: `class="text-[rgb(var(--ec-page-text))]"`, with opacity: `[rgb(var(--ec-accent)/0.1)]`.

| Variable               | Usage                            |
| ---------------------- | -------------------------------- |
| `--ec-page-bg`         | Page/content background          |
| `--ec-page-text`       | Primary text                     |
| `--ec-page-text-muted` | Secondary/muted text             |
| `--ec-page-border`     | Borders and dividers             |
| `--ec-card-bg`         | Card/elevated surface background |
| `--ec-accent`          | Accent/brand color               |
| `--ec-accent-subtle`   | Light accent background          |
| `--ec-accent-text`     | Text on accent backgrounds       |

- Never use `dark:` Tailwind variants — theming is handled by the variables via the `data-theme` attribute.
