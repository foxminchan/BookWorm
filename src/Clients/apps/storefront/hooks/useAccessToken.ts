"use client";

import { useCallback, useEffect, useRef } from "react";

import { apiClient } from "@workspace/api-client/client";
import { AUTH } from "@workspace/utils/constants";

import { authClient, useSession } from "@/lib/auth-client";

/**
 * Fetches the Keycloak access token via Better Auth and registers it
 * with the shared API client so all outgoing requests include the
 * `Authorization: Bearer <token>` header.
 */
export function useAccessToken(): void {
  const { data: session } = useSession();
  const tokenRef = useRef<string | null>(null);

  const fetchToken = useCallback(async () => {
    if (!session?.user) {
      tokenRef.current = null;
      return;
    }

    try {
      const { data } = await authClient.getAccessToken({
        providerId: AUTH.PROVIDER,
      });

      tokenRef.current = data?.accessToken ?? null;
    } catch {
      tokenRef.current = null;
    }
  }, [session?.user]);

  useEffect(() => {
    apiClient.setTokenProvider(() => tokenRef.current);
  }, []);

  useEffect(() => {
    void fetchToken();
  }, [fetchToken]);
}
