"use client";

import { createAppAuthClient } from "@workspace/auth/client";

export const authClient = createAppAuthClient({
  basePath: "/backoffice",
  defaultOrigin: "http://localhost:3001",
});

export const { signIn, signOut, useSession } = authClient;
