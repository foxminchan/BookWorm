import { createEntraAuth } from "@workspace/utils/auth";

import { env } from "@/env.mjs";

export const auth = createEntraAuth({
  env,
  defaultBaseUrl: "http://localhost:3001",
  scopes: [
    "catalog_read",
    "catalog_write",
    "ordering_read",
    "ordering_write",
    "rating_read",
    "rating_write",
  ],
});
