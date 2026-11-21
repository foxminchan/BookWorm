"use client";

import { createAppAuthClient } from "@workspace/auth/client";

export const authClient = createAppAuthClient({
  defaultOrigin: "http://localhost:3000",
});

export const { signIn, useSession } = authClient;
