"use client";

import type { createAuthClient as CreateAuthClient } from "better-auth/react";
import { createAuthClient } from "better-auth/react";

export const authClient: ReturnType<typeof CreateAuthClient> = createAuthClient(
  {
    baseURL:
      globalThis.window === undefined
        ? "http://localhost:3001"
        : globalThis.location.origin,
  },
);

export const { signIn, signOut, useSession } = authClient;
