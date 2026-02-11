import { useCallback, useState } from "react";

import { useRouter } from "next/navigation";

import { authClient } from "@/lib/auth-client";

type UseLogoutReturn = {
  logout: () => Promise<void>;
  isLoggingOut: boolean;
};

export function useLogout(): UseLogoutReturn {
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const router = useRouter();

  const logout = useCallback(async () => {
    setIsLoggingOut(true);

    try {
      await authClient.signOut({
        fetchOptions: {
          onSuccess: () => {
            router.push("/");
          },
        },
      });
    } catch (error) {
      console.error("Logout failed:", error);
    } finally {
      setIsLoggingOut(false);
    }
  }, [router]);

  return { logout, isLoggingOut };
}
