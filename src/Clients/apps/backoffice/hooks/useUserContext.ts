"use client";

import { useSession } from "@/lib/auth-client";

export function useUserContext() {
  const { data: session, isPending, error } = useSession();

  return {
    user: session?.user ?? null,
    isLoading: isPending,
    isAuthenticated: !!session?.user,
    error,
  };
}
