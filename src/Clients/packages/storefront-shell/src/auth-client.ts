"use client";

import { createAppAuthClient } from "@workspace/auth/client";

/**
 * Shared Better Auth client for customer-facing microfrontends (`storefront` and `commerce`).
 * Resolves to the unified gateway origin's `/api/auth` route owned by `storefront`.
 */
export const authClient = createAppAuthClient({
  defaultOrigin: "http://localhost:3000",
});

export const { signIn, signOut, useSession } = authClient;
