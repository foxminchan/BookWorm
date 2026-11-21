"use client";

import { useCallback, useState } from "react";

import { useRouter } from "next/navigation";

export interface UseLogoutResult {
  logout: () => Promise<void>;
  isLoggingOut: boolean;
}

export type SignOutFn = (options?: {
  fetchOptions?: { onSuccess?: () => void };
}) => Promise<unknown>;

export interface CreateUseLogoutOptions {
  signOut: SignOutFn;
  redirectTo?: string;
  refreshOnSignOut?: boolean;
  useFetchOptionsCallback?: boolean;
}

/**
 * Shared logout hook supporting optional loading state, navigation redirect,
 * router refresh, and Better Auth fetchOptions success callbacks.
 */
export function useAuthLogout({
  signOut,
  redirectTo = "/",
  refreshOnSignOut = false,
  useFetchOptionsCallback = false,
}: CreateUseLogoutOptions): UseLogoutResult {
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const router = useRouter();

  const logout = useCallback(async () => {
    setIsLoggingOut(true);

    try {
      if (useFetchOptionsCallback) {
        await signOut({
          fetchOptions: {
            onSuccess: () => {
              router.push(redirectTo);
            },
          },
        });
      } else {
        await signOut();
        router.push(redirectTo);
        if (refreshOnSignOut) {
          router.refresh();
        }
      }
    } catch (error) {
      console.error("Logout failed:", error);
    } finally {
      setIsLoggingOut(false);
    }
  }, [signOut, redirectTo, refreshOnSignOut, useFetchOptionsCallback, router]);

  return { logout, isLoggingOut };
}

/**
 * Normalizes a Better Auth `useSession` hook result into a consistent session state.
 */
export function useUserSession<TUser = unknown, TError = unknown>(
  useSessionHook: () => {
    data: { user?: TUser | null } | null;
    isPending?: boolean;
    error?: TError | null;
  },
): {
  user: TUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  error: TError | null;
} {
  const { data: session, isPending = false, error = null } = useSessionHook();

  return {
    user: session?.user ?? null,
    isLoading: isPending,
    isAuthenticated: !!session?.user,
    error,
  };
}
