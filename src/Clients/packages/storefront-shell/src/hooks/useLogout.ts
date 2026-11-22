"use client";

import { useCallback } from "react";

import { type SignOutFn, useAuthLogout } from "@workspace/auth/hooks";

import { authClient } from "../auth-client";

/**
 * Signs the customer out via Better Auth and performs a full browser navigation
 * to `/` so logout works reliably across microfrontend zones (`storefront` and `commerce`).
 */
export function useLogout() {
  const signOut = useCallback<SignOutFn>(
    (options) => authClient.signOut(options),
    [],
  );

  return useAuthLogout({
    signOut,
    redirectTo: "/",
    useFetchOptionsCallback: true,
    hardNavigate: true,
  });
}
