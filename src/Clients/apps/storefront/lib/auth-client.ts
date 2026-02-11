"use client";

import { genericOAuthClient } from "better-auth/client/plugins";
import { createAuthClient } from "better-auth/react";

export const authClient = createAuthClient({
  baseURL:
    globalThis.window === undefined
      ? "http://localhost:3000"
      : globalThis.location.origin,
  plugins: [genericOAuthClient()],
});

export const { signIn, signOut, useSession } = authClient;
