import { createEntraAuth } from "@workspace/auth/server";

import { env } from "@/env.mjs";

export const auth = createEntraAuth({
  env,
  defaultBaseUrl: "http://localhost:3000",
  scopes: [
    "catalog_read",
    "basket_read",
    "basket_write",
    "ordering_read",
    "ordering_write",
    "rating_read",
    "rating_write",
    "chatting_read",
    "chatting_write",
  ],
});
