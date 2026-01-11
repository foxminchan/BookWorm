"use client";

import { useCallback } from "react";

import { useRouter } from "next/navigation";

import { signOut } from "@/lib/auth-client";

export function useLogout() {
  const router = useRouter();

  const logout = useCallback(async () => {
    try {
      await signOut();
      router.push("/");
      router.refresh();
    } catch (error) {
      console.error("Logout failed:", error);
    }
  }, [router]);

  return { logout };
}
