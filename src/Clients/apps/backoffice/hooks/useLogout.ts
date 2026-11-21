"use client";

import { useAuthLogout } from "@workspace/auth/hooks";

import { signOut } from "@/lib/auth-client";

export function useLogout() {
  return useAuthLogout({
    signOut,
    refreshOnSignOut: true,
  });
}
