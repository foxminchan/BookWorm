"use client";

import type { createAuthClient as CreateAuthClient } from "better-auth/react";
import { createAuthClient } from "better-auth/react";

export interface CreateAppAuthClientOptions {
  basePath?: string;
  defaultOrigin: string;
}

/** Creates a Better Auth React client configured for browser and SSR origins. */
export function createAppAuthClient({
  basePath = "",
  defaultOrigin,
}: CreateAppAuthClientOptions): ReturnType<typeof CreateAuthClient> {
  const normalizedPath = basePath
    ? `/${basePath.replace(/^\/+|\/+$/g, "")}`
    : "";

  return createAuthClient({
    baseURL:
      globalThis.window === undefined
        ? `${defaultOrigin}${normalizedPath}`
        : `${globalThis.location.origin}${normalizedPath}`,
  });
}
