"use client";

import { createAuthClient } from "better-auth/react";

export const authClient = createAuthClient({
  baseURL:
    globalThis.window === undefined
      ? "http://localhost:3000"
      : globalThis.location.origin,
});

export const { signIn, useSession } = authClient;
