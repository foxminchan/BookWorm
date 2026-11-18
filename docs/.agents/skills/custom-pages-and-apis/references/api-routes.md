# API Routes Reference

Custom API endpoints live in `pages/api/` inside the catalog and are served under the prefix, e.g. `pages/api/stats.ts` → `/custom/api/stats`.

## SSR requirement

API routes only work when the catalog runs in server mode:

```js
// eventcatalog.config.js
export default {
	// ...
	output: "server",
};
```

- **Production build**: if `pages/api/**` files exist and `output` is not `"server"`, the build fails with a hard error. Don't generate API routes for a static catalog without also flipping the output mode (ask the user first — server mode changes how they deploy).
- **Dev server**: endpoints are served regardless of output mode (Astro dev always runs a server), but a warning is printed. Don't let a user ship thinking it works because dev worked.
- Regular `.astro` pages work in both static and server output.

## Writing endpoints

Endpoints are standard Astro API routes: export HTTP-method functions (`GET`, `POST`, `PUT`, `DELETE`, `ALL`) that return a `Response`. The full toolkit (`@catalog/utils`) is importable.

```ts
// pages/api/stats.ts → GET /custom/api/stats
import type { APIRoute } from "astro";
import { getEvents, getServices, getDomains } from "@catalog/utils";

export const GET: APIRoute = async () => {
	const [events, services, domains] = await Promise.all([
		getEvents({ getAllVersions: false }),
		getServices({ getAllVersions: false }),
		getDomains({ getAllVersions: false }),
	]);

	return Response.json({
		events: events.length,
		services: services.length,
		domains: domains.length,
	});
};
```

Dynamic segments come through `params`:

```ts
// pages/api/teams/[id].ts → GET /custom/api/teams/:id
import type { APIRoute } from "astro";
import { getTeams } from "@catalog/utils";

export const GET: APIRoute = async ({ params }) => {
	const teams = await getTeams();
	const team = teams.find((t) => t.data.id === params.id);

	if (!team) {
		return Response.json({ error: `Team not found: ${params.id}` }, { status: 404 });
	}

	return Response.json({
		id: team.data.id,
		name: team.data.name,
		members: (team.data.members ?? []).map((m) => m.id),
	});
};
```

Conventions:

- Return `Response.json(...)` (or `new Response(...)` for non-JSON).
- For errors, return a JSON error object with the right status code — don't throw: `Response.json({ error: '...' }, { status: 404 })`.
- Read request bodies in `POST`/`PUT` handlers via `await request.json()` (validate with Zod if the catalog has it available).

## Authentication

If the catalog has EventCatalog authentication configured (SSR feature), the auth middleware covers custom pages and API routes automatically — unauthenticated requests are redirected/rejected the same as core routes. No extra wiring is needed, but remind users that programmatic clients calling their custom APIs need to authenticate like any other catalog request.
