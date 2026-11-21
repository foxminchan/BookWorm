"use client";

import type { createAuthClient as CreateAuthClient } from "better-auth/react";
import { createAuthClient } from "better-auth/react";

export interface CreateAppAuthClientOptions {
  basePath?: string;
  defaultOrigin: string;
}

/** Strips leading and trailing slashes and prefixes a single leading slash when non-empty. */
function normalizeBasePath(basePath: string): string {
  let start = 0;
  let end = basePath.length;
  while (start < end && basePath[start] === "/") {
    start++;
  }
  while (end > start && basePath[end - 1] === "/") {
    end--;
  }
  return start < end ? `/${basePath.slice(start, end)}` : "";
}

/** Creates a Better Auth React client configured for browser and SSR origins. */
export function createAppAuthClient({
  basePath = "",
  defaultOrigin,
}: CreateAppAuthClientOptions): ReturnType<typeof CreateAuthClient> {
  const normalizedPath = normalizeBasePath(basePath);

  return createAuthClient({
    baseURL:
      globalThis.window === undefined
        ? `${defaultOrigin}${normalizedPath}`
        : `${globalThis.location.origin}${normalizedPath}`,
  });
}
