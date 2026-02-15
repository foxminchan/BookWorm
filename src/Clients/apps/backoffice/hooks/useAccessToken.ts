"use client";

import { useCallback, useEffect } from "react";

import { apiClient } from "@workspace/api-client/client";
import { AUTH } from "@workspace/utils/constants";

import { authClient, useSession } from "@/lib/auth-client";

/**
 * Registers an async token provider with the shared API client so every
 * outgoing request includes an `Authorization: Bearer <token>` header.
 *
 * Leverages Better Auth's built-in token refresh — `getAccessToken` will
 * automatically refresh an expired token before returning, so no custom
 * expiration tracking or timer logic is required.
 */
export function useAccessToken(): void {
  const { data: session } = useSession();

  const getToken = useCallback(async (): Promise<string | null> => {
    if (!session?.user) {
      return null;
    }

    try {
      const { data } = await authClient.getAccessToken({
        providerId: AUTH.PROVIDER,
      });

      return data?.accessToken ?? null;
    } catch {
      return null;
    }
  }, [session?.user]);

  useEffect(() => {
    apiClient.setTokenProvider(getToken);
  }, [getToken]);
}
