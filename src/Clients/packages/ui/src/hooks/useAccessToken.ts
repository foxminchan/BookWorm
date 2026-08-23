"use client";

import { useCallback, useEffect } from "react";

import { apiClient } from "@workspace/api-client/client";
import { AUTH } from "@workspace/utils/constants";

/**
 * Minimal contract for the auth client's token retrieval method.
 * Only the subset of the Better Auth client used by this hook.
 */
type AuthClientLike = {
  listAccounts(): Promise<{
    data?: { id: string; providerId: string }[] | null;
  }>;
  getAccessToken(opts: {
    accountId: string;
  }): Promise<{ data?: { accessToken?: string } | null }>;
};

/**
 * Minimal contract for the session hook.
 * Mirrors the return shape of Better Auth's `useSession`.
 */
type UseSessionHook = () => { data: { user?: unknown } | null };

/**
 * Registers an async token provider with the shared API client so every
 * outgoing request includes an `Authorization: Bearer <token>` header.
 *
 * Leverages Better Auth's built-in token refresh — `getAccessToken` will
 * automatically refresh an expired token before returning, so no custom
 * expiration tracking or timer logic is required.
 *
 * @param authClient - The Better Auth client instance (app-specific).
 * @param useSessionHook - The `useSession` hook from the app's auth client.
 */
export function useAccessToken(
  authClient: AuthClientLike,
  useSessionHook: UseSessionHook,
): void {
  const { data: session } = useSessionHook();
  const isAuthenticated = !!session?.user;

  const getToken = useCallback(async (): Promise<string | null> => {
    if (!isAuthenticated) {
      return null;
    }

    try {
      const { data: accounts } = await authClient.listAccounts();
      const account = accounts?.find(
        (account) => account.providerId === AUTH.PROVIDER,
      );

      if (!account) {
        return null;
      }

      const { data } = await authClient.getAccessToken({
        accountId: account.id,
      });

      return data?.accessToken ?? null;
    } catch {
      return null;
    }
  }, [isAuthenticated, authClient]);

  useEffect(() => {
    apiClient.setTokenProvider(getToken);
  }, [getToken]);
}
