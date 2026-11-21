"use client";

import { useCallback } from "react";

import { type SignOutFn, useAuthLogout } from "@workspace/auth/hooks";

import { authClient } from "@/lib/auth-client";

export function useLogout() {
  const signOut = useCallback<SignOutFn>(
    (options) => authClient.signOut(options),
    [],
  );

  return useAuthLogout({
    signOut,
    useFetchOptionsCallback: true,
  });
}
