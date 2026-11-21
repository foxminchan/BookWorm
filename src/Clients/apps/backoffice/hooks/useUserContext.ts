"use client";

import { useUserSession } from "@workspace/auth/hooks";

import { useSession } from "@/lib/auth-client";

export function useUserContext() {
  return useUserSession(useSession);
}
